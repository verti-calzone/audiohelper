using System;
using System.Collections;
using System.Runtime.CompilerServices;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod;

namespace Celeste.Mod.audiohelper.Entities;

[CustomEntity("audiohelper/BouncyString")]
[Tracked]
public class BouncyString : JumpThru
{
    public float offsetY = 0, startY;
    public Vector2 startPosition;
    public enum States { wait, ride, reset };
    public States state = States.wait;
    public float midPoint;
    public Vector2 pivot;

    public float speed, accel, baseSpeed, baseAccel, liftBoost;

    private readonly float minDistance = 8, maxDistance = 64;
    private readonly float minSpeed = 100, maxSpeed = 400;
    private readonly float minAccel = 900, maxAccel = 2400;
    private readonly float minLiftboost = -40, maxLiftboost = -500;
    public BouncyString(EntityData data, Vector2 offset) : base(data.Position + offset, data.Width, safe: false)
    {
        Position.Y += 4;
        startY = Position.Y;
        midPoint = Position.X + (Width / 2);

        pivot.X = midPoint;
        pivot.Y = Position.Y;

        SurfaceSoundIndex = 0;

        Collider = new PlayerOnlyColliderList(new Hitbox(Width, 5f));

        //Logger.Info("audiohelper", "in ctor, position is " + Position);
    }

