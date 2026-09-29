using System;
using System.Collections.Generic;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;

namespace Celeste.Mod.audiohelper.Entities;

[CustomEntity("audiohelper/CassetteRotatingBlock")]
[Tracked]
public class CassetteRotatingBlock : Solid
{
    public CassetteListener listener;
    public CassetteRotator rotator;
    //public CassetteRotatingBlockPath path;

    public Vector2 positionOffset, pivot;
    public float radius;

    // audiovisuals
    public Sprite spool;
    public bool bigSprite;
    public string texture, sprite;
    public Color colour;

    // constructor
    public CassetteRotatingBlock(EntityData data, Vector2 offset) : base(data.Position + offset, data.Width, data.Height, safe: false)
    {
        Tag = Tags.TransitionUpdate;
        AddTag(TagsExt.FreezeFrameUpdate);

        // functional components
        radius = data.Float("Radius");
        int TpC = data.Int("TicksPerCycle");
        Add(rotator = new CassetteRotator(OnMove, SilentUpdate, OnSwap, data.Float("AngleOffset"), radius, TpC, data.Bool("Clockwise")));
        Add(listener = new CassetteListener(0));

        // data
        listener.Tempo = data.Float("Tempo");
        texture = data.Attr("Texture", "default");
        colour = data.HexColor("Colour", Calc.HexToColor("ffffff"));
        SurfaceSoundIndex = data.Int("SoundIndex", 35);
        pivot = data.Position + offset;
        positionOffset = new Vector2(-Width/2, -Height/2);

        // vfx
        Add(new LightOcclude(1f));
        Depth = Depths.FGTerrain + 1;

        // spinner sprite
        if (Width > 24 && Height > 24) bigSprite = true;
        else bigSprite = false;

        Add(spool = GFX.SpriteBank.Create("cassettemovingblock_spool_" + (bigSprite ? "big_" : "small_") + texture));
        spool.Position = Center - Position;
        spool.Rate = 0.5f + Calc.Clamp((radius*MathF.PI)/(TpC*8), 0f, 2f);
        spool.UseRawDeltaTime = true;

        if (!CassetteMovingBlockTexture.textureDictionary.ContainsKey((texture, new Vector2(Width, Height)))) CassetteMovingBlockTexture.BakeTextures(texture, this, bigSprite);
    }
    public override void Added(Scene scene)
    {
        base.Added(scene);
        Scene.Add(new CassetteRotatingBlockPath(pivot, radius, bigSprite));
    }

    public override void Update()
    {
        base.Update();
    }

    public override void Render()
    {
        base.Render();

        spool.Render();

        CassetteMovingBlockTexture.textureDictionary.TryGetValue((texture, new Vector2(Width, Height)), out var vrt);
        Draw.SpriteBatch?.Draw((RenderTarget2D)vrt, Position + Shake, colour);
    }

    public void OnMove(Vector2 destination)
    {
        MoveTo(pivot + destination + positionOffset);
    }

    public void SilentUpdate()
    {

    }

    public void OnSwap()
    {

    }
}