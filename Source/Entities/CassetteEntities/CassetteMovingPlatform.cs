using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Reflection.Metadata;
using Celeste.Mod.Entities;
using CelesteMod.Publicizer;
using FMOD.Studio;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using static Celeste.Mod.audiohelper.Entities.CassetteTrackSpinner;

namespace Celeste.Mod.audiohelper.Entities;

[CustomEntity("audiohelper/CassetteMovingPlatform")]
[Tracked]
public class CassetteMovingPlatform : JumpThru
{
    public CassetteListener listener;
    public CassetteMover mover;

    public float yOffset, sinkTimer;
    public Vector2 newPosition, offset;

    // audiovisuals
    public string texture;
    public MTexture[]  textures;
    public SoundSource sfx, sfx2;
    public bool sound;

    // constructor
    public CassetteMovingPlatform(EntityData data, Vector2 offset) : base(data.Position + offset, data.Width, safe: false)
    {
        // data
        Add(mover = new CassetteMover(OnMove, StartMove, EndMove, SilentUpdate));
        Add(listener = new CassetteListener(0));
        mover.easer = data.Enum<CassetteMover.Easers>("Easer", CassetteMover.Easers.SineInOut);
        mover.speed = data.Enum<CassetteMover.Speeds>("Speed", CassetteMover.Speeds.FastStop);
        mover.customSpeed = data.Attr("CustomSpeed");
        listener.Tempo = data.Float("Tempo");
        mover.tickOffset = data.Int("Offset");

        texture = data.Attr("Texture");
        SurfaceSoundIndex = 5;
        Add(sfx = new SoundSource());
        Add(sfx2 = new SoundSource());
        sfx.Position.X += Width / 2;
        sfx2.Position.X += Width / 2;
        sound = Calc.Random.Choose(true, false);

        // sending data to the base
        mover.vertexList.Add(data.Position + offset);
        foreach (Vector2 node in data.Nodes) mover.vertexList.Add(node + offset);
        mover.vertices = mover.vertexList.ToArray();
    }

    public override void Added(Scene scene)
    {
        base.Added(scene);
        MTexture mTexture = GFX.Game["objects/woodPlatform/" + texture];

        textures = new MTexture[mTexture.Width / 8];
        for (int i = 0; i < textures.Length; i++) textures[i] = mTexture.GetSubtexture(i * 8, 0, 8, 8);
        for (int i = 0; i < mover.vertices.Length; i++)
        {
            Vector2 start = mover.vertices[i];
            Vector2 end = mover.vertices[(i+1) % mover.vertices.Length];
            scene.Add(new MovingPlatformLine(new Vector2(start.X+Width/2, start.Y + 4), new Vector2(end.X+Width/2, end.Y + 4)));
        }
    }

    public override void Update()
    {
        base.Update();
        Vector2 dummy = Position;
        if (HasPlayerRider())
        {
            sinkTimer = 0.2f;
            yOffset = Calc.Approach(yOffset, 3f, 50f * Engine.DeltaTime);
        }
        else if (sinkTimer > 0f)
        {
            sinkTimer -= Engine.DeltaTime;
            yOffset = Calc.Approach(yOffset, 3f, 50f * Engine.DeltaTime);
        }
        else yOffset = Calc.Approach(yOffset, 0f, 20f * Engine.DeltaTime);
        offset.Y = yOffset;

        MoveTo(newPosition + offset);
        Logger.Info("audiohelper", "position is " + (newPosition + offset));
    }

    public override void Render()
    {
        textures[0].Draw(Position);
        for (int i = 8; (float)i < Width - 8f; i += 8) textures[1].Draw(Position + new Vector2(i, 0f));
        textures[3].Draw(Position + new Vector2(Width - 8f, 0f));
        textures[2].Draw(Position + new Vector2(Width / 2f - 4f, 0f));
    }

    public void OnMove(Vector2 destination)
    {
        newPosition = destination;
    }
    public void StartMove()
    {
        sound = !sound;
        sfx.Play(sound ? "event:/vert_audiohelper/movingplatform/move_1" : "event:/vert_audiohelper/movingplatform/move_2");
    }

    public void EndMove()
    {
        sfx2.Play(sound ? "event:/vert_audiohelper/movingplatform/move_1_end" : "event:/vert_audiohelper/movingplatform/move_2_end");
    }
    public void SilentUpdate() { }

    // fixes liftboost when horizontal only
    public override void MoveHExact(int move)
    {
        if (Collidable)
        {
            if (move < 0)
            {
                foreach (Actor entity in base.Scene.Tracker.GetEntities<Actor>())
                {
                    if (entity.IsRiding(this))
                    {
                        Collidable = false;
                        if (entity.TreatNaive) entity.NaiveMove(Vector2.UnitX * move);
                        else entity.MoveHExact(move);
                        entity.LiftSpeed = LiftSpeed;
                        Collidable = true;
                    }
                }
            }
            else
            {
                foreach (Actor entity2 in base.Scene.Tracker.GetEntities<Actor>())
                {
                    if (entity2.IsRiding(this))
                    {
                        Collidable = false;
                        if (entity2.TreatNaive) entity2.NaiveMove(Vector2.UnitX * move);
                        else entity2.MoveHExact(move);
                        entity2.LiftSpeed = LiftSpeed;
                        Collidable = true;
                    }
                }
            }
        }
        base.X += move;
        MoveStaticMovers(Vector2.UnitX * move);
    }
}