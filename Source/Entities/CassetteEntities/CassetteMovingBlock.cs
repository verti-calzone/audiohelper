using System;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.audiohelper.Entities;

[CustomEntity("audiohelper/CassetteMovingBlock")]
[Tracked]
public class CassetteMovingBlock : Solid
{
    public CassetteListener listener;
    public CassetteMover mover;
    public CassetteMovingBlockPath path;

    public float yOffset, sinkTimer;
    public Vector2 newPosition, offset;

    // audiovisuals
    public MTexture[,] solidNineSlice, cutoutNineSlice;
    public MTexture paddingTexture;
    public Sprite spinner;
    public Vector2 iconOffset;
    public SoundSource sfx;
    public float rate;
    public bool bigSprite;

    public static ParticleType P_SlowStop;
// constructor
public CassetteMovingBlock(EntityData data, Vector2 offset) : base(data.Position + offset, data.Width, data.Height, safe: false)
    {
        // data
        Add(mover = new CassetteMover(OnMove, StartMove, EndMove, SilentUpdate));
        Add(listener = new CassetteListener(0));
        mover.easer = data.Enum<CassetteMover.Easers>("Easer", CassetteMover.Easers.SineInOut);
        mover.speed = data.Enum<CassetteMover.Speeds>("Speed", CassetteMover.Speeds.FastStop);
        mover.customSpeed = data.Attr("CustomSpeed");
        listener.Tempo = data.Float("Tempo");
        mover.tickOffset = data.Int("Offset");

        // audio
        SurfaceSoundIndex = 35;
        Add(sfx = new SoundSource());
        sfx.Position = Center;

        // vfx
        P_SlowStop = new ParticleType
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
        Add(new LightOcclude(1f));
        Depth = Depths.FGTerrain + 1;

        // textures
        MTexture mTexture = GFX.Game["objects/audiohelper/cassettemovingblock/" + data.Attr("spriteName")];
        solidNineSlice = new MTexture[3, 3];
        for (int num = 0; num < 3; num++)
        {
            for (int num2 = 0; num2 < 3; num2++) solidNineSlice[num, num2] = mTexture.GetSubtexture(new Rectangle(num * 8, num2 * 8, 8, 8));
        }
        MTexture mTexture2 = GFX.Game["objects/audiohelper/cassettemovingblock/" + data.Attr("spriteName")+"_cutout"];
        cutoutNineSlice = new MTexture[3, 3];
        for (int num = 0; num < 3; num++)
        {
            for (int num2 = 0; num2 < 3; num2++) cutoutNineSlice[num, num2] = mTexture2.GetSubtexture(new Rectangle(num * 8, num2 * 8, 8, 8));
        }

        // padding texture
        string paddingSize;
        bool evenWidth = false, evenHeight = false;

        if (int.IsEvenInteger((int)(Width / 8))) evenWidth = true;
        if (int.IsEvenInteger((int)(Height / 8))) evenHeight = true;

        if (Width <= 24 && Height <= 24) paddingSize = "1x1"; // 3x3 or smaller
        else if (Width == 16) paddingSize = "1x2"; // 2x4+ edge case
        else if (Height == 16) paddingSize = "2x1"; // 4+x2 edge case
        else if (Width == 24 && evenHeight) paddingSize = "1x2"; // 3xEven
        else if (evenWidth && Height == 24) paddingSize = "2x1"; // Evenx3
        else if ((Width == 24 && !evenHeight) || (!evenWidth && Height == 24)) paddingSize = "1x1"; // 3xOdd and Oddx3

        else if (evenWidth && evenHeight) paddingSize = "2x2";
        else if (!evenWidth && evenHeight) paddingSize = "3x2";
        else if (evenWidth && !evenHeight) paddingSize = "2x3";
        else paddingSize = "3x3";

        paddingTexture = GFX.Game["objects/audiohelper/cassettemovingblock/padding_" + data.Attr("spriteName") + "/" + paddingSize];

        // spinner sprite
        if (Width > 24 && Height > 24) bigSprite = true;
        else bigSprite = false;

        Add(spinner = GFX.SpriteBank.Create(bigSprite ? "audiohelper_cassette_spool_big" : "audiohelper_cassette_spool"));
        spinner.Position = (iconOffset = new Vector2(Width / 2f, Height / 2f));
        spinner.Stop();
        spinner.Play("spin", randomizeFrame: true);
        spinner.Rate = 0f;

        // sending data to the mover
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
    }

    public override void Render()
    {
        float colCount = Width / 8f - 1f;
        float rowCount = Height / 8f - 1f;

        bool isInCentreColumn, isInCentreRow;
        float range = bigSprite ? 1f : 0.5f;

        for (int col = 0; col <= colCount; col++)
        {
            if (Math.Abs(col - colCount/2) <= range) isInCentreColumn = true;
            else isInCentreColumn = false;

            for (int row = 0; row <= rowCount; row++)
            {
                if (isInCentreColumn && Math.Abs(row - rowCount/2) <= range) isInCentreRow = true;
                else isInCentreRow = false;

                int colTile = ((col < colCount) ? Math.Min(col, 1) : 2);
                int rowTile = ((row < rowCount) ? Math.Min(row, 1) : 2);                

                if (isInCentreColumn && isInCentreRow) cutoutNineSlice[colTile, rowTile].Draw(Position + base.Shake + new Vector2(col * 8, row * 8));
                else solidNineSlice[colTile, rowTile].Draw(Position + base.Shake + new Vector2(col * 8, row * 8));

            }
        }
        paddingTexture.DrawCentered(Center+Shake);
        spinner.Render();
        base.Render();
    }

    public void OnMove(Vector2 destination)
    {
        MoveTo(destination);
    }
    public void StartMove()
    {
        //sfx.Play("event:/game/05_mirror_temple/swapblock_move");
        spinner.Rate = mover.easer == CassetteMover.Easers.CubeIn ? 1.5f : 1f;

        path.Start();
    }

    public void EndMove()
    {
        bool intense = false;
        if (mover.easer == CassetteMover.Easers.CubeIn)
        {
            StartShaking(0.15f);
            intense = true;
        }
        Audio.Play("event:/game/05_mirror_temple/swapblock_move_end", Center);

        Vector2 travelDirection = mover.vertices[mover.activeVertex] - mover.vertices[(mover.activeVertex - 1 + mover.vertices.Length) % mover.vertices.Length];
        ImpactParticles(travelDirection, intense);
        StopParticles(travelDirection, intense);
        spinner.Rate = 0f;

        path.Stop();
    }
    public void SilentUpdate()
    {
        Scene.Add(path = new CassetteMovingBlockPath(this, mover.ticksPerWait * mover.tickTimer));
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