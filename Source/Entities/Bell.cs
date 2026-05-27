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
    public float Timer = 0.1f;
    public float Speed;
    public float Angle;
    public float AngularMomentum;
    public float Inertia;
    public float InertiaBase = 10;
    public MultiParameterSoundSource sfx;
    private Collider MainCollider;
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
        Add(new HoldableCollider(OnHoldable));
        Add(new SeekerCollider(OnSeeker));
        Depth = 2000;
    }

    private void OnPlayer(Player player)
    {
        if(Ready) Ring(player.Speed.X,player.Speed.Y);
        Waiting = true;
    }

    private void OnHoldable(Holdable holdable)
    {
        if(Ready) Ring(holdable.GetSpeed().X,holdable.GetSpeed().Y);
        Waiting = true;
    }

    private void OnSeeker(Seeker seeker)
    {
        if(Ready) Ring(seeker.Speed.X,seeker.Speed.Y);
        Waiting = true;
    }

    private void OnChaser()
    {
        if(Ready) Ring(200,0);
        Waiting = true;
    }

    public virtual void Ring(float xSpeed, float ySpeed)
    {
        Speed = (float)Math.Sqrt(xSpeed*xSpeed+ySpeed*ySpeed) + 500*VolumeBoost;
        Speed = Calc.Clamp(Speed, 200f, 500f);
        if (!string.IsNullOrEmpty(Sound)) sfx.Play(Sound,"pitch",Pitch,"speed",Speed*0.002f,false);
        AngularMomentum = 0.01f*Speed;
        if(Math.Sign(xSpeed) == 1) AngularMomentum *= -1;
        if(!string.IsNullOrEmpty(SetFlag)) SceneAs<Level>().Session.SetFlag(SetFlag);
        Ready = false;
    }
    [MethodImpl(MethodImplOptions.NoInlining)]
	public override void Update()
	{
		base.Update();

        MainCollider = Collider;
        Collider = new Circle(16f);
        foreach(BadelineOldsite chaser in SceneAs<Level>().Tracker.GetEntities<BadelineOldsite>()) if(Collider.Collide(chaser)) OnChaser();
        Collider = MainCollider;

        // if (!Waiting)
        // {
        //     if(Timer > 0f) Timer -= Engine.DeltaTime;
        // }
        // else Waiting = false;
        // if(Timer <= 0f)
        // {
        //     Ready = true;
        //     Timer = 0.1f;
        // }

        if (!Waiting) Ready = true;
        Waiting = false;


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
        if (Angle > 0.3f) sprite.Play("left1");
        else if (Angle > 0.2f) sprite.Play("left0");
        else if (Angle > -0.2f) sprite.Play("idle");
        else if (Angle > -0.3f) sprite.Play("right0");
        else sprite.Play("right1");
    }
  public override void Render()
  {
    sprite.DrawSimpleOutline();
    base.Render();
  }
}