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

[CustomEntity("audiohelper/CassetteMovingPlatform")]
[Tracked]
public class CassetteMovingPlatform : CassetteMover
{
    public string Texture;
    public MTexture[]  Textures;
    public int Width;

    // constructor
    public CassetteMovingPlatform(EntityData data, Vector2 offset) : base(data, offset)
    {
        // data
        Easer = data.Enum<CassetteMover.Easers>("Easer", Easers.SineInOut);
        Speed = data.Enum<CassetteMover.Speeds>("Speed", Speeds.FastStop);
        CustomSpeed = data.Attr("CustomSpeed");
        cassetteListener.Tempo = data.Float("Tempo");
        TickOffset = data.Int("Offset");
        Width = data.Int("Width");

        Texture = data.Attr("Texture");

        // sending data to the base
        VertexList.Add(data.Position + offset);
        foreach (Vector2 node in data.Nodes) VertexList.Add(node + offset);
        Vertices = VertexList.ToArray();
    }

    public override void Added(Scene scene)
    {
        base.Added(scene);
        MTexture mTexture = GFX.Game["objects/woodPlatform/" + Texture];

        Textures = new MTexture[mTexture.Width / 8];
        for (int i = 0; i < Textures.Length; i++) Textures[i] = mTexture.GetSubtexture(i * 8, 0, 8, 8);
        for (int i = 0; i < Vertices.Length; i++) scene.Add(new MovingPlatformLine(new Vector2(Vertices[i].X, Vertices[i].Y + 4), new Vector2(Vertices[(i + 1) % Vertices.Length].X, Vertices[(i + 1) % Vertices.Length].Y + 4)));
    }

    public override void Update()
    {
        base.Update();
    }

    public override void Render()
    {
        Textures[0].Draw(Position);
        for (int i = 8; (float)i < Width - 8f; i += 8) Textures[1].Draw(Position + new Vector2(i, 0f));
        Textures[3].Draw(Position + new Vector2(Width - 8f, 0f));
        Textures[2].Draw(Position + new Vector2(Width / 2f - 4f, 0f));
    }

    public override void StartMove()
    {
        base.StartMove();
    }

    public override void EndMove()
    {
        base.EndMove();
    }
}