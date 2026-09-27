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
    public static Dictionary<float, VirtualRenderTarget> textureDictionary = [];
    public Vector2 pivot, renderPosition;
    public float radius;
    public bool big;
    public int padding;
    public static Color outerColour = Calc.HexToColor("302838");
    public static Color innerColour = Calc.HexToColor("403848");
    public CassetteRotatingBlockPath(Vector2 location, float size, bool bigSprite) 
    {
        radius = size;
        big = bigSprite;
        padding = big ? 11 : 5;
        renderPosition = location - new Vector2(radius + padding, radius + padding);
        Depth = Depths.BGDecals - 1;
        if (!textureDictionary.ContainsKey(radius)) BakeTextures(radius, big, padding);
    }
    public static void BakeTextures(float radius, bool big, int padding)
    {
        int size = (int)(radius + padding) * 2;
        int innerPadding = big ? 9 : 4;

        VirtualRenderTarget circleTexture = VirtualContent.CreateRenderTarget("crb-rendertarget", size, size);
        Engine.Graphics.GraphicsDevice.SetRenderTarget(circleTexture);

        Vector2 localPivot = new(radius + padding, radius + padding);

        int segments = (int)Calc.Clamp(radius / 4, 4, 16);

        Draw.SpriteBatch.Begin();
        Draw.Circle(localPivot, radius + padding, outerColour, segments);
        Draw.Circle(localPivot, radius + padding - 1, innerColour, segments);

        Draw.Circle(localPivot, radius - innerPadding, outerColour, segments);
        Draw.Circle(localPivot, radius - innerPadding + 1, innerColour, segments);
        Draw.SpriteBatch.End();

        textureDictionary.Add(radius, circleTexture);
    }

    public override void Render()
    {
        base.Render();
        textureDictionary.TryGetValue(radius, out var vrt);
        Draw.SpriteBatch?.Draw((RenderTarget2D)vrt, renderPosition, Color.White);
    }
}