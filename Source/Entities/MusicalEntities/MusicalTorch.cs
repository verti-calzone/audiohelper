using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System.Runtime.CompilerServices;

namespace Celeste.Mod.audiohelper.Entities;

[CustomEntity("audiohelper/MusicalTorch")]
[Tracked]

public class MusicalTorch : Entity {
    private EntityID id;
    private VertexLight light;
    private BloomPoint bloom;
    public Sprite sprite;
    public float alpha = 1.0f;
    public int StartRadius = 48;
    public int EndRadius = 64;
    public Color colour = Calc.HexToColor("ffffff");
    private bool lit;
    public bool stayLit;
    public string activateSound;
    public string musicParameter;
    public float parameterValue;
    private string FlagName
    {
        [MethodImpl(MethodImplOptions.NoInlining)]
        get
        {
            return "musicalTorch_" + id.Key;
        }
    }
    public MusicalTorch(EntityData data, Vector2 offset, EntityID id) : base(data.Position + offset)
    {
        alpha = data.Float("Alpha");
        StartRadius = data.Int("StartRadius");
        EndRadius = data.Int("EndRadius");
        colour = data.HexColor("Colour");
        stayLit = data.Bool("StayLit");
        activateSound = data.Attr("ActivationSound");
        musicParameter = data.Attr("MusicParameter");
        parameterValue = data.Float("ParameterValue");
        Add(sprite = GFX.SpriteBank.Create("torch"));

        Collider = new Hitbox(32f, 32f, -16f, -16f);
        Add(new PlayerCollider(OnPlayer));
        Add(light = new VertexLight(colour, alpha, StartRadius, EndRadius));
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
    private void OnPlayer(Player player)
    {
        if (!lit)
        {
            Audio.Play(activateSound, Position);
            Audio.SetMusicParam(musicParameter, parameterValue);
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
            if(stayLit)
            {
                SceneAs<Level>().Session.SetFlag(FlagName);
            }
            SceneAs<Level>().ParticlesFG.Emit(Torch.P_OnLight, 12, Position, new Vector2(3f, 3f));
        }
    }
}