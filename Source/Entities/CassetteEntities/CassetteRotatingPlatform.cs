using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;

namespace Celeste.Mod.audiohelper.Entities;

[CustomEntity("audiohelper/CassetteRotatingPlatform")]
[Tracked]
public class CassetteRotatingPlatform : JumpThru
{
    public CassetteListener listener;
    public CassetteRotator rotator;
    public CassetteRotatingPlatformCircle circle;

    public Vector2 pivot, renderPosition;
    public float radius;

    public float yOffset, sinkTimer;
    public Vector2 positionOffset;

    // audiovisuals
    public string texture;
    public MTexture[]  textures;
    public SoundSource sfx;
    public bool soundChoice;
    public float soundTimer;

    // constructor
    public CassetteRotatingPlatform(EntityData data, Vector2 offset) : base(data.Position + offset, data.Width, safe: false)
    {
        Tag = Tags.TransitionUpdate;
        AddTag(TagsExt.FreezeFrameUpdate);

        // data
        radius = data.Float("Radius");
        Add(rotator = new CassetteRotator(OnMove, SilentUpdate, OnSwap, data.Float("AngleOffset"), radius, data.Int("TicksPerCycle"), data.Bool("Clockwise")));
        Add(listener = new CassetteListener(0));
        listener.Tempo = data.Float("Tempo");

        texture = data.Attr("texture", "default");
        SurfaceSoundIndex = data.Int("SoundIndex", 5);
        Add(sfx = new SoundSource());
        sfx.Position.X += Width / 2;
        soundChoice = Calc.Random.Choose(true, false);
        soundTimer = Calc.Random.Range(0, 3);
        Add(new LightOcclude(0.5f));

        pivot = data.Position + offset;
        renderPosition = pivot + 4 * Vector2.UnitY - new Vector2(radius + 1, radius + 1);
        positionOffset.X = -Width / 2;

        MTexture mTexture = GFX.Game["objects/woodPlatform/" + texture];
        textures = new MTexture[mTexture.Width / 8];
        for (int i = 0; i < textures.Length; i++) textures[i] = mTexture.GetSubtexture(i * 8, 0, 8, 8);
    }

    public override void Added(Scene scene)
    {
        base.Added(scene);

        scene.Add(new CassetteRotatingPlatformCircle(pivot, texture == "cliffside", radius));
    }

    public override void Update()
    {
        base.Update();

        soundTimer -= Engine.DeltaTime;

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
        positionOffset.Y = yOffset;
    }

    public override void Render()
    {
        textures[0].Draw(Position + Shake);
        for (int i = 8; (float)i < Width - 8f; i += 8) textures[1].Draw(Position + Shake + new Vector2(i, 0f));
        textures[3].Draw(Position + Shake + new Vector2(Width - 8f, 0f));
        textures[2].Draw(Position + Shake + new Vector2(Width / 2f - 4f, 0f));        
    }

    public void OnMove(Vector2 destination)
    {
        MoveTo(pivot + destination + positionOffset);
    }
    public void SilentUpdate() { }
    public void OnSwap()
    {
        if (soundTimer <= 0)
        {
            soundChoice = !soundChoice;
            sfx.Play(soundChoice ? "event:/vert_audiohelper/movingplatform/move_1" : "event:/vert_audiohelper/movingplatform/move_2");
            soundTimer = 3f;
        }
    }

    // fixes liftboost when horizontal only
    public override void MoveHExact(int move)
    {
        if (Collidable)
        {
            if (move < 0)
            {
                foreach (Actor entity in base.Scene.Tracker.GetEntities<Actor>())
                {
                    if (entity.IsRiding(this))
                    {
                        Collidable = false;
                        if (entity.TreatNaive) entity.NaiveMove(Vector2.UnitX * move);
                        else entity.MoveHExact(move);
                        entity.LiftSpeed = LiftSpeed;
                        Collidable = true;
                    }
                }
            }
            else
            {
                foreach (Actor entity2 in base.Scene.Tracker.GetEntities<Actor>())
                {
                    if (entity2.IsRiding(this))
                    {
                        Collidable = false;
                        if (entity2.TreatNaive) entity2.NaiveMove(Vector2.UnitX * move);
                        else entity2.MoveHExact(move);
                        entity2.LiftSpeed = LiftSpeed;
                        Collidable = true;
                    }
                }
            }
        }
        base.X += move;
        MoveStaticMovers(Vector2.UnitX * move);
    }
}