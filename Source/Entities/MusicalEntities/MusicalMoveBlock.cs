using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System.Runtime.CompilerServices;
using System;
using System.Runtime.Intrinsics;
using FMOD.Studio;
using System.Collections;
using MonoMod;
using System.Collections.Generic;

namespace Celeste.Mod.audiohelper.Entities;

[CustomEntity("audiohelper/MusicalMoveBlock")]
[Tracked]

public class MusicalMoveBlock : MoveBlock {
    public string ActivateSound;
    public string MoveSound;
    public string BreakSound;
    public string ReformSound;
    public string ReappearSound;
    public string MusicParam;
    public float MusicParamValue;
    public float OldMusicParamValue = -1f;
    public bool IncMode;

    [MethodImpl(MethodImplOptions.NoInlining)]
    public MusicalMoveBlock(EntityData data, Vector2 offset)
        : base(data.Position + offset, data.Width, data.Height, data.Enum("direction", Directions.Left), data.Bool("canSteer", defaultValue: true), data.Bool("fast"))
    {
        ActivateSound = data.Attr("ActivateSound");
        MoveSound = data.Attr("MoveSound");
        BreakSound = data.Attr("BreakSound");
        ReformSound = data.Attr("ReformSound");
        ReappearSound = data.Attr("ReappearSound");
        MusicParam = data.Attr("MusicParameter");
        MusicParamValue = data.Float("MusicParameterValue");
        IncMode = data.Bool("IncrementMode");
        Get<Coroutine>().Replace(MusicalController());
    }
    public IEnumerator MusicalController()
    {
        while (true)
        {
            triggered = false;
            state = MovementState.Idling;
            while (!triggered && !HasPlayerRider())
            {
                yield return null;
            }
            if (!string.IsNullOrEmpty(ActivateSound)) Audio.Play(ActivateSound, Position);
            if (!string.IsNullOrEmpty(MusicParam))
            {
                Audio.CurrentMusicEventInstance.getParameterValue(MusicParam, out OldMusicParamValue, out _);
                if (!IncMode) Audio.SetMusicParam(MusicParam,MusicParamValue);
                else Audio.SetMusicParam(MusicParam,OldMusicParamValue+MusicParamValue);
            }
            state = MovementState.Moving;
            StartShaking(0.2f);
            ActivateParticles();
            yield return 0.2f;
            targetSpeed = fast ? 75f : 60f;
            if (!string.IsNullOrEmpty(MoveSound)) moveSfx.Play(MoveSound);
            moveSfx.Param("arrow_stop", 0f);
            StopPlayerRunIntoAnimation = false;
            float crashTimer = 0.15f;
            float crashResetTimer = 0.1f;
            float noSteerTimer = 0.2f;
            while (true)
            {
                if (canSteer)
                {
                    targetAngle = homeAngle;
                    bool flag = (direction != Directions.Right && direction != Directions.Left) ? HasPlayerClimbing() : HasPlayerOnTop();
                    if (flag && noSteerTimer > 0f)
                    {
                        noSteerTimer -= Engine.DeltaTime;
                    }
                    if (flag)
                    {
                        if (noSteerTimer <= 0f)
                        {
                            if (direction == Directions.Right || direction == Directions.Left)
                            {
                                targetAngle = homeAngle + (float)Math.PI / 4f * (float)angleSteerSign * (float)Input.MoveY.Value;
                            }
                            else
                            {
                                targetAngle = homeAngle + (float)Math.PI / 4f * (float)angleSteerSign * (float)Input.MoveX.Value;
                            }
                        }
                    }
                    else
                    {
                        noSteerTimer = 0.2f;
                    }
                }
                if (Scene.OnInterval(0.02f))
                {
                    MoveParticles();
                }
                speed = Calc.Approach(speed, targetSpeed, 300f * Engine.DeltaTime);
                angle = Calc.Approach(angle, targetAngle, (float)Math.PI * 16f * Engine.DeltaTime);
                Vector2 vector = Calc.AngleToVector(angle, speed);
                Vector2 vec = vector * Engine.DeltaTime;
                bool flag2;
                if (direction == Directions.Right || direction == Directions.Left)
                {
                    flag2 = MoveCheck(vec.XComp());
                    noSquish = Scene.Tracker.GetEntity<Player>();
                    MoveVCollideSolids(vec.Y, thruDashBlocks: false);
                    noSquish = null;
                    LiftSpeed = vector;
                    if (Scene.OnInterval(0.03f))
                    {
                        if (vec.Y > 0f)
                        {
                            ScrapeParticles(Vector2.UnitY);
                        }
                        else if (vec.Y < 0f)
                        {
                            ScrapeParticles(-Vector2.UnitY);
                        }
                    }
                }
                else
                {
                    flag2 = MoveCheck(vec.YComp());
                    noSquish = Scene.Tracker.GetEntity<Player>();
                    MoveHCollideSolids(vec.X, thruDashBlocks: false);
                    noSquish = null;
                    LiftSpeed = vector;
                    if (Scene.OnInterval(0.03f))
                    {
                        if (vec.X > 0f)
                        {
                            ScrapeParticles(Vector2.UnitX);
                        }
                        else if (vec.X < 0f)
                        {
                            ScrapeParticles(-Vector2.UnitX);
                        }
                    }
                    if (direction == Directions.Down && Top > (float)(SceneAs<Level>().Bounds.Bottom + 32))
                    {
                        flag2 = true;
                    }
                }
                if (flag2)
                {
                    moveSfx?.Param("arrow_stop", 1f);
                    crashResetTimer = 0.1f;
                    if (!(crashTimer > 0f))
                    {
                        break;
                    }
                    crashTimer -= Engine.DeltaTime;
                }
                else
                {
                    moveSfx?.Param("arrow_stop", 0f);
                    if (crashResetTimer > 0f)
                    {
                        crashResetTimer -= Engine.DeltaTime;
                    }
                    else
                    {
                        crashTimer = 0.15f;
                    }
                }
                Level level = Scene as Level;
                if (Left < (float)level.Bounds.Left || Top < (float)level.Bounds.Top || Right > (float)level.Bounds.Right)
                {
                    break;
                }
                yield return null;
            }
            if (!string.IsNullOrEmpty(BreakSound)) Audio.Play(BreakSound, Position);
            moveSfx?.Stop();
            if (!string.IsNullOrEmpty(MusicParam)) Audio.SetMusicParam(MusicParam,OldMusicParamValue);
            state = MovementState.Breaking;
            speed = targetSpeed = 0f;
            angle = targetAngle = homeAngle;
            StartShaking(0.2f);
            StopPlayerRunIntoAnimation = true;
            yield return 0.2f;
            BreakParticles();
            List<Debris> debris = new List<Debris>();
            for (int i = 0; (float)i < Width; i += 8)
            {
                for (int j = 0; (float)j < Height; j += 8)
                {
                    Vector2 vector2 = new Vector2((float)i + 4f, (float)j + 4f);
                    Debris debris2 = Engine.Pooler.Create<Debris>().Init(Position + vector2, Center, startPosition + vector2);
                    debris.Add(debris2);
                    Scene.Add(debris2);
                }
            }
            MoveBlock moveBlock = this;
            Vector2 amount = startPosition - Position;
            DisableStaticMovers();
            moveBlock.MoveStaticMovers(amount);
            Position = startPosition;
            MoveBlock moveBlock2 = this;
            MoveBlock moveBlock3 = this;
            bool visible = false;
            moveBlock3.Collidable = false;
            moveBlock2.Visible = visible;
            yield return 2.2f;
            foreach (Debris item in debris)
            {
                item.StopMoving();
            }
            while (CollideCheck<Actor>() || CollideCheck<Solid>())
            {
                yield return null;
            }
            Collidable = true;
            EventInstance instance = Audio.Play(string.IsNullOrEmpty(ReformSound) ? "event:/none" : ReformSound, debris[0].Position);
            MoveBlock moveBlock4 = this;
            Coroutine component;
            Coroutine routine = component = new Coroutine(SoundFollowsDebrisCenter(instance, debris));
            moveBlock4.Add(component);
            foreach (Debris item2 in debris)
            {
                item2.StartShaking();
            }
            yield return 0.2f;
            foreach (Debris item3 in debris)
            {
                item3.ReturnHome(0.65f);
            }
            yield return 0.6f;
            routine.RemoveSelf();
            foreach (Debris item4 in debris)
            {
                item4.RemoveSelf();
            }
            if (string.IsNullOrEmpty(ReappearSound)) Audio.Play(ReappearSound, Position);
            Visible = true;
            EnableStaticMovers();
            speed = targetSpeed = 0f;
            angle = targetAngle = homeAngle;
            noSquish = null;
            fillColor = idleBgFill;
            UpdateColors();
            flash = 1f;
        }
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public override void Removed(Scene scene)
    {
        base.Removed(scene);
        if(!string.IsNullOrEmpty(MusicParam)) Audio.SetMusicParam(MusicParam,OldMusicParamValue);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public override void SceneEnd(Scene scene)
    {
        base.SceneEnd(scene);
        if(!string.IsNullOrEmpty(MusicParam)) Audio.SetMusicParam(MusicParam,OldMusicParamValue);
    }
}