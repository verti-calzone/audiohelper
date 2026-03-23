using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System.Runtime.CompilerServices;
using System;

namespace Celeste.Mod.audiohelper.Entities;

[CustomEntity("audiohelper/Bell")]
[Tracked]

public class Bell : Entity {
    public Sprite sprite;
    public string Sound;
    public int Pitch;
    public Color Colour = Calc.HexToColor("c0c0c0");
    public float VolumeBoost = 0f;
    public string SetFlag;
    public bool Ready = true;
    public bool Waiting = false;
    public float Timer = 1f;
    public float Speed;
    public float Angle;
    public float AngularMomentum;
    public float Inertia;
    public float InertiaBase = 10;
    public MultiParameterSoundSource sfx;
    public Bell(EntityData data, Vector2 offset, EntityID id) : base(data.Position + offset)
    {
        Sound = data.Attr("sound");
        Pitch = data.Int("pitch");
        Colour = data.HexColor("colour");
        VolumeBoost = data.Float("VolumeBoost");
        if(VolumeBoost < 0) VolumeBoost = 0f;
        SetFlag = data.Attr("SetFlag");
        Add(sprite = GFX.SpriteBank.Create("audiohelper_bell"));
        sprite.Color = Colour;
        Add(sfx = new MultiParameterSoundSource());
        sfx.Position.Y = 8;
        Collider = new Circle(12f);
        Collider.Position.Y = 8;
        Add(new PlayerCollider(OnPlayer));
        Depth = 2000;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private void OnPlayer(Player player)
    {
        if(Ready)
        {
            Speed = (float)Math.Sqrt(player.Speed.X*player.Speed.X+player.Speed.Y*player.Speed.Y) + 500*VolumeBoost;
            if(Speed < 200) Speed = 200;
            else if(Speed > 500) Speed = 500;
            //Logger.Info("audiohelper","rang a bell with speed: "+Speed+" and speed param: "+Speed*0.002);
            if (!string.IsNullOrEmpty(Sound)) sfx.Play(Sound,"pitch",Pitch,"speed",Speed*0.002f,false);
            AngularMomentum = 0.01f*Speed;
            if(Math.Sign(player.Speed.X) == 1) AngularMomentum *= -1;
            if(!string.IsNullOrEmpty(SetFlag)) SceneAs<Level>().Session.SetFlag(SetFlag);
            Ready = false;
        }
        Waiting = true;
    }
    [MethodImpl(MethodImplOptions.NoInlining)]
	public override void Update()
	{
		base.Update();
        if (!Waiting)
        {
            if(Timer > 0f) Timer -= Engine.DeltaTime;
        }
        else Waiting = false;
        if(Timer <= 0f)
        {
            Ready = true;
            Timer = 1f;
        }


        if(Math.Abs(AngularMomentum)<=0.01 && Math.Abs(Angle)<=0.1f)
        {
            AngularMomentum = 0;
            Angle = 0;
        }
        Angle += AngularMomentum * Engine.DeltaTime;
        if(Angle > 0)
        {
            if(AngularMomentum > 0) Inertia = 1.5f*InertiaBase;
            else Inertia = InertiaBase;
        }
        else
        {   
            if(AngularMomentum < 0) Inertia = -1.5f*InertiaBase;
            else Inertia = -InertiaBase;
        }
        AngularMomentum -= Inertia * Engine.DeltaTime;
        sprite.Rotation = Angle;
        if (Angle > 0.3f)
        {
            sprite.Play("left1");
        }
        else if (Angle > 0.2f)
        {
            sprite.Play("left0");
        }
        else if (Angle > -0.2f)
        {
            sprite.Play("idle");
        }
        else if (Angle > -0.3f)
        {
            sprite.Play("right0");
        }
        else
        {
            sprite.Play("right1");
        }
    }
  public override void Render()
  {
    sprite.DrawSimpleOutline();
    base.Render();
  }
}