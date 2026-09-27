using System;
using System.Collections.Generic;
using System.Xml.Linq;
using Celeste.Mod.Entities;
using Celeste.Mod.Helpers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;

namespace Celeste.Mod.audiohelper.Entities;

[CustomEntity("audiohelper/CassetteRotatingPlatformCircle")]
[Tracked]
public class CassetteRotatingPlatformCircle : Entity
{
    public static Dictionary<(bool, float), VirtualRenderTarget> textureDictionary = [];
    public Vector2 pivot, renderPosition;
    public bool isCliffside;
    public float radius;
    public CassetteRotatingPlatformCircle(Vector2 location, bool cliffside, float size) 
    {
        isCliffside = cliffside;
        radius = size;
        renderPosition = location + 4 * Vector2.UnitY - new Vector2(radius + 1, radius + 1);
        Depth = Depths.BGDecals - 1;
        if (!textureDictionary.ContainsKey((isCliffside, radius))) BakeTextures(isCliffside, radius);
    }
    public static void BakeTextures(bool cliffside, float radius)
    {
        int size = (int)(radius + 1) * 2;
        VirtualRenderTarget circleTexture = VirtualContent.CreateRenderTarget("crp-rendertarget", size, size);
        Engine.Graphics.GraphicsDevice.SetRenderTarget(circleTexture);

        Vector2 localPivot = new(radius + 1, radius + 1);
        Color outerColour, innerColour;
        if (cliffside)
        {
            outerColour = Calc.HexToColor("a4464a");
            innerColour = Calc.HexToColor("86354e");
        }
        else
        {
            outerColour = Calc.HexToColor("2a1923");
            innerColour = Calc.HexToColor("160b12");
        }
        int segments = (int)Calc.Clamp(radius / 4, 2, 16);

        Draw.SpriteBatch.Begin();
        Draw.Circle(localPivot, radius - 1, outerColour, segments);
        Draw.Circle(localPivot, radius + 1, outerColour, segments);
        Draw.Circle(localPivot, radius, innerColour, segments);
        Draw.SpriteBatch.End();

        textureDictionary.Add((cliffside, radius), circleTexture);
    }

    public override void Render()
    {
        base.Render();
        textureDictionary.TryGetValue((isCliffside, radius), out var vrt);
        Draw.SpriteBatch?.Draw((RenderTarget2D)vrt, renderPosition, Color.White);
    }
}