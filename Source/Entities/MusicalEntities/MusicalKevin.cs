using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System.Runtime.CompilerServices;
using System;
using System.Runtime.Intrinsics;
using FMOD.Studio;
using System.Collections;

namespace Celeste.Mod.audiohelper.Entities;

[CustomEntity("audiohelper/MusicalKevin")]
[Tracked]

public class MusicalKevin : CrushBlock {
    public string ActivateSound;
    public string MoveSound;
    public string ImpactSound;
    public string ReturnSound;
    public string WaypointSound;
    public string RestSound;

    public string MusicParam;
    public float ParamValue;
    public bool IncMode = false;
    public Musicalizer Musicalizer;
    public MusicalKevin(EntityData data, Vector2 offset) : base(data.Position + offset, data.Width, data.Height, data.Enum("axes", Axes.Both), data.Bool("chillout"))
    {
        ActivateSound = data.Attr("ActivateSound");
        MoveSound = data.Attr("MoveSound");
        ImpactSound = data.Attr("ImpactSound");
        ReturnSound = data.Attr("ReturnSound");
        WaypointSound = data.Attr("WaypointSound");
        RestSound = data.Attr("RestSound");
        
        MusicParam = data.Attr("MusicParameter");
        ParamValue = data.Float("ParameterValue");
        IncMode = data.Bool("IncrementMode");

        if(!string.IsNullOrEmpty(MusicParam)) Musicalizer = new Musicalizer();
    }
    private void MusicalAttack(Vector2 direction)
    {
        if(!string.IsNullOrEmpty(ActivateSound)) Audio.Play(ActivateSound, base.Center);
        if(!string.IsNullOrEmpty(MusicParam))
        {
            Musicalizer.ResetParameter(MusicParam,ParamValue,IncMode);
            Musicalizer.SetParameter(MusicParam,ParamValue,IncMode);
        }
        if (currentMoveLoopSfx != null)
        {
            currentMoveLoopSfx.Param("end", 1f);
            SoundSource sfx = currentMoveLoopSfx;
            Alarm.Set(this, 0.5f, delegate
            {
                sfx.RemoveSelf();
            });
        }
        Add(currentMoveLoopSfx = new SoundSource());
        currentMoveLoopSfx.Position = new Vector2(base.Width, base.Height) / 2f;
        currentMoveLoopSfx.Play(string.IsNullOrEmpty(MoveSound) ? "event:/none" : MoveSound);
        face.Play("hit");
        crushDir = direction;
        canActivate = false;
        attackCoroutine.Replace(MusicalAttackSequence());
        ClearRemainder();
        TurnOffImages();
        ActivateParticles(crushDir);
        if (crushDir.X < 0f)
        {
            foreach (Image activeLeftImage in activeLeftImages)
            {
                activeLeftImage.Visible = true;
            }
            nextFaceDirection = "left";
        }
        else if (crushDir.X > 0f)
        {
            foreach (Image activeRightImage in activeRightImages)
            {
                activeRightImage.Visible = true;
            }
            nextFaceDirection = "right";
        }
        else if (crushDir.Y < 0f)
        {
            foreach (Image activeTopImage in activeTopImages)
            {
                activeTopImage.Visible = true;
            }
            nextFaceDirection = "up";
        }
        else if (crushDir.Y > 0f)
        {
            foreach (Image activeBottomImage in activeBottomImages)
            {
                activeBottomImage.Visible = true;
            }
            nextFaceDirection = "down";
        }
        bool flag = true;
        if (returnStack.Count > 0)
        {
            MoveState moveState = returnStack[returnStack.Count - 1];
            if (moveState.Direction == direction || moveState.Direction == -direction)
            {
                flag = false;
            }
        }
        if (flag)
        {
            returnStack.Add(new MoveState(Position, crushDir));
        }
    }
    private IEnumerator MusicalAttackSequence()
    {
        Input.Rumble(RumbleStrength.Strong, RumbleLength.Medium);
        StartShaking(0.4f);
        yield return 0.4f;
        if (!chillOut)
        {
            canActivate = true;
        }
        StopPlayerRunIntoAnimation = false;
        bool slowing = false;
        float speed = 0f;
        while (true)
        {
            if (!chillOut)
            {
                speed = Calc.Approach(speed, 240f, 500f * Engine.DeltaTime);
            }
            else if (slowing || CollideCheck<SolidTiles>(Position + crushDir * 256f))
            {
                speed = Calc.Approach(speed, 24f, 500f * Engine.DeltaTime * 0.25f);
                if (!slowing)
                {
                    slowing = true;
                    Alarm.Set(this, 0.5f, [MethodImpl(MethodImplOptions.NoInlining)] () =>
                    {
                        face.Play("hurt");
                        SoundSource soundSource = currentMoveLoopSfx;
                        if (soundSource != null)
                        {
                            soundSource = soundSource.Stop();
                        }
                        TurnOffImages();
                    });
                }
            }
            else
            {
                speed = Calc.Approach(speed, 240f, 500f * Engine.DeltaTime);
            }
            bool flag = ((crushDir.X == 0f) ? MoveVCheck(speed * crushDir.Y * Engine.DeltaTime) : MoveHCheck(speed * crushDir.X * Engine.DeltaTime));
            if (Top >= (float)(level.Bounds.Bottom + 32))
            {
                RemoveSelf();
                yield break;
            }
            if (flag)
            {
                break;
            }
            if (Scene.OnInterval(0.02f))
            {
                Vector2 position;
                float direction;
                if (crushDir == Vector2.UnitX)
                {
                    position = new Vector2(Left + 1f, Calc.Random.Range(Top + 3f, Bottom - 3f));
                    direction = (float)Math.PI;
                }
                else if (crushDir == -Vector2.UnitX)
                {
                    position = new Vector2(Right - 1f, Calc.Random.Range(Top + 3f, Bottom - 3f));
                    direction = 0f;
                }
                else if (crushDir == Vector2.UnitY)
                {
                    position = new Vector2(Calc.Random.Range(Left + 3f, Right - 3f), Top + 1f);
                    direction = -(float)Math.PI / 2f;
                }
                else
                {
                    position = new Vector2(Calc.Random.Range(Left + 3f, Right - 3f), Bottom - 1f);
                    direction = (float)Math.PI / 2f;
                }
                level.Particles.Emit(P_Crushing, position, direction);
            }
            yield return null;
        }
        FallingBlock fallingBlock = CollideFirst<FallingBlock>(Position + crushDir);
        if (fallingBlock != null)
        {
            fallingBlock.Triggered = true;
        }
        if (crushDir == -Vector2.UnitX)
        {
            Vector2 vector = new Vector2(0f, 2f);
            for (int num = 0; (float)num < Height / 8f; num++)
            {
                Vector2 vector2 = new Vector2(Left - 1f, Top + 4f + (float)(num * 8));
                if (!Scene.CollideCheck<Water>(vector2) && Scene.CollideCheck<Solid>(vector2))
                {
                    SceneAs<Level>().ParticlesFG.Emit(P_Impact, vector2 + vector, 0f);
                    SceneAs<Level>().ParticlesFG.Emit(P_Impact, vector2 - vector, 0f);
                }
            }
        }
        else if (crushDir == Vector2.UnitX)
        {
            Vector2 vector3 = new Vector2(0f, 2f);
            for (int num2 = 0; (float)num2 < Height / 8f; num2++)
            {
                Vector2 vector4 = new Vector2(Right + 1f, Top + 4f + (float)(num2 * 8));
                if (!Scene.CollideCheck<Water>(vector4) && Scene.CollideCheck<Solid>(vector4))
                {
                    SceneAs<Level>().ParticlesFG.Emit(P_Impact, vector4 + vector3, (float)Math.PI);
                    SceneAs<Level>().ParticlesFG.Emit(P_Impact, vector4 - vector3, (float)Math.PI);
                }
            }
        }
        else if (crushDir == -Vector2.UnitY)
        {
            Vector2 vector5 = new Vector2(2f, 0f);
            for (int num3 = 0; (float)num3 < Width / 8f; num3++)
            {
                Vector2 vector6 = new Vector2(Left + 4f + (float)(num3 * 8), Top - 1f);
                if (!Scene.CollideCheck<Water>(vector6) && Scene.CollideCheck<Solid>(vector6))
                {
                    SceneAs<Level>().ParticlesFG.Emit(P_Impact, vector6 + vector5, (float)Math.PI / 2f);
                    SceneAs<Level>().ParticlesFG.Emit(P_Impact, vector6 - vector5, (float)Math.PI / 2f);
                }
            }
        }
        else if (crushDir == Vector2.UnitY)
        {
            Vector2 vector7 = new Vector2(2f, 0f);
            for (int num4 = 0; (float)num4 < Width / 8f; num4++)
            {
                Vector2 vector8 = new Vector2(Left + 4f + (float)(num4 * 8), Bottom + 1f);
                if (!Scene.CollideCheck<Water>(vector8) && Scene.CollideCheck<Solid>(vector8))
                {
                    SceneAs<Level>().ParticlesFG.Emit(P_Impact, vector8 + vector7, -(float)Math.PI / 2f);
                    SceneAs<Level>().ParticlesFG.Emit(P_Impact, vector8 - vector7, -(float)Math.PI / 2f);
                }
            }
        }
        if(!string.IsNullOrEmpty(ImpactSound)) Audio.Play(ImpactSound, Center);
        if(!string.IsNullOrEmpty(MusicParam)) Musicalizer.ResetParameter(MusicParam,ParamValue,IncMode);
        level.DirectionalShake(crushDir);
        Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
        StartShaking(0.4f);
        StopPlayerRunIntoAnimation = true;
        SoundSource sfx = currentMoveLoopSfx;
        currentMoveLoopSfx.Param("end", 1f);
        currentMoveLoopSfx = null;
        Alarm.Set(this, 0.5f, delegate
        {
            sfx.RemoveSelf();
        });
        crushDir = Vector2.Zero;
        TurnOffImages();
        if (chillOut)
        {
            yield break;
        }
        face.Play("hurt");
        returnLoopSfx.Play(string.IsNullOrEmpty(ImpactSound) ? "event:/none" : ReturnSound);
        yield return 0.4f;
        speed = 0f;
        float waypointSfxDelay = 0f;
        while (returnStack.Count > 0)
        {
            yield return null;
            StopPlayerRunIntoAnimation = false;
            MoveState moveState = returnStack[returnStack.Count - 1];
            speed = Calc.Approach(speed, 60f, 160f * Engine.DeltaTime);
            waypointSfxDelay -= Engine.DeltaTime;
            if (moveState.Direction.X != 0f)
            {
                MoveTowardsX(moveState.From.X, speed * Engine.DeltaTime);
            }
            if (moveState.Direction.Y != 0f)
            {
                MoveTowardsY(moveState.From.Y, speed * Engine.DeltaTime);
            }
            if ((moveState.Direction.X != 0f && ExactPosition.X != moveState.From.X) || (moveState.Direction.Y != 0f && ExactPosition.Y != moveState.From.Y))
            {
                continue;
            }
            speed = 0f;
            returnStack.RemoveAt(returnStack.Count - 1);
            StopPlayerRunIntoAnimation = true;
            if (returnStack.Count <= 0)
            {
                face.Play("idle");
                returnLoopSfx.Stop();
                if (waypointSfxDelay <= 0f)
                {
                    if(!string.IsNullOrEmpty(RestSound)) Audio.Play(RestSound, Center);
                }
            }
            else if (waypointSfxDelay <= 0f)
            {
                if(!string.IsNullOrEmpty(WaypointSound)) Audio.Play(WaypointSound, Center);
            }
            waypointSfxDelay = 0.1f;
            StartShaking(0.2f);
            yield return 0.2f;
        }
    }
    public static void MusicalKevinAttack(On.Celeste.CrushBlock.orig_Attack orig, CrushBlock self, Vector2 direction)
    {
        if (self is MusicalKevin MusicalKevin) MusicalKevin.MusicalAttack(direction);
        else orig(self, direction);
    }
}