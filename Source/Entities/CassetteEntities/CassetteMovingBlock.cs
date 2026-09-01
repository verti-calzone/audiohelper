using System;
using System.Collections.Generic;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using static MonoMod.InlineRT.MonoModRule;

namespace Celeste.Mod.audiohelper.Entities;

[CustomEntity("audiohelper/CassetteMovingBlock")]
[Tracked]
public class CassetteMovingBlock : Solid
{
    public CassetteListener listener;
    public CassetteMover mover;
    public CassetteMovingBlockPath path;

    public Vector2 newPosition, offset, prevPosition;
    public float rate;
    public bool intense = false;

    // audiovisuals
    public MTexture[,] nineSlice;
    public MTexture cutoutTexture, rimTexture;
    public Sprite spool;
    public bool bigSprite;
    public string texture, sprite;
    public Color colour;

    public static readonly ParticleType P_SlowStop = new ParticleType
    {
        Color = Calc.HexToColor("ffe0d3"),
        Size = 1f,
        FadeMode = ParticleType.FadeModes.Late,
        SpeedMin = 20f,
        SpeedMax = 50f,
        SpeedMultiplier = 0.1f,
        DirectionRange = 0.6981317f,
        LifeMin = 0.5f,
        LifeMax = 1.2f
    };
    public static readonly BlendState subtract = new BlendState
    {
        ColorBlendFunction = BlendFunction.ReverseSubtract,
        AlphaBlendFunction = BlendFunction.ReverseSubtract,
        ColorSourceBlend = Blend.One,
        ColorDestinationBlend = Blend.One,
        AlphaSourceBlend = Blend.One,
        AlphaDestinationBlend = Blend.One
    };

    public static Dictionary<(string, Vector2), VirtualRenderTarget> textureDictionary = new();

    // constructor
    public CassetteMovingBlock(EntityData data, Vector2 offset) : base(data.Position + offset, data.Width, data.Height, safe: false)
    {
        // functional components
        Add(mover = new CassetteMover(OnMove, StartMove, EndMove, SilentUpdate));
        Add(listener = new CassetteListener(0));

        // data
        mover.easer = data.Enum<CassetteMover.Easers>("Easer", CassetteMover.Easers.SineInOut);
        mover.speed = data.Enum<CassetteMover.Speeds>("Speed", CassetteMover.Speeds.FastStop);
        mover.customSpeed = data.Attr("CustomSpeed");
        listener.Tempo = data.Float("Tempo");
        mover.tickOffset = data.Int("Offset");
        texture = data.Attr("Texture", "default");
        colour = data.HexColor("Colour", Calc.HexToColor("ffffff"));
        SurfaceSoundIndex = data.Int("SoundIndex", 35);

        // vfx
        Add(new LightOcclude(1f));
        Depth = Depths.FGTerrain + 1;
        if (mover.easer == CassetteMover.Easers.CubeIn)intense = true;

        // spinner sprite
        if (Width > 24 && Height > 24) bigSprite = true;
        else bigSprite = false;

        Add(spool = GFX.SpriteBank.Create("cassettemovingblock_spool_" + (bigSprite ? "big_" : "small_") + texture));
        spool.Position = Center - Position;
        spool.Rate = 0f;
        spool.UseRawDeltaTime = true;

        if (!textureDictionary.ContainsKey((texture, new Vector2(Width, Height)))) BakeTextures(texture);
        

        // sending data to the mover
        mover.vertexList.Add(data.Position + offset);
        foreach (Vector2 node in data.Nodes) mover.vertexList.Add(node + offset);
        mover.vertices = mover.vertexList.ToArray();
    }

    public void BakeTextures(string name)
    {
        
        VirtualRenderTarget blockTexture = VirtualContent.CreateRenderTarget("cmb-rendertarget", (int)Width, (int)Height);
        Engine.Graphics.GraphicsDevice.SetRenderTarget(blockTexture);

        Draw.SpriteBatch.Begin();
        MTexture mTexture = GFX.Game["objects/audiohelper/cassettemovingblock/" + name + "/block"];
        nineSlice = new MTexture[3, 3];
        for (int num = 0; num < 3; num++)
        {
            for (int num2 = 0; num2 < 3; num2++)
            {
                nineSlice[num, num2] = mTexture.GetSubtexture(new Rectangle(num * 8, num2 * 8, 8, 8));
            }
        }

        float colCount = Width / 8f - 1f;
        float rowCount = Height / 8f - 1f;

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
        cutoutTexture = GFX.Game["objects/audiohelper/cassettemovingblock/" + name + "/cutout_" + (bigSprite ? "big" : "small")];
        cutoutTexture.DrawCentered(Center - Position);
        Draw.SpriteBatch.End();

        Draw.SpriteBatch.Begin();
        bool useAlt = false;
        if (Height == 16 || (Width == 16 && Height == 24)) useAlt = true;
        string alt = useAlt ? "_alt" : string.Empty;
        rimTexture = GFX.Game["objects/audiohelper/cassettemovingblock/" + name + "/rim_" + (bigSprite ? "big" : "small") + alt];
        rimTexture.DrawCentered(Center - Position);
        Draw.SpriteBatch.End();

        textureDictionary.Add((name, new Vector2(Width, Height)), blockTexture);
    }

    public override void Added(Scene scene)
    {
        base.Added(scene);
    }

    public override void Update()
    {
        base.Update();

        float positionDifference = (prevPosition - Position).Length();
        if (positionDifference != 0f) rate = Calc.ClampedMap(positionDifference, 0f, 5f, 1f, 2.5f);
        else rate = 0f;

        spool.Rate = rate;
        path.spinRate = rate;

        prevPosition = Position;
    }

