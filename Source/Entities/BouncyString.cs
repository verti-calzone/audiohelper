using System;
using System.Collections;
using System.Runtime.CompilerServices;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics.PackedVector;
using Monocle;
using MonoMod;

namespace Celeste.Mod.audiohelper.Entities;

[CustomEntity("audiohelper/BouncyString")]
[Tracked]
public class BouncyString : JumpThru
{
    public float startY, intendedY;
    public enum States { wait, bounce };
    public States state = States.wait;
    public enum WireStates { still, pinch, beginRing, ringing }
    public WireStates wireState;
    public float midPointX, focusSpeedX = 0;
    public Vector2 focus, renderFocus;
    public float virtualFocusY;

    public float afterImages = 0;
    public int maxAfterImages = 5, afterImageOffset = 2;
    public Vector2[] wirePoints;
    public int bufferHead = 0, bufferLength;
    public Vector2[] pastFoci;

    public enum Styles { body, highlight, shadow }
    public Styles style;

    public float speed, accel, baseSpeed, liftBoost, autolaunch, ringTimer, ringTimerMax; // variables that get init'd per-bounce

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
        midPointX = Position.X + (Width / 2);

        focus.X = midPointX;
        focus.Y = Position.Y;
        renderFocus = focus;
        bufferLength = maxAfterImages * afterImageOffset;
        pastFoci = new Vector2[bufferLength];

        for(int i = 0; i < bufferLength; i++) pastFoci[i] = renderFocus;

        wirePoints = new Vector2[(int)MathF.Ceiling(Width/16)];

        SurfaceSoundIndex = 0;

        Collider = new PlayerOnlyColliderList(new Hitbox(Width, 5f));
    }

    public override void Update()
    {
        base.Update();

        Player playerRider = GetPlayerRider();
        StateMachine(playerRider);
        WireStateMachine(playerRider);
        SetPlatformHeight();

        if (Scene.OnInterval(1f / 60))
        {
            pastFoci[bufferHead] = renderFocus;
            bufferHead++;
            bufferHead %= bufferLength;
        }
    }

    public void StateMachine(Player playerRider)
    {
        switch (state)
        {
            case States.wait:
                if (playerRider is not null)
                {
                    InitializeBounce(playerRider);
                    state = States.bounce;
                    wireState = WireStates.pinch;
                }
                break;
            case States.bounce:
                speed += accel * Engine.DeltaTime * (intendedY <= startY ? 1 : -1);

                // ends bounce state
                if (speed > 0.5f * -baseSpeed && intendedY < startY)
                {
                    // launches the player automatically
                    if (playerRider is not null && playerRider.Speed.Y >= 0)
                    {
                        playerRider.Speed.Y = autolaunch;
                    }
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
        switch (wireState)
        {
            case WireStates.still:
                break;
            case WireStates.pinch:
                if (playerRider is not null) focus.X = playerRider.CenterX;
                else wireState = WireStates.beginRing;

                focus.Y = intendedY;
                renderFocus = focus;

                break;
            case WireStates.beginRing:
                afterImages = maxAfterImages;

                // TODO: play sfx

                focus.Y += speed * Engine.DeltaTime;
                renderFocus = focus;

                wireState = WireStates.ringing;
                break;
            case WireStates.ringing:
                afterImages = (maxAfterImages * ringTimer * ringTimer) / (ringTimerMax * ringTimerMax);

                // ringing stops
                if (ringTimer <= 0)
                {
                    focus.Y = startY;
                    focus.X = midPointX;
                    wireState = WireStates.still;
                    speed = 0f;
                    afterImages = 0;
                }

                // move focus X
                focus.X = Calc.Approach(focus.X, midPointX, 200 * Engine.DeltaTime);

                // move focus Y
                speed += accel * Engine.DeltaTime * (focus.Y <= startY ? 1 : -1);
                focus.Y += speed * Engine.DeltaTime;

                // set renderFocus
                renderFocus.Y = startY + (focus.Y - startY) * (ringTimer * ringTimer * ringTimer) / (ringTimerMax * ringTimerMax * ringTimerMax);
                renderFocus.X = focus.X;

                ringTimer -= Engine.DeltaTime;

                break;
        }
        //afterImages = Calc.Approach(afterImages, (increaseAfterImageCount ? 5 : 0),10 * Engine.DeltaTime);
    }

    public void SetPlatformHeight()
    {
        if (intendedY >= startY)
        {
            MoveToY(intendedY, (speed < 0f) ? liftBoost : speed);
        }
        else
        {
            Player player = Scene.Tracker.GetEntity<Player>();
            if (player.Left > Position.X + Width || player.Right < Position.X)
            {
                MoveToY(startY);
            }
            else
            {
                Vector2 pivotOffset;
                pivotOffset.X = focus.X - (Position.X + (player.CenterX <= focus.X ? 0 : Width));
                pivotOffset.Y = focus.Y - startY;
                float percent = (player.CenterX - (Position.X + (player.CenterX <= focus.X ? 0 : Width))) / pivotOffset.X;
                float platformOffset = pivotOffset.Y * percent;
                MoveToY(startY + platformOffset, (speed < 0f) ? liftBoost : speed);
            }
        }
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
    }

    public override void Render()
    {
        base.Render();

        for (int image = (int)afterImages; image >= 0; image--)
        {
            float alpha;
            Vector2 localFocus;

            if (image == 0)
            {
                alpha = 1;
                localFocus = renderFocus;
            }
            else
            {
                alpha = image / (4 * afterImages);
                int index = (bufferHead - (afterImageOffset * image) + bufferLength) % bufferLength;
                localFocus = pastFoci[index];
            }

            foreach (Styles style in Enum.GetValues<Styles>())
            {
                DrawLineStyles(alpha, localFocus, style);
            }
        }
    }

    public void DrawLineStyles(float alpha, Vector2 localFocus, Styles style)
    {
        float heightOffset;
        Color colour;
        int thickness;
        switch (style)
        {
            // body
            case Styles.body:
            default:
                heightOffset = 0.5f;
                colour = Calc.HexToColor("b38374");
                thickness = 2;
                break;
            // highlight
            case Styles.highlight:
                heightOffset = -0.5f;
                colour = Calc.HexToColor("e2b2a0");
                thickness = 1;
                break;
            // shadow
            case Styles.shadow:
                heightOffset = 1.5f;
                colour = Calc.HexToColor("3b1a17");
                thickness = 1;
                break;
        }
        Color alphaColour = colour * alpha;
        DrawLines(heightOffset, alphaColour, thickness, localFocus);
    }

    public void UpdatePoints()
    {
        // TODO: have afterimage count fade in
        // have the render points expand from the focus to equal thirds (see if 3 is enough or if it should scale with width)
    }

    public void DrawLines(float heightOffset, Color colour, int thickness, Vector2 localFocus)
    {
        Draw.Line(Position.X, startY + heightOffset, localFocus.X - 2, localFocus.Y + heightOffset, colour, thickness);
        Draw.Line(localFocus.X - 2, localFocus.Y + heightOffset, localFocus.X + 2, localFocus.Y + heightOffset, colour, thickness);
        Draw.Line(localFocus.X + 2, localFocus.Y + heightOffset, Position.X + Width, startY + heightOffset, colour, thickness);
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
}