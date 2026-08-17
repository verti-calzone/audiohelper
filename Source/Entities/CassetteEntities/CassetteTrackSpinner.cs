using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using Celeste.Mod.Entities;
using FMOD.Studio;
using Microsoft.Xna.Framework;
using Monocle;
using CelesteMod.Publicizer;

namespace Celeste.Mod.audiohelper.Entities;

[CustomEntity("audiohelper/CassetteTrackSpinner")]
[Tracked]
public class CassetteTrackSpinner : CassetteMover {
    public enum Styles {Blade, Dust, Starfish};
    public Styles Style;
    public string CustomSpeed;
    public float LocalTempo = 1f;


    // visuals
    public Sprite Sprite;
    public static ParticleType Particle = BladeTrackSpinner.P_Trail;

    // constructor
    public CassetteTrackSpinner(EntityData data, Vector2 offset) : base(data, offset)
    {
        // data
        Easer = data.Enum<CassetteMover.Easers>("Easer", Easers.SineInOut);
        Speed = data.Enum<CassetteMover.Speeds>("Speed", Speeds.FastStop);
        CustomSpeed = data.Attr("CustomSpeed");
        Style = data.Enum<Styles>("Style", Styles.Blade);
        LocalTempo = data.Float("Tempo");
        TickOffset = data.Int("Offset");


        // sending data to the base
        VertexList.Add(data.Position+offset);
        foreach(Vector2 node in data.Nodes) VertexList.Add(node+offset);
        Vertices = VertexList.ToArray();
        base.cassetteListener.Tempo = LocalTempo;

        base.Collider = new ColliderList(new Circle(6f), new Hitbox(16f, 4f, -8f, -3f));
        Add(new PlayerCollider(OnPlayer));

        Add(Sprite = GFX.SpriteBank.Create("templeBlade"));
        Sprite.Play("idle");
        base.Depth = -50;
    }

    public override void Update()
    {
        base.Update();
        if (Moving && base.Scene.OnInterval(0.04f)) SceneAs<Level>().ParticlesBG.Emit(Particle, 2, Position, Vector2.One * 3f);
    }

    public override void StartMove()
    {
        base.StartMove();
        Sprite.Play("spin");
        Audio.Play("event:/game/05_mirror_temple/bladespinner_spin", Position);
    }

    public override void EndMove()
    {
        base.EndMove();
    }

    public virtual void OnPlayer(Player player)
    {
        if (player.Die((player.Position - Position).SafeNormalize()) != null)
        {
            Moving = false;
            Frozen = true;
        }
    }
}