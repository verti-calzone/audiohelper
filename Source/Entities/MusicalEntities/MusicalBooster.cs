using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System.Runtime.CompilerServices;
using System;
using System.Runtime.Intrinsics;
using FMOD.Studio;
using System.Collections;

namespace Celeste.Mod.audiohelper.Entities;

[CustomEntity("audiohelper/MusicalBooster")]
[Tracked]

public class MusicalBooster : Booster {
    public string EnterSound;
    public string StartSound;
    public string LoopSound;
    public string ExitSound;
    public string SpawnSound;
    public string MusicParam;
    public float ParamValue;

    public bool IncMode = false;
    public Musicalizer Musicalizer = new();
    public MusicalBooster(EntityData data, Vector2 offset) : base(data.Position + offset, data.Bool("red"))
    {
        EnterSound = data.Attr("EnterSound");
        StartSound = data.Attr("StartSound");
        LoopSound = data.Attr("LoopSound");
        ExitSound = data.Attr("ExitSound");
        SpawnSound = data.Attr("SpawnSound");
        MusicParam = data.Attr("MusicParameter");
        ParamValue = data.Float("ParameterValue");
        IncMode = data.Bool("IncrementMode");
        Get<PlayerCollider>().OnCollide = MusicalOnPlayer;
    }
    [MethodImpl(MethodImplOptions.NoInlining)]
    private void MusicalOnPlayer(Player player)
    {
        if (respawnTimer <= 0f && cannotUseTimer <= 0f && !BoostingPlayer)
        {
            cannotUseTimer = 0.45f;
            if (red) player.RedBoost(this);
            else player.Boost(this);
            if(!string.IsNullOrEmpty(EnterSound)) Audio.Play(EnterSound, Position);
            wiggler.Start();
            sprite.Play("inside");
            sprite.FlipX = player.Facing == Facings.Left;
        }
    }
    [MethodImpl(MethodImplOptions.NoInlining)]
    new public void PlayerBoosted(Player player, Vector2 direction)
    {
        if(!string.IsNullOrEmpty(StartSound)) Audio.Play(StartSound, Position);
        if (red)
        {
            if(!string.IsNullOrEmpty(LoopSound)) loopingSfx.Play(LoopSound);
            loopingSfx.DisposeOnTransition = false;
        }
        if(!string.IsNullOrEmpty(MusicParam)) Musicalizer.SetParameter(MusicParam,ParamValue,IncMode);
        BoostingPlayer = true;
        Tag = (int)Tags.Persistent | (int)Tags.TransitionUpdate;
        sprite.Play("spin");
        sprite.FlipX = player.Facing == Facings.Left;
        outline.Visible = true;
        wiggler.Start();
        dashRoutine.Replace(BoostRoutine(player, direction));
    }
    [MethodImpl(MethodImplOptions.NoInlining)]
    new public void PlayerReleased()
    {
        if(!string.IsNullOrEmpty(ExitSound)) Audio.Play(ExitSound, sprite.RenderPosition);
        if(!string.IsNullOrEmpty(MusicParam)) Musicalizer.ResetParameter(MusicParam,ParamValue,IncMode);
        sprite.Play("pop");
        cannotUseTimer = 0f;
        respawnTimer = 1f;
        BoostingPlayer = false;
        wiggler.Stop();
        loopingSfx.Stop();
    }
    [MethodImpl(MethodImplOptions.NoInlining)]
    new public void Respawn()
    {
        if(!string.IsNullOrEmpty(SpawnSound)) Audio.Play(SpawnSound, Position);
        sprite.Position = Vector2.Zero;
        sprite.Play("loop", restart: true);
        wiggler.Start();
        sprite.Visible = true;
        outline.Visible = false;
        AppearParticles();
    }
    public static void MusicalBoosterStart(On.Celeste.Booster.orig_PlayerBoosted orig, Booster self, Player player, Vector2 direction)
    {
        if (self is MusicalBooster MusicalBooster) MusicalBooster.PlayerBoosted(player, direction);
        else orig(self, player, direction);
    }
    public static void MusicalBoosterRelease(On.Celeste.Booster.orig_PlayerReleased orig, Booster self)
    {
        if (self is MusicalBooster MusicalBooster) MusicalBooster.PlayerReleased();
        else orig(self);
    }
    public static void MusicalBoosterRespawn(On.Celeste.Booster.orig_Respawn orig, Booster self)
    {
        if (self is MusicalBooster MusicalBooster) MusicalBooster.Respawn();
        else orig(self);
    }
}