    public override void Update()
    {
        base.Update();

        // implement better pivot return code; have it osc, have it not snap to player x if the player has left the wire during this bounce

        Player playerRider = GetPlayerRider();
        if (playerRider is null && pivot.X != midPoint)
        {
            pivot.X = Calc.Approach(pivot.X, midPoint, 100 * Engine.DeltaTime);
        }

        switch (state)
        {
            case States.wait:
                if (playerRider is not null)
                {
                    BeginBounce(playerRider);
                    state = States.ride;
                }
                break;
            case States.reset:
                speed = Calc.Approach(speed, baseSpeed, baseAccel * 0.5f * Engine.DeltaTime);
                pivot.Y += speed * Engine.DeltaTime;
                if (pivot.Y >= startY)
                {
                    pivot.Y = startY;
                    Position.Y = startY;
                    state = States.wait;
                    speed = 0f;
                }
                break;
            case States.ride:
                if (playerRider is not null) pivot.X = playerRider.CenterX;
                if (pivot.Y >= startY)
                {
                    speed -= accel * Engine.DeltaTime;
                }
                else
                {
                    speed += accel * Engine.DeltaTime;
                    float speedLimit = 0.5f * -baseSpeed;
                    if (speed > speedLimit)
                    {
                        if (playerRider is not null && playerRider.Speed.Y >= 0) playerRider.Speed.Y = Calc.ClampedMap(baseSpeed, minSpeed, maxSpeed, -150, -200);
                        state = States.reset;
                    }
                }
                pivot.Y += speed * Engine.DeltaTime;
                break;
        }

        if (pivot.Y >= startY)
        {
            MoveToY(pivot.Y, (speed < 0f) ? liftBoost : speed);
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
                pivotOffset.X = pivot.X - (Position.X + (player.CenterX <= pivot.X ? 0 : Width));
                pivotOffset.Y = pivot.Y - startY;
                float percent = (player.CenterX - (Position.X + (player.CenterX <= pivot.X ? 0 : Width))) / pivotOffset.X;
                float platformOffset = pivotOffset.Y * percent;
                MoveToY(startY + platformOffset, (speed < 0f) ? liftBoost : speed);
            }
        }
    }

    public override void Render()
    {
        base.Render();

        Color c;
        if (state == States.wait) c = Color.Green;
        else if (state == States.ride) c = Color.Red;
        else c = Color.Blue;
        Draw.Line(Position, Position + Vector2.UnitX * Width, c, 1);

        for (int i = 0; i <= 2; i++)
        {
            int thickness = 1;
            Color colour = Color.Gray;
            float heightOffset = 0;
            switch (i)
            {
                // body
                case 0:
                    heightOffset = 0;
                    colour = Calc.HexToColor("b38374");
                    thickness = 2;
                    break;
                // shadow
                case 1:
                    heightOffset = 1;
                    colour = Calc.HexToColor("3b1a17");
                    thickness = 1;
                    break;
                // highlight
                case 2:
                    heightOffset = -1;
                    colour = Calc.HexToColor("e2b2a0");
                    thickness = 1;
                    break;
            }
            DrawLinesPinched(heightOffset, colour, thickness);
        }

    }

    public void DrawLinesPinched(float heightOffset, Color colour, int thickness)
    {
        Draw.Line(Position.X, startY + heightOffset, pivot.X - 2, pivot.Y + heightOffset, colour, thickness);
        Draw.Line(pivot.X - 2, pivot.Y + heightOffset, pivot.X + 2, pivot.Y + heightOffset, colour, thickness);
        Draw.Line(pivot.X + 2, pivot.Y + heightOffset, Position.X + Width, startY + heightOffset, colour, thickness);
    }

    public void BeginBounce(Player player)
    {
        float left = player.Left - Position.X;
        float right = Position.X + Width - player.Right;
        float distance = MathF.Min(left, right);

        speed = Calc.ClampedMap(distance, minDistance, maxDistance, minSpeed, maxSpeed);
        accel = Calc.ClampedMap(distance, minDistance, maxDistance, minAccel, maxAccel);
        liftBoost = Calc.ClampedMap(distance, minDistance, maxDistance, minLiftboost, maxLiftboost);

        baseSpeed = speed;
        baseAccel = accel;
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


//using System;
//using System.Collections;
//using System.Runtime.CompilerServices;
//using Celeste.Mod.Entities;
//using Microsoft.Xna.Framework;
//using Monocle;
//using MonoMod;

//namespace Celeste.Mod.audiohelper.Entities;

//[CustomEntity("audiohelper/BouncyString")]
//[Tracked]
//public class BouncyString : JumpThru
//{
//    public float offsetY = 0, startY;
//    public Vector2 startPosition;
//    public enum States { wait, ride, reset };
//    public States state = States.wait;
//    public bool wasRiding = false;
//    public float pinchPointX, midPoint;

//    public float speed, accel, baseSpeed, baseAccel, liftBoost;

//    private readonly float minDistance = 8, maxDistance = 64;
//    private readonly float minSpeed = 100, maxSpeed = 400;
//    private readonly float minAccel = 900, maxAccel = 2400;
//    private readonly float minLiftboost = -40, maxLiftboost = -500;
//    public BouncyString(EntityData data, Vector2 offset) : base(data.Position + offset, data.Width, safe: false)
//    {
//        Position.Y += 4;
//        startY = Position.Y;
//        midPoint = Position.X + (Width / 2);
//        pinchPointX = midPoint;

//        SurfaceSoundIndex = 0;

//        Collider = new PlayerOnlyColliderList(new Hitbox(Width, 5f));

//        //Logger.Info("audiohelper", "in ctor, position is " + Position);
//    }

//    public override void Update()
//    {
//        base.Update();

//        Player playerRider = GetPlayerRider();
//        if (playerRider is null && pinchPointX != midPoint)
//        {
//            pinchPointX = Calc.Approach(pinchPointX, midPoint, 100 * Engine.DeltaTime);
//        }

//        switch (state)
//        {
//            case States.wait:
//                if (playerRider is not null)
//                {
//                    BeginBounce(playerRider);
//                    state = States.ride;
//                }
//                break;
//            case States.reset:
//                speed = Calc.Approach(speed, baseSpeed, baseAccel * 0.5f * Engine.DeltaTime);
//                MoveTowardsY(startY, speed * Engine.DeltaTime);
//                if (ExactPosition.Y == startY)
//                {
//                    state = States.wait;
//                    speed = 0f;
//                }
//                break;
//            case States.ride:
//                if (playerRider is not null) pinchPointX = playerRider.CenterX;
//                if (Position.Y >= startY)
//                {
//                    speed -= accel * Engine.DeltaTime;
//                }
//                else
//                {
//                    speed += accel * Engine.DeltaTime;
//                    float speedLimit = 0.5f * -baseSpeed;
//                    if (speed > speedLimit)
//                    {
//                        if (playerRider is not null && playerRider.Speed.Y >= 0) playerRider.Speed.Y = Calc.ClampedMap(baseSpeed, minSpeed, maxSpeed, -150, -200);
//                        state = States.reset;
//                    }
//                }
//                MoveV(speed * Engine.DeltaTime, (speed < 0f) ? liftBoost : speed);
//                break;
//        }
//    }

//    public override void Render()
//    {
//        base.Render();

//        for (int i = 0; i <= 2; i++)
//        {
//            int thickness = 1;
//            Color colour = Color.Gray;
//            float lineOffset = 0;
//            switch (i)
//            {
//                // body
//                case 0:
//                    lineOffset = 0;
//                    colour = Calc.HexToColor("b38374");
//                    thickness = 2;
//                    break;
//                // shadow
//                case 1:
//                    lineOffset = 1;
//                    colour = Calc.HexToColor("3b1a17");
//                    thickness = 1;
//                    break;
//                // highlight
//                case 2:
//                    lineOffset = -1;
//                    colour = Calc.HexToColor("e2b2a0");
//                    thickness = 1;
//                    break;
//            }
//            DrawLinesPinched(lineOffset, colour, thickness);
//        }

//    }

//    public void DrawLinesPinched(float lineOffset, Color colour, int thickness)
//    {
//        Draw.Line(Position.X, startY + lineOffset, pinchPointX - 2, Position.Y + lineOffset, colour, thickness);
//        Draw.Line(pinchPointX - 2, Position.Y + lineOffset, pinchPointX + 2, Position.Y + lineOffset, colour, thickness);
//        Draw.Line(pinchPointX + 2, Position.Y + lineOffset, Position.X + Width, startY + lineOffset, colour, thickness);
//    }

//    public void BeginBounce(Player player)
//    {
//        float left = player.Left - Position.X;
//        float right = Position.X + Width - player.Right;
//        float distance = MathF.Min(left, right);

//        speed = Calc.ClampedMap(distance, minDistance, maxDistance, minSpeed, maxSpeed);
//        accel = Calc.ClampedMap(distance, minDistance, maxDistance, minAccel, maxAccel);
//        liftBoost = Calc.ClampedMap(distance, minDistance, maxDistance, minLiftboost, maxLiftboost);

//        baseSpeed = speed;
//        baseAccel = accel;
//    }

//    private class PlayerOnlyColliderList : ColliderList
//    {
//        public PlayerOnlyColliderList(params Collider[] colliders)
//        {
//            this.colliders = colliders;
//        }

//        public override bool Collide(Hitbox hitbox) => hitbox.Entity is Player && base.Collide(hitbox);
//        public override bool Collide(Circle hitbox) => false;
//        public override bool Collide(Grid hitbox) => false;
//        public override bool Collide(ColliderList hitbox) => false;
//    }
//}