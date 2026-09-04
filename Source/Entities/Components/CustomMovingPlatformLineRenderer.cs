using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Xml.Linq;
using Celeste.Mod.Entities;
using Celeste.Mod.Helpers;
using FMOD.Studio;
using Iced.Intel;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.Cil;
using MonoMod.Utils;


// UNUSED //


namespace Celeste.Mod.audiohelper.Entities;

[CustomEntity("audiohelper/CustomMovingPlatformLineRenderer")]
[Tracked(true)]
public class CustomMovingPlatformLineRenderer : Entity {

    public Vector2[] vertices;
    public Color innerColour, outerColour;
    public int numLines;
    public float platformWidth;

    public MTexture inner, outer;
  	public CustomMovingPlatformLineRenderer(Vector2[] nodes, float givenWidth, string style) : base()
    {
        Depth = 9001;

        vertices = new Vector2[nodes.Length];
        platformWidth = givenWidth;

        for (int i = 0; i < nodes.Length; i++)
        {
            vertices[i].X = (int)(nodes[i].X + platformWidth / 2);
            vertices[i].Y = (int)(nodes[i].Y + 4);
        }

        if (style == "cliffside")
        {
            outerColour = Calc.HexToColor("a4464a");
            innerColour = Calc.HexToColor("86354e");
        }
        else
        {
            outerColour = Calc.HexToColor("2a1923");
            innerColour = Calc.HexToColor("160b12");
        }

        inner = GFX.Game["objects/audiohelper/cassettemovingplatformline/inner"];
        outer = GFX.Game["objects/audiohelper/cassettemovingplatformline/outer"];

        if (vertices.Length == 2) numLines = 1;
        else numLines = vertices.Length;
    }

    public override void Added(Scene scene)
    {
        base.Added(scene);
    }
    public override void Render()
    {
        for (int i = 0; i < numLines; i++)
        {
            Vector2 start = vertices[i];
            Vector2 end = vertices[(i + 1) % vertices.Length];
            Vector2 lineStart = start;
            Vector2 lineEnd = end;

            if (start.X < end.X)
            {
                lineStart.Y -= 1;
                lineEnd.Y -= 1;
            }
            if (start.Y > end.Y)
            {
                lineStart.X -= 0.5f;
                lineEnd.X -= 0.5f;
            }

            DrawAll(start, end, lineStart, lineEnd, Vector2.UnitX, outerColour);
            DrawAll(start, end, lineStart, lineEnd, -Vector2.UnitX, outerColour);
            DrawAll(start, end, lineStart, lineEnd, Vector2.UnitY, outerColour);
            DrawAll(start, end, lineStart, lineEnd, -Vector2.UnitY, outerColour);
        }
        for (int i = 0; i < numLines; i++)
        {
            Vector2 start = vertices[i];
            Vector2 end = vertices[(i + 1) % vertices.Length];
            Vector2 lineStart = start;
            Vector2 lineEnd = end;

            if (start.X < end.X)
            {
                lineStart.Y -= 1;
                lineEnd.Y -= 1;
            }
            if (start.Y > end.Y)
            {
                lineStart.X -= 0.5f;
                lineEnd.X -= 0.5f;
            }

            DrawAll(start, end, lineStart, lineEnd, Vector2.Zero, innerColour);
        }
        base.Render();
    }
    public void DrawAll(Vector2 start, Vector2 end, Vector2 lineStart, Vector2 lineEnd, Vector2 offset, Color colour)
    {
        Draw.Line(lineStart + offset, lineEnd + offset, colour);
        inner.DrawCentered(start + offset, colour);
        if (numLines == 1) inner.DrawCentered(end + offset, colour);
    }
}