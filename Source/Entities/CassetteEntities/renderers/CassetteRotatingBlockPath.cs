using System;
using System.Collections.Generic;
using System.Xml.Linq;
using Celeste.Mod.Entities;
using Celeste.Mod.Helpers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;

namespace Celeste.Mod.audiohelper.Entities;

[CustomEntity("audiohelper/CassetteRotatingBlockPath")]
[Tracked]
public class CassetteRotatingBlockPath : Entity
{
    public static Dictionary<(float, bool), VirtualRenderTarget> textureDictionary = [];
    public Vector2 pivot, renderPosition;
    public float radius;
    public bool big;
    public int outerPadding, innerPadding;
    public static Color outerColour = Calc.HexToColor("343440");
    public static Color innerColour = Calc.HexToColor("4b4b59");
    public static Color toothColour = Calc.HexToColor("606068");
    public CassetteRotatingBlockPath(Vector2 location, float size, bool bigSprite) 
    {
        radius = size;
        big = bigSprite;
        outerPadding = big ? 11 : 5;
        innerPadding = big ? 9 : 4;
        renderPosition = location - new Vector2(radius + outerPadding, radius + outerPadding);
        Depth = Depths.BGDecals - 1;
        if (!textureDictionary.ContainsKey((radius, big))) BakeTextures();
    }
    public void BakeTextures()
    {
        int size = (int)(radius + outerPadding) * 2;

        VirtualRenderTarget pathTexture = VirtualContent.CreateRenderTarget("crb-rendertarget", size, size);
        Engine.Graphics.GraphicsDevice.SetRenderTarget(pathTexture);

        Vector2 localPivot = new(radius + outerPadding, radius + outerPadding);

        int segments = (int)Calc.Clamp(radius / 4, 4, 16);

        Draw.SpriteBatch.Begin();

        DrawInners(localPivot + Vector2.UnitY, true, segments);
        DrawInners(localPivot, false, segments);

        Draw.Circle(localPivot, radius + outerPadding, outerColour, segments);
        Draw.Circle(localPivot, radius - innerPadding, outerColour, segments);

        Draw.SpriteBatch.End();

        textureDictionary.Add((radius, big), pathTexture);
    }

    public void DrawInners(Vector2 centre, bool shadow, int segments)
    {
        Draw.Circle(centre, radius + outerPadding - 1, shadow ? Color.Black : innerColour, segments);
        Draw.Circle(centre, radius - innerPadding + 1, shadow ? Color.Black : innerColour, segments);

        DrawTeeth(centre, shadow ? Color.Black : toothColour, true);
        DrawTeeth(centre, shadow ? Color.Black : toothColour, false);
    }

    public void DrawTeeth(Vector2 centre, Color colour, bool inner)
    {
        float circumfrence = (radius + (inner ? -innerPadding + 2 : outerPadding - 2)) * MathF.Tau;
        int resolution = 6;

        for (int i = 0; i < circumfrence/resolution; i++)
        {
            float startAngle = (MathF.Tau * i) / circumfrence * resolution;
            float endAngle = (MathF.Tau * (i + 0.5f)) / circumfrence * resolution;

            Vector2 start = Calc.AngleToVector(startAngle, radius + (inner ? -innerPadding + 2 : outerPadding - 2));
            Vector2 end = Calc.AngleToVector(endAngle, radius + (inner ? -innerPadding + 2 : outerPadding - 2));

            Draw.Line(centre + start, centre + end, colour);
        }
    }

    public override void Render()
    {
        base.Render();
        textureDictionary.TryGetValue((radius, big), out var vrt);
        Draw.SpriteBatch?.Draw((RenderTarget2D)vrt, renderPosition, Color.White);
    }
}