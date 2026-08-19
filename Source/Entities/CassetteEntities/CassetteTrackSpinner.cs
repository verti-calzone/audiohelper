using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Reflection.Metadata;
using Celeste.Mod.Entities;
using CelesteMod.Publicizer;
using FMOD.Studio;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.audiohelper.Entities;

[CustomEntity("audiohelper/CassetteTrackSpinner")]
[Tracked]
public class CassetteTrackSpinner : CassetteMover {
    public enum Styles { Blade, Dust, Starfish };
    public Styles Style;


    // visuals
    public Sprite Sprite;
    public static ParticleType BladeParticle = BladeTrackSpinner.P_Trail;
    public static ParticleType DustParticle = DustStaticSpinner.P_Move;
    public static ParticleType[] StarfishParticle = StarTrackSpinner.P_Trail;
    public ParticleType Particle;
    public int ColourID;
    public DustGraphic Dust;
    public Vector2 TargetFacingAngle;

    // constructor
    public CassetteTrackSpinner(EntityData data, Vector2 offset) : base(data, offset)
    {
        // data
        Easer = data.Enum<CassetteMover.Easers>("Easer", Easers.SineInOut);
        Speed = data.Enum<CassetteMover.Speeds>("Speed", Speeds.FastStop);
        CustomSpeed = data.Attr("CustomSpeed");
        cassetteListener.Tempo = data.Float("Tempo");
        TickOffset = data.Int("Offset");

        Style = data.Enum<Styles>("Style", Styles.Blade);

        // sending data to the base
        VertexList.Add(data.Position + offset);
        foreach (Vector2 node in data.Nodes) VertexList.Add(node + offset);
        Vertices = VertexList.ToArray();

        base.Collider = new ColliderList(new Circle(6f), new Hitbox(16f, 4f, -8f, -3f));
        Add(new PlayerCollider(OnPlayer));


        // Creating the sprite
        if (Style == Styles.Starfish)
        {
            Add(Sprite = GFX.SpriteBank.Create("moonBlade"));
            ColourID = Calc.Random.Choose(0, 1, 2);
            Sprite.Play("idle" + ColourID);
        }
        else if (Style == Styles.Dust)
        {
            Add(Dust = new DustGraphic(ignoreSolids: true));
            Dust.eyesMoveByRotation = true;
            Particle = DustParticle;   
        }
        else // fallback to blade
        {
            Add(Sprite = GFX.SpriteBank.Create("templeBlade"));
            Sprite.Play("idle");
            Particle = BladeParticle;
        }
        Add(new MirrorReflection());
        Depth = -50;
    }

    public override void SilentUpdate(int TicksUntilReset, int BpT, int TpS, float tempoMult)
    {
        base.SilentUpdate(TicksUntilReset, BpT, TpS, tempoMult);
        if (Style == Styles.Dust)
        {
            Vector2 eyeDir = (Vertices[(ActiveVertex + 1) % Vertices.Length] - Vertices[ActiveVertex]).SafeNormalize();
            Dust.EyeDirection = eyeDir;
            Dust.EyeTargetDirection = eyeDir;
        }
    }

    public override void Update()
    {
        base.Update();
        if (Moving && base.Scene.OnInterval(0.04f))
        {
            if (Style == Styles.Starfish) SceneAs<Level>().ParticlesBG.Emit(StarfishParticle[ColourID], 1, Position, Vector2.One * 3f);
            else if (Style == Styles.Dust)
            {
                SceneAs<Level>().ParticlesBG.Emit(DustParticle, 1, Position, Vector2.One * 4f);
            }
            else SceneAs<Level>().ParticlesBG.Emit(BladeParticle, 2, Position, Vector2.One * 3f); // fallback to blade
        }
    }

    public override void StartMove()
    {
        base.StartMove();

        if (Style == Styles.Starfish)
        {
            ColourID++;
            ColourID %= 3;
            Sprite.Play("spin" + ColourID);
        }
        else if (Style == Styles.Dust) return; // skips the audio call
        else Sprite.Play("spin"); // fallback to blade
        Audio.Play("event:/game/05_mirror_temple/bladespinner_spin", Position);
    }

    public override void EndMove()
    {
        base.EndMove();
        if (Style == Styles.Dust) Dust.EyeTargetDirection = (Vertices[(ActiveVertex + 1) % Vertices.Length] - Vertices[ActiveVertex]).SafeNormalize(); 
    }

    public virtual void OnPlayer(Player player)
    {
        if (player.Die((player.Position - Position).SafeNormalize()) != null)
        {
            Moving = false;
            Frozen = true;
        }
        if (Style == Styles.Dust)
        {
            Dust.OnHitPlayer();
        }
    }
}