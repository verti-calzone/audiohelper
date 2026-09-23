using System.Runtime.CompilerServices;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.audiohelper.Entities;

[CustomEntity("audiohelper/CassetteRotateSpinner")]
[Tracked]
public class CassetteRotateSpinner : Entity {

    public CassetteListener listener;
    public CassetteRotator rotator;

    public Vector2 pivot;
    public bool fallen = false;

    // visuals
    public enum Styles { Blade, Dust, Starfish };
    public Styles Style;
    
    public Sprite sprite;
    public static ParticleType bladeParticle = BladeTrackSpinner.P_Trail;
    public static ParticleType dustParticle = DustStaticSpinner.P_Move;
    public static ParticleType[] starfishParticle = StarTrackSpinner.P_Trail;
    public ParticleType particle;
    public int colourID;
    public DustGraphic dust;
    public Vector2 targetFacingAngle;

    // constructor
    public CassetteRotateSpinner(EntityData data, Vector2 offset) : base(data.Position + offset)
    {
        Tag = Tags.TransitionUpdate;
        AddTag(TagsExt.FreezeFrameUpdate);

        // data
        Add(rotator = new CassetteRotator(OnMove, SilentUpdate, OnSwap, data.Float("AngleOffset"), data.Float("Radius"), data.Int("TicksPerCycle"), data.Bool("Clockwise")));
        Add(listener = new CassetteListener(0));
        if (data.Bool("AttachToSolid"))
        {
            StaticMover staticMover = new StaticMover();
            staticMover.SolidChecker = (Solid solid) => solid.CollidePoint(pivot);
            staticMover.JumpThruChecker = (JumpThru jumpthru) => jumpthru.CollidePoint(pivot);
            staticMover.OnMove = (Vector2 move) =>
            {
                pivot += move;
                Position += move;
            };
            staticMover.OnDestroy = delegate
            {
                fallen = true;
            };
            Add(staticMover);
        }

        listener.Tempo = data.Float("Tempo");
        Style = data.Enum<Styles>("Style", Styles.Blade);

        Collider = new ColliderList(new Circle(6f));
        Add(new PlayerCollider(OnPlayer));

        pivot = data.Position + offset;

        // Creating the sprite
        if (Style == Styles.Starfish)
        {
            Add(sprite = GFX.SpriteBank.Create("moonBlade"));
            colourID = Calc.Random.Choose(0, 1, 2);
            sprite.Play("idle" + colourID);
        }
        else if (Style == Styles.Dust)
        {
            Add(dust = new DustGraphic(ignoreSolids: true));
            dust.eyesMoveByRotation = true;
        }
        else // fallback to blade
        {
            Add(sprite = GFX.SpriteBank.Create("templeBlade"));
            sprite.Play("idle");
        }
        Add(new MirrorReflection());
        Depth = -50;
    }

    public void SilentUpdate()
    {
        if (Style == Styles.Dust)
        {
            Vector2 eyeDir = Vector2.One;
            dust.EyeDirection = eyeDir;
            dust.EyeTargetDirection = eyeDir;
        }
    }

    public void OnSwap()
    {
        if (!rotator.frozen)
        {
            if (Style == Styles.Starfish)
            {
                colourID++;
                colourID %= 3;
                sprite.Play("spin" + colourID);
            }
            else if (Style == Styles.Dust) return;
            else sprite.Play("spin"); // fallback to blade
        }
    }

    public override void Update()
    {
        base.Update();
        if (!rotator.frozen && Scene.OnInterval(0.04f))
        {
            if (Style == Styles.Starfish) SceneAs<Level>().ParticlesBG.Emit(starfishParticle[colourID], 1, Position, Vector2.One * 3f);
            else if (Style == Styles.Dust) SceneAs<Level>().ParticlesBG.Emit(dustParticle, 1, Position, Vector2.One * 4f);
            else SceneAs<Level>().ParticlesBG.Emit(bladeParticle, 2, Position, Vector2.One * 3f); // fallback to blade
        }
        if (fallen)
        {
            pivot.Y += 160f * Engine.DeltaTime; // continues the fall after the block is gone
            if (Y > ((Scene as Level).Bounds.Bottom + 32)) RemoveSelf();
        }
    }
    public void OnMove(Vector2 destination)
    {
        Position = pivot + destination;
    }

    public virtual void OnPlayer(Player player)
    {
        if (player.Die((player.Position - Position).SafeNormalize()) != null) rotator.frozen = true;
        if (Style == Styles.Dust) dust.OnHitPlayer();
    }
}