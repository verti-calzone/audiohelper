using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.audiohelper.Entities;

[CustomEntity("audiohelper/CassetteTrackSpinner")]
[Tracked]
public class CassetteTrackSpinner : Entity {

    public CassetteListener listener;
    public CassetteMover mover;

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
    public CassetteTrackSpinner(EntityData data, Vector2 offset)
    {
        // data
        Add(mover = new CassetteMover(OnMove, StartMove, EndMove, SilentUpdate));
        Add(listener = new CassetteListener(0));
        mover.easer = data.Enum<CassetteMover.Easers>("Easer", CassetteMover.Easers.SineInOut);
        mover.speed = data.Enum<CassetteMover.Speeds>("Speed", CassetteMover.Speeds.FastStop);
        mover.customSpeed = data.Attr("CustomSpeed");
        listener.Tempo = data.Float("Tempo");
        mover.tickOffset = data.Int("Offset");
        Style = data.Enum<Styles>("Style", Styles.Blade);

        // sending data to the cassette mover
        mover.vertexList.Add(data.Position + offset);
        foreach (Vector2 node in data.Nodes) mover.vertexList.Add(node + offset);
        mover.vertices = mover.vertexList.ToArray();

        base.Collider = new ColliderList(new Circle(6f), new Hitbox(16f, 4f, -8f, -3f));
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
            Vector2 eyeDir = (mover.vertices[(mover.activeVertex + 1) % mover.vertices.Length] - mover.vertices[mover.activeVertex]).SafeNormalize();
            dust.EyeDirection = eyeDir;
            dust.EyeTargetDirection = eyeDir;
        }
    }

    public override void Update()
    {
        base.Update();
        if (mover.moving && Scene.OnInterval(0.04f))
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

    public void StartMove()
    {
        if (Style == Styles.Starfish)
        {
            colourID++;
            colourID %= 3;
            sprite.Play("spin" + colourID);
        }
        else if (Style == Styles.Dust) return; // skips the audio call
        else sprite.Play("spin"); // fallback to blade
        Audio.Play("event:/game/05_mirror_temple/bladespinner_spin", Position);
    }

    public void EndMove()
    {
        if (Style == Styles.Dust) dust.EyeTargetDirection = (mover.vertices[(mover.activeVertex + 1) % mover.vertices.Length] - mover.vertices[mover.activeVertex]).SafeNormalize(); 
    }

    public virtual void OnPlayer(Player player)
    {
        if (player.Die((player.Position - Position).SafeNormalize()) != null)
        {
            mover.moving = false;
            mover.frozen = true;
        }
        if (Style == Styles.Dust)
        {
            dust.OnHitPlayer();
        }
    }
}