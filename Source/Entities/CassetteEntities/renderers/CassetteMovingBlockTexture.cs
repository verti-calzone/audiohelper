using System;
using System.Collections.Generic;
using Celeste.Mod.Entities;
using Celeste.Mod.Helpers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;

namespace Celeste.Mod.audiohelper.Entities;

public static class CassetteMovingBlockTexture
{
    public static Dictionary<(string, Vector2), VirtualRenderTarget> textureDictionary = [];

    public static readonly BlendState subtract = new BlendState
    {
        ColorBlendFunction = BlendFunction.ReverseSubtract,
        AlphaBlendFunction = BlendFunction.ReverseSubtract,
        ColorSourceBlend = Blend.One,
        ColorDestinationBlend = Blend.One,
        AlphaSourceBlend = Blend.One,
        AlphaDestinationBlend = Blend.One
    };

    public static void BakeTextures(string name, Entity entity, bool bigSprite)
    {
        VirtualRenderTarget blockTexture = VirtualContent.CreateRenderTarget("cmb-rendertarget", (int)entity.Width, (int)entity.Height);
        Engine.Graphics.GraphicsDevice.SetRenderTarget(blockTexture);

        Draw.SpriteBatch.Begin();
        MTexture mTexture = GFX.Game["objects/audiohelper/cassettemovingblock/" + name + "/block"];
        MTexture[,] nineSlice = new MTexture[3, 3];
        for (int num = 0; num < 3; num++)
        {
            for (int num2 = 0; num2 < 3; num2++)
            {
                nineSlice[num, num2] = mTexture.GetSubtexture(new Rectangle(num * 8, num2 * 8, 8, 8));
            }
        }

        float colCount = entity.Width / 8f - 1f;
        float rowCount = entity.Height / 8f - 1f;

        for (int col = 0; col <= colCount; col++)
        {
            for (int row = 0; row <= rowCount; row++)
            {
                int colTile = ((col < colCount) ? Math.Min(col, 1) : 2);
                int rowTile = ((row < rowCount) ? Math.Min(row, 1) : 2);
                nineSlice[colTile, rowTile].Draw(new Vector2(col * 8, row * 8));
            }
        }
        Draw.SpriteBatch.End();

        Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, subtract);
        MTexture cutoutTexture = GFX.Game["objects/audiohelper/cassettemovingblock/" + name + "/cutout_" + (bigSprite ? "big" : "small")];
        cutoutTexture.DrawCentered(entity.Center - entity.Position);
        Draw.SpriteBatch.End();

        Draw.SpriteBatch.Begin();
        bool useAlt = false;
        if (entity.Height == 16 || (entity.Width == 16 && entity.Height == 24)) useAlt = true;
        string alt = useAlt ? "_alt" : string.Empty;
        MTexture rimTexture = GFX.Game["objects/audiohelper/cassettemovingblock/" + name + "/rim_" + (bigSprite ? "big" : "small") + alt];
        rimTexture.DrawCentered(entity.Center - entity.Position);
        Draw.SpriteBatch.End();

        textureDictionary.Add((name, new Vector2(entity.Width, entity.Height)), blockTexture);
    }
}