using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System.Runtime.CompilerServices;
using System;
using System.Runtime.Intrinsics;
using FMOD.Studio;
using System.Collections;

namespace Celeste.Mod.audiohelper.Entities;

[CustomEntity("audiohelper/MusicalBumper")]
[Tracked]

public class MusicalBumper : Bumper {
    public string BumpSound;
    public string FireSound;
    public string SpawnSound;
    public string MusicParam;
    public float ParamValue, ResetValue;
    public bool IncMode = false;
    private int Mode = 0;
    public int CoreState;
    public bool BigShake = false;
    public Musicalizer Musicalizer;
    public MusicalBumper(EntityData data, Vector2 offset) : base(data.Position + offset, data.FirstNodeNullable(offset))
    {
        BumpSound = data.Attr("BumpSound");
        FireSound = data.Attr("FireSound");
        SpawnSound = data.Attr("SpawnSound");
        MusicParam = data.Attr("MusicParameter");
        ParamValue = data.Float("ParameterValue");
        ResetValue = data.Float("ParameterResetValue");
        Mode = data.Int("Mode");
        IncMode = data.Bool("IncrementMode");
        CoreState = data.Int("CoreState");
        BigShake = data.Bool("BigShake");

        Get<CoreModeListener>().OnChange = MusicalOnChangeMode;
        Get<PlayerCollider>().OnCollide = MusicalOnPlayer;
        if(!string.IsNullOrEmpty(MusicParam)) Musicalizer = new Musicalizer();
    }
    [MethodImpl(MethodImplOptions.NoInlining)]
    public override void Added(Scene scene)
    {
        base.Added(scene);
        if(CoreState == 2) fireMode = SceneAs<Level>().CoreMode == Session.CoreModes.Hot;
        else if (CoreState == 1) fireMode = false;
        else if (CoreState == 0) fireMode = true;
        spriteEvil.Visible = fireMode;
        sprite.Visible = !fireMode;
    }
    [MethodImpl(MethodImplOptions.NoInlining)]
    private void MusicalOnChangeMode(Session.CoreModes coreMode)
    {
        if(CoreState == 2)
        {
            fireMode = coreMode == Session.CoreModes.Hot;
            spriteEvil.Visible = fireMode;
            sprite.Visible = !fireMode;
        }
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public override void Update()
    {
        base.Update();
        if (respawnTimer > 0f)
        {
            respawnTimer -= Engine.DeltaTime;
            if (respawnTimer <= 0f)
            {
                light.Visible = true;
                bloom.Visible = true;
                sprite.Play("on");
                spriteEvil.Play("on");
                if (!fireMode && !string.IsNullOrEmpty(SpawnSound)) Audio.Play(SpawnSound, Position);
                if(!string.IsNullOrEmpty(MusicParam)) Musicalizer.ResetParameter(MusicParam,ParamValue,IncMode,Mode,ResetValue);
            }
        }
        else if (base.Scene.OnInterval(0.05f))
        {
            float num = Calc.Random.NextAngle();
            ParticleType type = fireMode ? P_FireAmbience : P_Ambience;
            float direction = fireMode ? (-(float)Math.PI / 2f) : num;
            float length = fireMode ? 12 : 8;
            SceneAs<Level>().Particles.Emit(type, 1, base.Center + Calc.AngleToVector(num, length), Vector2.One * 2f, direction);
        }
        UpdatePosition();
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private void MusicalOnPlayer(Player player)
    {
        if (fireMode)
        {
            if (!SaveData.Instance.Assists.Invincible)
            {
                Vector2 vector = (player.Center - base.Center).SafeNormalize();
                hitDir = -vector;
                hitWiggler.Start();
                if(!string.IsNullOrEmpty(FireSound)) Audio.Play(FireSound, Position);
                respawnTimer = 0.6f;
                player.Die(vector);
                SceneAs<Level>().Particles.Emit(P_FireHit, 12, base.Center + vector * 12f, Vector2.One * 3f, vector.Angle());
            }
        }
        
        else if (respawnTimer <= 0f)
        {
            if(!string.IsNullOrEmpty(BumpSound)) Audio.Play(BumpSound, Position);
            if(!string.IsNullOrEmpty(MusicParam)) Musicalizer.SetParameter(MusicParam,ParamValue,IncMode,Mode);
            respawnTimer = 0.6f;
            Vector2 vector2 = player.ExplodeLaunch(Position, snapUp: false, sidesOnly: false);
            sprite.Play("hit", restart: true);
            spriteEvil.Play("hit", restart: true);
            light.Visible = false;
            bloom.Visible = false;
            SceneAs<Level>().DirectionalShake(vector2, BigShake ?  0.3f : 0.15f);
            SceneAs<Level>().Displacement.AddBurst(base.Center, 0.3f, 8f, 32f, 0.8f);
            SceneAs<Level>().Particles.Emit(P_Launch, 12, base.Center + vector2 * 12f, Vector2.One * 3f, vector2.Angle());
        }
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public override void Removed(Scene scene)
    {
        base.Removed(scene);
        if(!string.IsNullOrEmpty(MusicParam)) Musicalizer.ResetParameter(MusicParam,ParamValue,IncMode,Mode,ResetValue);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public override void SceneEnd(Scene scene)
    {
        base.SceneEnd(scene);
        if(!string.IsNullOrEmpty(MusicParam)) Musicalizer.ResetParameter(MusicParam,ParamValue,IncMode,Mode,ResetValue);
    }
}