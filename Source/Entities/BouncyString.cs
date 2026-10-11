using System;
using System.Collections;
using System.Linq;
using System.Runtime.CompilerServices;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Graphics.PackedVector;
using Monocle;
using MonoMod;

namespace Celeste.Mod.audiohelper.Entities;

[CustomEntity("audiohelper/BouncyString")]
[Tracked]
public class BouncyString : JumpThru
{
    // Main Logic
    public float startY, intendedY, midpointX;
    public bool canLaunch;
    public enum States { wait, bounce };
    public States state = States.wait;
    public enum WireStates { still, pinch, beginRing, ringing }
    public WireStates wireState;
    public Vector2 focus, renderFocus;
    private InvisibleWall leftWall, rightWall;

    // Rendering
    public float afterImageCount = 0, afterImageTimer;
    public int maxAfterImages = 5, afterImageOffset = 3;
    public bool increaseAfterImageCount = false;
    public int bufferHead = 0, bufferLength;
    public Vector2[][] pastWirePoints;
    public float wireTransformProgress = 0;
    public Vector2[] wirePoints;
    public enum Styles { body, highlight, shadow }
    public Styles style;
    public MTexture wireCap;

    // Bounce logic
    public float speed, accel, baseSpeed, liftBoost, autolaunch, ringTimer, ringTimerMax; // variables that get init'd per-bounce
    private readonly float speedCap = 400;
    private readonly float minDistance = 8, maxDistance = 64;
    private readonly float minSpeed = 100, maxSpeed = 300;
    private readonly float minAccel = 1000, maxAccel = 2100;
    private readonly float minLiftboost = 0, maxLiftboost = -130;
    private readonly float minAutoLaunch = -150, maxAutoLaunch = - 200;
    private readonly float minRingTimer = 1f, maxRingTimer = 5f;
    public BouncyString(EntityData data, Vector2 offset) : base(data.Position + offset, data.Width, safe: false)
    {
        Position.Y += 4;
        startY = Position.Y;
        intendedY = Position.Y;
        midpointX = Position.X + (Width / 2);

        focus.X = midpointX;
        focus.Y = Position.Y;
        renderFocus = focus;

        int numWirePoints = (int)(Width / 32);
        numWirePoints = Calc.Clamp(numWirePoints, 0, 8);
        wirePoints = new Vector2[4 + numWirePoints];
        wirePoints[0] = Position;
        wirePoints[^1] = Position + Vector2.UnitX * Width;

        bufferLength = maxAfterImages * afterImageOffset;
        pastWirePoints = new Vector2[bufferLength][];
        for(int i = 0; i < bufferLength; i++) pastWirePoints[i] = wirePoints;

        SurfaceSoundIndex = 0;
        Collider = new PlayerOnlyColliderList(new Hitbox(Width, 5f));

        wireCap = GFX.Game["objects/audiohelper/bouncystring/wirecap"];
    }
    public override void Added(Scene scene)
    {
        base.Added(scene);

        scene.Add(leftWall = new InvisibleWall(Position - Vector2.UnitX));
        scene.Add(rightWall = new InvisibleWall(Position + Vector2.UnitX * Width));
    }
    public override void Update()
    {
        base.Update();

        Player playerRider = GetPlayerRider();
        StateMachine(playerRider);
        WireStateMachine(playerRider);
        SetPlatformHeight();
    }
    public void StateMachine(Player playerRider)
    {
        switch (state)
        {
            case States.wait:
                if (playerRider is not null)
                {
                    InitializeBounce(playerRider);
                    intendedY = renderFocus.Y;
                    state = States.bounce;
                    wireState = WireStates.pinch;
                }
                break;
            case States.bounce:
                speed += accel * Engine.DeltaTime * (intendedY <= startY ? 1 : -1);
                CapSpeed();

                if (intendedY > startY) canLaunch = true;

                // ends bounce state
                if (speed > 0.5f * -baseSpeed && intendedY < startY && canLaunch)
                {
                    // launches the player automatically
                    if (playerRider is not null && playerRider.Speed.Y >= 0) playerRider.Speed.Y = autolaunch;
                    state = States.wait;
                    wireState = WireStates.beginRing;
                    intendedY = startY;
                }
                else intendedY += speed * Engine.DeltaTime;
                break;
        }
    }
    public void WireStateMachine(Player playerRider)
    {
        bool getPoints = true; ;
        switch (wireState)
        {
            case WireStates.still:
            default:
                getPoints = false;
                break;
            case WireStates.pinch:
                if (playerRider is not null) focus.X = playerRider.CenterX;
                else wireState = WireStates.beginRing;

                focus.Y = intendedY;
                renderFocus = focus;

                break;
            case WireStates.beginRing:
                increaseAfterImageCount = true;
                PlayNote();

                focus.Y += speed * Engine.DeltaTime;
                renderFocus = focus;

                //if (accel < 0) accel = -2 * maxAccel;
                //else accel = 2 * maxAccel;

                wireState = WireStates.ringing;
                break;
            case WireStates.ringing:
                AdjustAfterImageCount();
                wireTransformProgress = Calc.Approach(wireTransformProgress, 1, 0.5f * Engine.DeltaTime);

                if (ringTimer <= 0)
                {
                    focus.Y = startY;
                    focus.X = midpointX;
                    wireState = WireStates.still;
                    speed = 0f;
                    increaseAfterImageCount = false;
                    afterImageCount = 0;
                    wireTransformProgress = 0;
                }

                focus.X = Calc.Approach(focus.X, midpointX, 50 * Engine.DeltaTime);
                speed += accel * Engine.DeltaTime * (focus.Y <= startY ? 1 : -1);
                CapSpeed();
                focus.Y += speed * Engine.DeltaTime;

                // set renderFocus
                renderFocus.Y = startY + (focus.Y - startY) * (ringTimer * ringTimer) / (ringTimerMax * ringTimerMax);
                renderFocus.X = focus.X;

                ringTimer -= Engine.DeltaTime;
                break;
        }
        if (getPoints) GetPoints();
    }
    public void AdjustAfterImageCount()
    {
        if (afterImageCount == maxAfterImages) increaseAfterImageCount = false;

        if (increaseAfterImageCount) afterImageCount = Calc.Approach(afterImageCount, maxAfterImages, 20 * Engine.DeltaTime);
        else if (afterImageTimer > 0) afterImageTimer -= Engine.DeltaTime;
        else afterImageCount = Calc.Approach(afterImageCount, 0, 2 * Engine.DeltaTime);
    }
    public void SetPlatformHeight()
    {
        Player player = Scene.Tracker.GetEntity<Player>();

        if (player is null)
        {
            MoveToY(startY);
            UpdateWall(player, leftWall, false);
            UpdateWall(player, rightWall, false);
        }
        else
        {
            if (intendedY > startY)
            {
                MoveToY(intendedY, (speed < 0f) ? liftBoost : speed);
            }
            else if (player.Left > Position.X + Width || player.Right < Position.X)
            {
                MoveToY(startY);
            }
            else
            {
                Vector2 pivotOffset;
                float percent;

                pivotOffset.X = renderFocus.X - (Position.X + (player.CenterX <= renderFocus.X ? 0 : Width));
                pivotOffset.Y = renderFocus.Y - startY;

                if (pivotOffset.X == 0) percent = 1;
                else percent = (player.CenterX - (Position.X + (player.CenterX <= renderFocus.X ? 0 : Width))) / pivotOffset.X;

                percent = Calc.Clamp(percent, 0f, 1f);
                float platformOffset = pivotOffset.Y * percent;

                MoveToY(startY + platformOffset, (speed < 0f) ? liftBoost : speed);
            }

            if (player.Bottom > startY && player.Bottom <= Position.Y && player.Left >= Position.X && player.Right <= Position.X + Width)
            {
                if (Scene.CollideCheck<Solid>(new Vector2(Position.X - 1, startY))) UpdateWall(player, leftWall, true);
                else UpdateWall(player, leftWall, false);

                if (Scene.CollideCheck<Solid>(new Vector2(Position.X + Width, startY))) UpdateWall(player, rightWall, true);
                else UpdateWall(player, rightWall, false);
            }
            else
            {
                UpdateWall(player, leftWall, false);
                UpdateWall(player, rightWall, false);
            }
        }
    }
    private void UpdateWall(Player player, InvisibleWall wall, bool on)
    {
        wall.Collidable = on;
        wall.MoveToY(on ? player.Top : startY);
        wall.Collidable = on;
        wall.MoveToY(on ? player.Top : startY);
    }
    private void CapSpeed()
    {
        if (speed > speedCap) speed = speedCap;
        else if (speed < -speedCap) speed = -speedCap;
    }
    public void InitializeBounce(Player player)
    {
        float left = player.Left - Position.X;
        float right = Position.X + Width - player.Right;
        float distance = MathF.Min(left, right);

        speed = Calc.ClampedMap(distance, minDistance, maxDistance, minSpeed, maxSpeed);
        accel = Calc.ClampedMap(distance, minDistance, maxDistance, minAccel, maxAccel);
        liftBoost = Calc.ClampedMap(distance, minDistance, maxDistance, minLiftboost, maxLiftboost);
        autolaunch = Calc.ClampedMap(distance, minDistance, maxDistance, minAutoLaunch, maxAutoLaunch);
        ringTimer = Calc.ClampedMap(distance, minDistance, maxDistance, minRingTimer, maxRingTimer);

        baseSpeed = speed;
        ringTimerMax = ringTimer;
        afterImageTimer = ringTimerMax / 4;

        canLaunch = false;
        wireTransformProgress = 0;
    }
    public void PlayNote()
    {

    }
    public void GetPoints()
    {
        int length = wirePoints.Length;
        int halfLengthHigh = length / 2;
        Vector2 startVector = wirePoints[0];
        Vector2 endVector = wirePoints[^1];
        Vector2 focusLeftOffset = (renderFocus - Vector2.UnitX * 3) - startVector;
        Vector2 focusRightOffset = (renderFocus + Vector2.UnitX * 3) - endVector;

        for (int i = 1; i <= length - 2; i++)
        {
            if (i < halfLengthHigh - 1)
            {
                float ratio = (float)i / halfLengthHigh;
                Vector2 offsetVector = focusLeftOffset * ratio;

                offsetVector.Y += offsetVector.Y * wireTransformProgress * (1 - (ratio * ratio));

                wirePoints[i] = offsetVector + startVector;
            }
            else if (i == halfLengthHigh - 1)
            {
                wirePoints[i] = focusLeftOffset + startVector;
            }
            else if (i == halfLengthHigh)
            {
                wirePoints[i] = focusRightOffset + endVector;
            }
            else
            {
                float ratio = (float)(length - 1 - i) / halfLengthHigh;
                Vector2 offsetVector = focusRightOffset * ratio;

                offsetVector.Y += offsetVector.Y * wireTransformProgress * (1 - (ratio * ratio));

                wirePoints[i] = offsetVector + endVector;
            }
        }

        if (Scene.OnInterval(1f / 60))
        {
            pastWirePoints[bufferHead] = wirePoints.ToArray();
            bufferHead++;
            bufferHead %= bufferLength;
        }
    }
    public override void Render()
    {
        base.Render();

        int intCount = (int)afterImageCount;

        for (int image = intCount; image >= 0; image--)
        {
            float alpha;
            Vector2[] localPoints;

            if (image == 0)
            {
                alpha = 1;
                localPoints = wirePoints;
            }
            else
            {
                alpha = (intCount - image) / (2f * intCount);
                int index = (bufferHead - (afterImageOffset * image) + bufferLength) % bufferLength;
                localPoints = pastWirePoints[index];
            }

            DrawLineStyles(alpha, localPoints, Styles.body);
            DrawLineStyles(alpha, localPoints, Styles.highlight);
            DrawLineStyles(alpha, localPoints, Styles.shadow);

            wireCap.DrawCentered(new Vector2(Position.X, startY));
            wireCap.DrawCentered(new Vector2(Position.X + Width, startY), Color.White, new Vector2(-1,1));
        }
    }
    public void DrawLineStyles(float alpha, Vector2[] localPoints, Styles style)
    {
        float heightOffset;
        Color colour;
        int thickness;
        switch (style)
        {
            case Styles.body:
            default:
                heightOffset = 0.5f;
                colour = Calc.HexToColor("b38374");
                thickness = 2;
                break;
            case Styles.highlight:
                heightOffset = -0.5f;
                colour = Calc.HexToColor("e2b2a0");
                thickness = 1;
                break;
            case Styles.shadow:
                heightOffset = 1.5f;
                colour = Calc.HexToColor("3b1a17");
                thickness = 1;
                break;
        }
        Color alphaColour = colour * alpha;
        DrawLines(heightOffset, alphaColour, thickness, localPoints);
    }
    public void DrawLines(float heightOffset, Color colour, int thickness, Vector2[] localPoints)
    {
        if (wireState == WireStates.still)
        {
            Draw.Line(Position.X, startY + heightOffset, Position.X + Width, startY + heightOffset, colour, thickness);
        }
        else
        {
            for (int i = 1; i < localPoints.Length; i++)
            {
                Draw.Line(localPoints[i-1].X, localPoints[i - 1].Y + heightOffset, localPoints[i].X, localPoints[i].Y + heightOffset, colour, thickness);
            }
        }
    }
    private class PlayerOnlyColliderList : ColliderList
    {
        public PlayerOnlyColliderList(params Collider[] colliders)
        {
            this.colliders = colliders;
        }

        public override bool Collide(Hitbox hitbox) => hitbox.Entity is Player && base.Collide(hitbox);
        public override bool Collide(Circle hitbox) => false;
        public override bool Collide(Grid hitbox) => false;
        public override bool Collide(ColliderList hitbox) => false;
    }
    private class InvisibleWall : Solid
    {
        public InvisibleWall(Vector2 position) : base(position, 1, 8, safe: false)
        {
            Collider = new PlayerOnlyColliderList(new Hitbox(1,8));
            SurfaceSoundIndex = 0;
            Add(new ClimbBlocker(edge: true));
        }

        //public override void Render()
        //{
        //    base.Render();
        //    if (Collidable) Draw.Rect(Position, 1, 8, Color.Red);
        //}
    }
}