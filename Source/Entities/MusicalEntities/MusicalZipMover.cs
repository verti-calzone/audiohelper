using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System.Runtime.CompilerServices;
using System;
using System.Runtime.Intrinsics;
using FMOD.Studio;
using System.Collections;

namespace Celeste.Mod.audiohelper.Entities;

[CustomEntity("audiohelper/MusicalZipMover")]
[Tracked]

public class MusicalZipMover : ZipMover {
    public float MoveTime = 0.5f;
    public float HoldTime = 0.5f;
    public float ReturnTime = 2.0f;
    
    public string ActivateSound;
    public string ImpactSound;
    public string ReturnSound;
    public string ResetSound;
    public string MusicParam;
    public float ParamValue, ResetValue;
    private int Mode = 0;
    public bool IncMode = false;
    public bool ResetType;
    public Musicalizer Musicalizer;

    public bool CanPlayResetSound = true;
    public MusicalZipMover(EntityData data, Vector2 offset) : base(data.Position + offset, data.Width, data.Height, data.Nodes[0] + offset, data.Enum("theme", Themes.Normal))
    {
        MoveTime = data.Float("MoveTime");
        HoldTime = data.Float("HoldTime");
        ReturnTime = data.Float("ReturnTime");
        if(MoveTime<0.1f) MoveTime = 0.1f;
        if(ReturnTime<0.1f) ReturnTime = 0.1f;
        
        ActivateSound = data.Attr("ActivateSound");
        ImpactSound = data.Attr("ImpactSound");
        ReturnSound = data.Attr("ReturnSound");
        ResetSound = data.Attr("ResetSound");
        MusicParam = data.Attr("MusicParameter");
        ParamValue = data.Float("ParameterValue");
        ResetValue = data.Float("ParameterResetValue");
        Mode = data.Int("Mode");
        IncMode = data.Bool("IncrementMode");
        ResetType = data.Bool("ResetBeforeReturn");

        Get<Coroutine>().Replace(MusicalSequence());
        if(!string.IsNullOrEmpty(MusicParam)) Musicalizer = new Musicalizer();
    }
    private IEnumerator MusicalSequence()
    {
        Vector2 start = Position;
        while (true)
        {
            if (!HasPlayerRider())
            {
                yield return null;
                continue;
            }

            // Activate Stage
            sfx.Play(string.IsNullOrEmpty(ActivateSound) ? "event:/none" : ActivateSound);
            if(!string.IsNullOrEmpty(MusicParam)) Musicalizer.SetParameter(MusicParam,ParamValue,IncMode,Mode);
            Input.Rumble(RumbleStrength.Medium, RumbleLength.Short);
            StartShaking(0.1f);
            yield return 0.1f;
            streetlight.SetAnimationFrame(3);
            StopPlayerRunIntoAnimation = false;
            float at = 0f;
            while (at < 1f)
            {
                yield return null;
                at = Calc.Approach(at, 1f, 1f/MoveTime * Engine.DeltaTime);
                percent = Ease.SineIn(at);
                Vector2 vector = Vector2.Lerp(start, target, percent);
                ScrapeParticlesCheck(vector);
                if (Scene.OnInterval(0.1f))
                {
                    pathRenderer.CreateSparks();
                }
                MoveTo(vector);
            }

            // Hold Stage
            sfx.Stop();
            if(!string.IsNullOrEmpty(ImpactSound)) Audio.Play(ImpactSound,Position);
            if(!string.IsNullOrEmpty(MusicParam) && ResetType) Musicalizer.ResetParameter(MusicParam,ParamValue,IncMode,Mode,ResetValue);
            StartShaking(0.2f);
            Input.Rumble(RumbleStrength.Strong, RumbleLength.Medium);
            SceneAs<Level>().Shake();
            StopPlayerRunIntoAnimation = true;
            yield return HoldTime;

            // Return Stage
            sfx.Play(string.IsNullOrEmpty(ReturnSound) ? "event:/none" : ReturnSound);
            StopPlayerRunIntoAnimation = false;
            streetlight.SetAnimationFrame(2);
            at = 0f;
            while (at < 1f)
            {
                yield return null;
                at = Calc.Approach(at, 1f, 1f/ReturnTime * Engine.DeltaTime);
                percent = 1f - Ease.SineIn(at);
                Vector2 position = Vector2.Lerp(target, start, Ease.SineIn(at));
                MoveTo(position);
                // ResetSound happens 0.1 seconds before the reset stage actually occurs
                if (CanPlayResetSound && (1 - at <= 0.1f * 1f/ReturnTime) && !string.IsNullOrEmpty(ResetSound))
                {
                    Audio.Play(ResetSound,Position);
                    CanPlayResetSound = false;
                }
            }

            // Reset Stage
            CanPlayResetSound = true;
            sfx.Stop();
            if(!string.IsNullOrEmpty(MusicParam) && !ResetType) Musicalizer.ResetParameter(MusicParam,ParamValue,IncMode,Mode,ResetValue);
            StopPlayerRunIntoAnimation = true;
            StartShaking(0.2f);
            streetlight.SetAnimationFrame(1);
            yield return 0.5f;
        }
    }
    [MethodImpl(MethodImplOptions.NoInlining)]
    public override void Removed(Scene scene)
    {
        if(!string.IsNullOrEmpty(MusicParam)) Musicalizer.ResetParameter(MusicParam,ParamValue,IncMode,Mode,ResetValue);
        base.Removed(scene);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public override void SceneEnd(Scene scene)
    {
        if(!string.IsNullOrEmpty(MusicParam)) Musicalizer.ResetParameter(MusicParam,ParamValue,IncMode,Mode,ResetValue);
        base.SceneEnd(scene);
    }
}