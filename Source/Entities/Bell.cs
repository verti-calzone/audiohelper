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
    public Color Colour = Calc.HexToColor("ffffff");
    public bool Ready;
    public bool Waiting = false;
    public float Timer;
    public float Speed;
    public float Angle;
    public float AnglularMomentum;
    public float Inertia;
    public float InertiaBase = 10;
    public MultiParameterSoundSource sfx = new MultiParameterSoundSource();
    public Bell(EntityData data, Vector2 offset, EntityID id) : base(data.Position + offset)
    {
        Sound = data.Attr("sound");
        Pitch = data.Int("pitch");
        Colour = data.HexColor("colour");
        Add(sprite = GFX.SpriteBank.Create("audiohelper_bell"));
        // sprite.Scale.X = 0.5f;
        // sprite.Scale.Y = 0.5f;
        sprite.Color = Colour;
        Add(sfx = new MultiParameterSoundSource());
        sfx.Position.Y = 8;
        Collider = new Circle(12f);
        Collider.Position.Y = 8;
        Add(new PlayerCollider(OnPlayer));
        Depth = 2000;
    }
    public override void Awake(Scene scene)
    {
        base.Awake(scene);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private void OnPlayer(Player player)
    {
        if(Ready)
        {
            Player entity = base.Scene.Tracker.GetEntity<Player>();
            Speed = (float)Math.Sqrt(entity.Speed.X*entity.Speed.X+entity.Speed.Y*entity.Speed.Y);
            if (!string.IsNullOrEmpty(Sound)){
                sfx.Play(Sound,"pitch", (float)Pitch,"speed",Speed*0.002f,false);
            }
            if(entity.Speed.X == 0)
            {
                AnglularMomentum = -2f;
            }
            else if(Math.Abs(entity.Speed.X) <= 200)
            {
                AnglularMomentum = Math.Sign(entity.Speed.X)*-2f;
            }
            else
            {
                AnglularMomentum = entity.Speed.X*-0.01f;
            }
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
            if(Timer > 0f)
            {
                Timer -= Engine.DeltaTime;
            }
        }
        else
        {
            Waiting = false;
        }
        if(Timer <= 0f)
        {
            Ready = true;
            Timer = 1f;
        }


        if(Math.Abs(AnglularMomentum)<=0.01 && Math.Abs(Angle)<=0.1f)
        {
            AnglularMomentum = 0;
            Angle = 0;
        }
        Angle += AnglularMomentum * Engine.DeltaTime;
        if(Angle > 0)
        {
            if(AnglularMomentum > 0)
            {
                Inertia = 1.5f*InertiaBase;
            }
            else
            {
                Inertia = InertiaBase;
            }
        }
        else
        {   
            if(AnglularMomentum < 0)
            {
                Inertia = -1.5f*InertiaBase;
            }
            else
            {
                Inertia = -InertiaBase;
            }
        }
        AnglularMomentum -= Inertia * Engine.DeltaTime;
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