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

namespace Celeste.Mod.audiohelper.Entities;

[CustomEntity("audiohelper/CassetteMovingSolid")]
[Tracked]
public class CassetteMovingSolid : Solid
{
    public CassetteListener listener;
    public CassetteMover mover;

    public float yOffset, sinkTimer;
    public Vector2 newPosition, offset;

    // audiovisuals

    // constructor
    public CassetteMovingSolid(EntityData data, Vector2 offset) : base(data.Position + offset, data.Width, data.Height, safe: false)
    {
        // data
        Add(mover = new CassetteMover(OnMove, StartMove, EndMove, SilentUpdate));
        Add(listener = new CassetteListener(0));
        mover.easer = data.Enum<CassetteMover.Easers>("Easer", CassetteMover.Easers.SineInOut);
        mover.speed = data.Enum<CassetteMover.Speeds>("Speed", CassetteMover.Speeds.FastStop);
        mover.customSpeed = data.Attr("CustomSpeed");
        listener.Tempo = data.Float("Tempo");
        mover.tickOffset = data.Int("Offset");

        SurfaceSoundIndex = 35;
        Add(new LightOcclude(1f));

        // sending data to the base
        mover.vertexList.Add(data.Position + offset);
        foreach (Vector2 node in data.Nodes) mover.vertexList.Add(node + offset);
        mover.vertices = mover.vertexList.ToArray();
    }

    public override void Added(Scene scene)
    {
        base.Added(scene);
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
    }

    public override void Render()
    {
        
    }

    public void OnMove(Vector2 destination)
    {
        newPosition = destination;
        MoveTo(newPosition + offset);
    }
    public void StartMove()
    {
        
    }

    public void EndMove()
    {
        
    }
    public void SilentUpdate() { }
}