    public override void Render()
    {
        base.Render();

        spool.Render();

        textureDictionary.TryGetValue((texture, new Vector2(Width, Height)), out var vrt);
        Draw.SpriteBatch?.Draw((RenderTarget2D)vrt, Position + Shake, colour);
    }

    public void OnMove(Vector2 destination)
    {
        MoveTo(destination);
    }
    public void StartMove()
    {
        path.Start();
    }

    public void EndMove()
    {
        if (intense) StartShaking(0.2f);
        else StartShaking(0.1f);

        Audio.Play("event:/vert_audiohelper/cassettemovingblock/end", Center);

        Vector2 travelDirection = mover.vertices[mover.activeVertex] - mover.vertices[(mover.activeVertex - 1 + mover.vertices.Length) % mover.vertices.Length];
        ImpactParticles(travelDirection, intense);
        StopParticles(travelDirection, intense);

        path.Stop();
    }
    public void SilentUpdate()
    {
        Scene.Add(path = new CassetteMovingBlockPath(this, mover.ticksPerWait * mover.tickTimer));
    }

    public override void SceneEnd(Scene scene)
    {
        base.SceneEnd(scene);

        //dictionary.Dispose();
    }
    private void StopParticles(Vector2 travelDirection, bool intense)
    {
        Level level = SceneAs<Level>();

        ParticleType particle;
        int density;
        if (intense)
        {
            particle = FinalBossMovingBlock.P_Stop;
            density = 4;
        }
        else
        {
            particle = P_SlowStop;
            density = 6;
        }

        float direction = travelDirection.Angle();
        if (travelDirection.X > 0f)
        {
            Vector2 vector = new Vector2(Right - 1f, Top);
            for (int i = 0; i < Height; i += density)
            {
                level.Particles.Emit(particle, vector + Vector2.UnitY * (2 + i + Calc.Random.Range(-1, 1)), direction);
            }
        }
        else if (travelDirection.X < 0f)
        {
            Vector2 vector = new Vector2(Left, Top);
            for (int i = 0; i < Height; i += density)
            {
                level.Particles.Emit(particle, vector + Vector2.UnitY * (2 + i + Calc.Random.Range(-1, 1)), direction);
            }
        }
        if (travelDirection.Y > 0f)
        {
            Vector2 vector = new Vector2(Left, Bottom - 1f);
            for (int i = 0; i < Width; i += density)
            {
                level.Particles.Emit(particle, vector + Vector2.UnitX * (2 + i + Calc.Random.Range(-1, 1)), direction);
            }
        }
        else if (travelDirection.Y < 0f)
        {
            Vector2 vector = new Vector2(Left, Top);
            for (int i = 0; i < Width; i += density)
            {
                level.Particles.Emit(particle, vector + Vector2.UnitX * (2 + i + Calc.Random.Range(-1, 1)), direction);
            }
        }
    }
    public void ImpactParticles(Vector2 travelVector, bool intense)
    {
        ParticleType particle;
        if (intense) particle = FallingBlock.P_LandDust;
        else particle = ParticleTypes.Dust;

        Level level = SceneAs<Level>();

        if (travelVector.X < 0f)
        {
            for (int i = 0; i < base.Height / 8f; i++)
            {
                Vector2 tileSpot = new Vector2(Left - 1f, Top + 4f + (i * 8));
                if (!Scene.CollideCheck<Water>(tileSpot) && Scene.CollideCheck<Solid>(tileSpot))
                {
                    if (tileSpot.Y <= Center.Y) level.ParticlesFG.Emit(particle, tileSpot, -MathF.PI / 2);
                    if (tileSpot.Y >= Center.Y) level.ParticlesFG.Emit(particle, tileSpot, MathF.PI / 2);
                }
            }
        }
        else if (travelVector.X > 0f)
        {
            for (int i = 0; i < base.Height / 8f; i++)
            {
                Vector2 tileSpot = new Vector2(Right + 1f, Top + 4f + (i * 8));
                if (!Scene.CollideCheck<Water>(tileSpot) && Scene.CollideCheck<Solid>(tileSpot))
                {
                    if (tileSpot.Y <= Center.Y) level.ParticlesFG.Emit(particle, tileSpot, -MathF.PI / 2);
                    if (tileSpot.Y >= Center.Y) level.ParticlesFG.Emit(particle, tileSpot, MathF.PI / 2);
                }
            }
        }
        if (travelVector.Y < 0f)
        {
            for (int i = 0; i < base.Width / 8f; i++)
            {
                Vector2 tileSpot = new Vector2(Left + 4f + (i * 8), Top - 1f);
                if (!Scene.CollideCheck<Water>(tileSpot) && Scene.CollideCheck<Solid>(tileSpot))
                {
                    if (tileSpot.X <= Center.X) level.ParticlesFG.Emit(particle, tileSpot, MathF.PI);
                    if (tileSpot.X >= Center.X) level.ParticlesFG.Emit(particle, tileSpot, 0f);
                }
            }
        }
        else
        {
            if (!(travelVector.Y > 0f)) return; 
            for (int i = 0; i < base.Width / 8f; i++)
            {
                Vector2 tileSpot = new Vector2(Left + 4f + (i * 8), Bottom + 1f);
                if (!Scene.CollideCheck<Water>(tileSpot) && Scene.CollideCheck<Solid>(tileSpot))
                {
                    if (tileSpot.X <= Center.X) level.ParticlesFG.Emit(particle, tileSpot, MathF.PI);
                    if (tileSpot.X >= Center.X) level.ParticlesFG.Emit(particle, tileSpot, 0f);
                }
            }
        }
    }
}