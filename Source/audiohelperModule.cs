using System;
using System.Reflection;
using Celeste.Mod.audiohelper.Entities;
using Celeste.Mod.Helpers;
using IL.Monocle;
using Monocle;
using Microsoft.Xna.Framework;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;

namespace Celeste.Mod.audiohelper;

public class audiohelperModule : EverestModule {
    public static audiohelperModule Instance { get; private set; }

    public override Type SettingsType => typeof(audiohelperModuleSettings);
    public static audiohelperModuleSettings Settings => (audiohelperModuleSettings) Instance._Settings;

    public override Type SessionType => typeof(audiohelperModuleSession);
    public static audiohelperModuleSession Session => (audiohelperModuleSession) Instance._Session;

    public override Type SaveDataType => typeof(audiohelperModuleSaveData);
    public static audiohelperModuleSaveData SaveData => (audiohelperModuleSaveData) Instance._SaveData;

    public audiohelperModule() {
        Instance = this;
#if DEBUG
        // debug builds use verbose logging
        Logger.SetLogLevel(nameof(audiohelperModule), LogLevel.Verbose);
#else
        // release builds use info logging to reduce spam in log files
        Logger.SetLogLevel(nameof(audiohelperModule), LogLevel.Info);
#endif
    }
    private static void MusicalBoosterStart(On.Celeste.Booster.orig_PlayerBoosted orig, Booster self, Player player, Vector2 direction)
    {
        if (self is MusicalBooster MusicalBooster) MusicalBooster.PlayerBoosted(player, direction);
        else orig(self, player, direction);
    }
    private static void MusicalBoosterRelease(On.Celeste.Booster.orig_PlayerReleased orig, Booster self)
    {
        if (self is MusicalBooster MusicalBooster) MusicalBooster.PlayerReleased();
        else orig(self);
    }
    private static void MusicalBoosterRespawn(On.Celeste.Booster.orig_Respawn orig, Booster self)
    {
        if (self is MusicalBooster MusicalBooster) MusicalBooster.Respawn();
        else orig(self);
    }
    private static void CCBMFreeze(On.Celeste.Celeste.orig_Freeze orig, float time)
    {
        if(Monocle.Engine.Scene != null) Monocle.Engine.Scene.Tracker.GetEntity<CustomCassetteBlockManager>()?.AdvanceMusic(time);
        orig(time);
    }
    public static bool SuppressVanillaCassetteBlockManagerDelegate(bool orig, Level level)
    {
        if(CustomCassetteBlockManager.IsActive(level)) return false;
        else return orig;
    }
    public static void IL_SuppressVanillaCassetteBlockManager(ILContext il)
    {
        ILCursor cursor = new ILCursor(il);

        if(cursor.TryGotoNextBestFit(MoveType.After,
            instr => instr.MatchBrtrue(out var _),
            instr => instr.MatchLdarg0(),
            instr => instr.MatchCallvirt<Level>("get_ShouldCreateCassetteManager")))
        {
            cursor.EmitLdarg0();
            cursor.EmitDelegate(SuppressVanillaCassetteBlockManagerDelegate);
        }
        else throw new Exception("Audiohelper: Could not make SuppressVanillaCassetteBlockManager hook!");
    }
    public static ILHook SuppressVanillaCassetteBlockManager;
    public static bool SuppressVanillaCassetteBlockManagerOnLevelStartDelegate(bool orig, Level level)
    {
        if(CustomCassetteBlockManager.IsActive(level)) return false;
        else return orig;
    }
    public static void IL_SuppressVanillaCassetteBlockManagerOnLevelStart(ILContext il)
    {
        ILCursor cursor = new ILCursor(il);

        if(cursor.TryGotoNextBestFit(MoveType.After,
            instr => instr.MatchBrfalse(out var _),
            instr => instr.MatchLdarg0(),
            instr => instr.MatchCallvirt<Level>("get_ShouldCreateCassetteManager")))
        {
            cursor.EmitLdarg0();
            cursor.EmitDelegate(SuppressVanillaCassetteBlockManagerOnLevelStartDelegate);
        }
        else throw new Exception("Audiohelper: Could not make SuppressVanillaCassetteBlockManagerOnLevelStart hook!");
    }
    public static ILHook SuppressVanillaCassetteBlockManagerOnLevelStart;

    public override void Load() {
        On.Celeste.Booster.PlayerBoosted += MusicalBoosterStart;
        On.Celeste.Booster.PlayerReleased += MusicalBoosterRelease;
        On.Celeste.Booster.Respawn += MusicalBoosterRespawn;

        On.Celeste.Celeste.Freeze += CCBMFreeze;
        SuppressVanillaCassetteBlockManager = new ILHook(typeof(Level).GetMethod("orig_LoadLevel", BindingFlags.Public | BindingFlags.Instance),IL_SuppressVanillaCassetteBlockManager);
        SuppressVanillaCassetteBlockManagerOnLevelStart = new ILHook(typeof(Level).GetMethod("orig_LoadLevel", BindingFlags.Public | BindingFlags.Instance),IL_SuppressVanillaCassetteBlockManagerOnLevelStart);

        On.Celeste.Audio.GetEventDescription += SimpleAudioReplacer.OnGetEventDescription;

        SpeedrunToolIop.srtloaduseapi();
    }

    public override void Unload() {
        On.Celeste.Booster.PlayerBoosted -= MusicalBoosterStart;
        On.Celeste.Booster.PlayerReleased -= MusicalBoosterRelease;
        On.Celeste.Booster.Respawn -= MusicalBoosterRespawn;

        On.Celeste.Celeste.Freeze -= CCBMFreeze;
        SuppressVanillaCassetteBlockManager.Dispose();
        SuppressVanillaCassetteBlockManagerOnLevelStart.Dispose();

        On.Celeste.Audio.GetEventDescription -= SimpleAudioReplacer.OnGetEventDescription;

        SpeedrunToolIop.Unload();
    }
}