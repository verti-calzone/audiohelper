using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System.Runtime.CompilerServices;
using System;

namespace Celeste.Mod.audiohelper.Entities;

[CustomEntity("audiohelper/MusicalBell")]
[Tracked]

public class MusicalBell : Bell {
    private string MusicParam;
    private float ParamValue, ResetValue;
    public bool IncMode = false;
    private float MusicalizerTargetTimer;
    public Musicalizer Musicalizer;
    private float MusicalizerTimer = 0f;
    private bool Reset;
    private int Mode = 0;

    public MusicalBell(EntityData data, Vector2 offset, EntityID id) : base(data, offset, id)
    {
        MusicParam = data.Attr("MusicParameter");
        ParamValue = data.Float("ParameterValue");
        ResetValue = data.Float("ParameterResetValue");
        Mode = data.Int("Mode");
        IncMode = data.Bool("IncrementMode");
        MusicalizerTargetTimer = data.Float("Timer");
        Musicalizer = new Musicalizer();
    }

    public override void Ring(float xSpeed, float ySpeed)
    {
        Speed = (float)Math.Sqrt(xSpeed*xSpeed+ySpeed*ySpeed) + 500*VolumeBoost;
        if(Speed < 200) Speed = 200;
        else if(Speed > 500) Speed = 500;
        if (!string.IsNullOrEmpty(Sound)) sfx.Play(Sound,"pitch",Pitch,"speed",Speed*0.002f,false);
        AngularMomentum = 0.01f*Speed;
        if(Math.Sign(xSpeed) == 1) AngularMomentum *= -1;
        if(!string.IsNullOrEmpty(SetFlag)) SceneAs<Level>().Session.SetFlag(SetFlag);
        Ready = false;
        Musicalizer.SetParameter(MusicParam,ParamValue,IncMode,Mode);
        MusicalizerTimer = MusicalizerTargetTimer;
        Reset = true;
    }
    [MethodImpl(MethodImplOptions.NoInlining)]
	public override void Update()
	{
		base.Update();
        if(MusicalizerTimer > 0) MusicalizerTimer -= Engine.DeltaTime;
        else if(Reset)
        {
            Musicalizer.ResetParameter(MusicParam,ParamValue,IncMode,Mode,ResetValue);
            MusicalizerTimer = 0f;
            Reset = false;
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