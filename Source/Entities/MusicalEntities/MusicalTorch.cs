using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System.Runtime.CompilerServices;

namespace Celeste.Mod.audiohelper.Entities;

[CustomEntity("audiohelper/MusicalTorch")]
[Tracked]

public class MusicalTorch : Entity {
    // Data variables
    public float Alpha;
    public int StartRadius;
    public int EndRadius;
    public Color Colour = Calc.HexToColor("ffffff");
    public bool StayLit;
    public string ActivateSound;
    public string MusicParam;
    public float ParamValue;
    public bool IncMode;
    public Musicalizer Musicalizer;

    // Internal variables
    private EntityID id;
    private VertexLight light;
    private BloomPoint bloom;
    public Sprite sprite;
    public bool lit;
    public string FlagName
    {
        [MethodImpl(MethodImplOptions.NoInlining)]
        get
        {
            return "musicalTorch_" + id.Key;
        }
    }
    public MusicalTorch(EntityData data, Vector2 offset, EntityID id) : base(data.Position + offset)
    {
        Alpha = data.Float("Alpha");
        StartRadius = data.Int("StartRadius");
        EndRadius = data.Int("EndRadius");
        Colour = data.HexColor("Colour");
        StayLit = data.Bool("StayLit");
        ActivateSound = data.Attr("ActivationSound");
        MusicParam = data.Attr("MusicParameter");
        ParamValue = data.Float("ParameterValue");
        IncMode = data.Bool("IncrementMode");

        if(!string.IsNullOrEmpty(MusicParam)) Musicalizer = new Musicalizer();

        Add(sprite = GFX.SpriteBank.Create("torch"));
        Collider = new Hitbox(32f, 32f, -16f, -16f);
        Add(new PlayerCollider(OnPlayer));
        Add(light = new VertexLight(Colour, Alpha, StartRadius, EndRadius));
        Add(bloom = new BloomPoint(0.5f, 8f));
        bloom.Visible = false;
        light.Visible = false;
        Depth = 2000;
    }
    public override void Awake(Scene scene)
    {
        base.Awake(scene);
        if (SceneAs<Level>().Session.GetFlag(FlagName))
        {
            bloom.Visible = light.Visible = true;
            lit = true;
            Collidable = false;
            sprite.Play("on");
        }
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public void OnPlayer(Player player)
    {
        if (!lit)
        {
            if (!string.IsNullOrEmpty(ActivateSound)) Audio.Play(ActivateSound, Position);
            if(!string.IsNullOrEmpty(MusicParam)) Musicalizer.SetParameter(MusicParam,ParamValue,IncMode);
            lit = true;
            bloom.Visible = true;
            light.Visible = true;
            Collidable = false;
            sprite.Play("turnOn");
            Tween tween = Tween.Create(Tween.TweenMode.Oneshot, Ease.BackOut, 1f, start: true);
            tween.OnUpdate = [MethodImpl(MethodImplOptions.NoInlining)] (Tween t) =>
            {
                light.StartRadius = StartRadius + (1f - t.Eased) * StartRadius/2;
                light.EndRadius = EndRadius + (1f - t.Eased) * EndRadius/2;
                bloom.Alpha = 0.5f + 0.5f * (1f - t.Eased);
            };
            Add(tween);
            if(StayLit)
            {
                SceneAs<Level>().Session.SetFlag(FlagName);
            }
            SceneAs<Level>().ParticlesFG.Emit(Torch.P_OnLight, 12, Position, new Vector2(3f, 3f));
        }
    }
    public override void Removed(Scene scene)
    {
        base.Removed(scene);
        if(!StayLit && !string.IsNullOrEmpty(MusicParam)) Musicalizer.ResetParameter(MusicParam,ParamValue,IncMode);
    }
}