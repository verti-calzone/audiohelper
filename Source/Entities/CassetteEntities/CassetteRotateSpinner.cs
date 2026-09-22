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
        Add(rotator = new CassetteRotator(OnMove, SilentUpdate, data.Position + offset, data.Float("AngleOffset"), data.Float("Radius"), data.Int("TicksPerCycle")));
        Add(listener = new CassetteListener(0));

        listener.Tempo = data.Float("Tempo");
        Style = data.Enum<Styles>("Style", Styles.Blade);

        Collider = new ColliderList(new Circle(6f));
        Add(new PlayerCollider(OnPlayer));

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

    public override void Update()
    {
        base.Update();
        if (rotator.moving && Scene.OnInterval(0.04f))
        {
            if (Style == Styles.Starfish) SceneAs<Level>().ParticlesBG.Emit(starfishParticle[colourID], 1, Position, Vector2.One * 3f);
            else if (Style == Styles.Dust) SceneAs<Level>().ParticlesBG.Emit(dustParticle, 1, Position, Vector2.One * 4f);
            else SceneAs<Level>().ParticlesBG.Emit(bladeParticle, 2, Position, Vector2.One * 3f); // fallback to blade
        }
    }
    public void OnMove(Vector2 destination)
    {
        Position = destination;
    }

    public virtual void OnPlayer(Player player)
    {
        if (player.Die((player.Position - Position).SafeNormalize()) != null) rotator.frozen = true;
        if (Style == Styles.Dust) dust.OnHitPlayer();
    }
}