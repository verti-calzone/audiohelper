using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System.Runtime.CompilerServices;
using System;
using System.Runtime.Intrinsics;
using FMOD.Studio;
using System.Collections;
using MonoMod;

namespace Celeste.Mod.audiohelper.Entities;

[CustomEntity("audiohelper/MusicalSwapBlock")]
[Tracked]

public class MusicalSwapBlock : SwapBlock {
    public string StartSound;
    public string MoveSound;
    public bool MoveSoundType;
    public string MoveEndSound;
    public string ReturnSound;
    public bool ReturnSoundType;
    public string ReturnEndSound;
    public string MusicParam;
    public float ParamValue, ResetValue;
    private int Mode = 0;
    public bool ResetType;
    public bool IncMode;
    public Musicalizer Musicalizer;
    public EventInstance startSfx;
    public bool WasSwapping = false;
    public MusicalSwapBlock(EntityData data, Vector2 offset)
        : base(data.Position + offset, data.Width, data.Height, data.Nodes[0] + offset, data.Enum("theme", Themes.Normal))
    {
        StartSound = data.Attr("StartSound");
        MoveSound = data.Attr("MoveSound");
        MoveSoundType = data.Bool("EndMoveSoundWithParameter");
        MoveEndSound = data.Attr("MoveEndSound");
        ReturnSound = data.Attr("ReturnSound");
        ReturnSoundType = data.Bool("EndReturnSoundWithParameter");
        ReturnEndSound = data.Attr("ReturnEndSound");
        MusicParam = data.Attr("MusicParameter");
        ParamValue = data.Float("ParameterValue");
        ResetValue = data.Float("ParameterResetValue");
        Mode = data.Int("Mode");
        ResetType = data.Bool("ResetBeforeReturn");
        IncMode = data.Bool("IncrementMode");
        Get<DashListener>().OnDash = MusicalOnDash;
        if(!string.IsNullOrEmpty(MusicParam)) Musicalizer = new Musicalizer();
    }
    [MethodImpl(MethodImplOptions.NoInlining)]
    private void MusicalOnDash(Vector2 direction)
    {
        Swapping = lerp < 1f;
        target = 1;
        returnTimer = 0.8f;
        burst = (base.Scene as Level).Displacement.AddBurst(base.Center, 0.2f, 0f, 16f);
        if (lerp >= 0.2f)
        {
            speed = maxForwardSpeed;
        }
        else
        {
            speed = MathHelper.Lerp(maxForwardSpeed * 0.333f, maxForwardSpeed, lerp / 0.2f);
        }
        if (ReturnSoundType || string.IsNullOrEmpty(ReturnSound)) Audio.SetParameter(returnSfx, "end", 1f);
        else Audio.Stop(returnSfx);
        Audio.Stop(startSfx);
        if (!Swapping)
        {
            if (MoveSoundType) Audio.SetParameter(moveSfx, "end", 1f);
            else Audio.Stop(moveSfx);
            if(!string.IsNullOrEmpty(MoveEndSound))Audio.Play(MoveEndSound, base.Center);
        }
        else
        {
            startSfx = Audio.Play(string.IsNullOrEmpty(StartSound) ? "event:/none" : StartSound, base.Center);
            if (!string.IsNullOrEmpty(MoveSound) && !Audio.IsPlaying(moveSfx)) moveSfx = Audio.Play(MoveSound, base.Center);
        }
        if (Position == start && !string.IsNullOrEmpty(MusicParam) && !ResetType) Musicalizer.ResetParameter(MusicParam,ParamValue,IncMode,Mode,ResetValue);
    }
    [MonoModLinkTo("Celeste.Solid", "System.Void Update()")]
    private extern void Solid_Update();
    [MethodImpl(MethodImplOptions.NoInlining)]
    public override void Update()
    {
        Solid_Update();
        if (returnTimer > 0f)
        {
            returnTimer -= Engine.DeltaTime;
            if (returnTimer <= 0f)
            {
                target = 0;
                speed = 0f;
                returnSfx = Audio.Play(string.IsNullOrEmpty(ReturnSound) ? "event:/none" : ReturnSound, base.Center);
            }
        }
        if (burst != null)
        {
            burst.Position = base.Center;
        }
        redAlpha = Calc.Approach(redAlpha, (target != 1) ? 1 : 0, Engine.DeltaTime * 32f);
        if (target == 0 && lerp == 0f)
        {
            middleRed.SetAnimationFrame(0);
            middleGreen.SetAnimationFrame(0);
        }
        if (target == 1)
        {
            speed = Calc.Approach(speed, maxForwardSpeed, maxForwardSpeed / 0.2f * Engine.DeltaTime);
        }
        else
        {
            speed = Calc.Approach(speed, maxBackwardSpeed, maxBackwardSpeed / 1.5f * Engine.DeltaTime);
        }
        float num = lerp;
        lerp = Calc.Approach(lerp, target, speed * Engine.DeltaTime);
        if (lerp != num)
        {
            Vector2 liftSpeed = (end - start) * speed;
            Vector2 position = Position;
            if (target == 1)
            {
                liftSpeed = (end - start) * maxForwardSpeed;
            }
            if (lerp < num)
            {
                liftSpeed *= -1f;
            }
            if (target == 1 && base.Scene.OnInterval(0.02f))
            {
                MoveParticles(end - start);
            }
            MoveTo(Vector2.Lerp(start, end, lerp), liftSpeed);
            if (position != Position)
            {
                Audio.Position(moveSfx, base.Center);
                Audio.Position(returnSfx, base.Center);
                if (Position == start && target == 0)
                {
                    if (ReturnSoundType || string.IsNullOrEmpty(ReturnSound)) Audio.SetParameter(returnSfx, "end", 1f);
                    else Audio.Stop(returnSfx);
                    if (!string.IsNullOrEmpty(ReturnEndSound)) Audio.Play(ReturnEndSound, base.Center);
                    if (!string.IsNullOrEmpty(MusicParam) && !ResetType) Musicalizer.ResetParameter(MusicParam,ParamValue,IncMode,Mode,ResetValue);
                }
                else if (Position == end && target == 1)
                {
                    if (!string.IsNullOrEmpty(MoveEndSound)) Audio.Play(MoveEndSound, base.Center);
                    if (MoveSoundType) Audio.SetParameter(moveSfx, "end", 1f);
                    else Audio.Stop(moveSfx);
                }
            }
        }
        if (Swapping && lerp >= 1f) Swapping = false;
        if(!WasSwapping && Swapping && !string.IsNullOrEmpty(MusicParam) && ResetType)Musicalizer.SetParameter(MusicParam,ParamValue,IncMode,Mode);
        if(WasSwapping && !Swapping && !string.IsNullOrEmpty(MusicParam) && ResetType) Musicalizer.ResetParameter(MusicParam,ParamValue,IncMode,Mode,ResetValue);
        WasSwapping = Swapping;

        StopPlayerRunIntoAnimation = lerp <= 0f || lerp >= 1f;
    }
    [MethodImpl(MethodImplOptions.NoInlining)]
    public override void Removed(Scene scene)
    {
        base.Removed(scene);
        Audio.Stop(startSfx);
        Audio.Stop(moveSfx);
        Audio.Stop(returnSfx);
        if(!string.IsNullOrEmpty(MusicParam)) Musicalizer.ResetParameter(MusicParam,ParamValue,IncMode,Mode,ResetValue);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public override void SceneEnd(Scene scene)
    {
        base.SceneEnd(scene);
        Audio.Stop(startSfx);
        Audio.Stop(moveSfx);
        Audio.Stop(returnSfx);
        if(!string.IsNullOrEmpty(MusicParam)) Musicalizer.ResetParameter(MusicParam,ParamValue,IncMode,Mode,ResetValue);
    }
}