using System;
using System.Reflection;
using Celeste.Mod.audiohelper.Entities;
using Celeste.Mod.Helpers;
using IL.Monocle;
using Monocle;
using Microsoft.Xna.Framework;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using Celeste.Mod.Entities;

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


    private static void CCBMFreeze(On.Celeste.Celeste.orig_Freeze orig, float time)
    {
        if(Monocle.Engine.Scene != null) Monocle.Engine.Scene.Tracker.GetEntity<CustomCassetteBlockManager>()?.CCBMAdvanceMusic(time);
        orig(time);
    }

    public static bool SuppressVanillaCassetteBlockManagerDelegate(bool orig, Level level)
    {
        //Logger.Info("audiohelper","running delegate: SuppressVanillaCassetteBlockManagerDelegate");
        if(CustomCassetteBlockManager.IsActive(level)) 
        {
            //Logger.Info("audiohelper","suppressing cbm");
            return false;
        }
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
        //Logger.Info("audiohelper","running delegate: SuppressVanillaCassetteBlockManagerOnLevelStartDelegate");
        if(CustomCassetteBlockManager.IsActive(level)) 
        {
            //Logger.Info("audiohelper","suppressing cbm on levelstart");
            return false;
        }
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
    public static ILHook CassetteLoopControllerHook;

    private static string BlueberryPulseDelegate(string oldstring,Strawberry berry)
        => berry.isGhostBerry && Settings.BlueberryPulseSound ? "event:/game/general/strawberry_blue_pulse" : oldstring;
    public static void IL_BlueberryPulseHook(ILContext il)
    {
        ILCursor cursor = new ILCursor(il);
        for (int i = 0; i <= 1; i++)
        {
            while (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdstr("event:/game/general/strawberry_pulse")))
            {
                cursor.EmitLdarg0();
                cursor.EmitDelegate(BlueberryPulseDelegate);
            }
        }
    }


    // // // LOADING AND UNLOADING // // //


    public override void Load() {
        // Musical entities
        On.Celeste.Booster.PlayerBoosted += MusicalBooster.MusicalBoosterStart;
        On.Celeste.Booster.PlayerReleased += MusicalBooster.MusicalBoosterRelease;
        On.Celeste.Booster.Respawn += MusicalBooster.MusicalBoosterRespawn;
        On.Celeste.CrushBlock.Attack += MusicalKevin.MusicalKevinAttack;

        // CCBM
        On.Celeste.Celeste.Freeze += CCBMFreeze;
        SuppressVanillaCassetteBlockManager = new ILHook(typeof(Level).GetMethod("orig_LoadLevel", BindingFlags.Public | BindingFlags.Instance),IL_SuppressVanillaCassetteBlockManager);
        SuppressVanillaCassetteBlockManagerOnLevelStart = new ILHook(typeof(Level).GetMethod("orig_LoadLevel", BindingFlags.Public | BindingFlags.Instance),IL_SuppressVanillaCassetteBlockManagerOnLevelStart);

        // Cassette mover
        On.Celeste.CassetteBlockManager.SilentUpdateBlocks += CassetteTickReader.OnSilentUpdateBlocks;
        IL.Celeste.CassetteBlockManager.AdvanceMusic += CassetteTickReader.IL_AdvanceMusic;
        On.Celeste.CassetteBlockManager.StopBlocks += CassetteMovingBlockPath.OnStopBlocks;

        // // Single hooks // //

        // Audio replacer
        On.Celeste.Audio.GetEventDescription += AdvancedAudioReplacer.OnGetEventDescription;

        // Pause on pause controller
        On.Celeste.Level.StartPauseEffects += PauseMusicOnPause.OnStartPauseEffects;

        // Cassette loop controller
        IL.Celeste.CassetteBlockManager.AdvanceMusic += CassetteLoopController.IL_CassetteLoopController;

        // Blueberry pulse
        IL.Celeste.Strawberry.OnAnimate += IL_BlueberryPulseHook;

        // srt
        SpeedrunToolIop.srtloaduseapi();
    }

    public override void Unload() {
        // Musical entities
        On.Celeste.Booster.PlayerBoosted -= MusicalBooster.MusicalBoosterStart;
        On.Celeste.Booster.PlayerReleased -= MusicalBooster.MusicalBoosterRelease;
        On.Celeste.Booster.Respawn -= MusicalBooster.MusicalBoosterRespawn;
        On.Celeste.CrushBlock.Attack -= MusicalKevin.MusicalKevinAttack;

        // CCBM
        On.Celeste.Celeste.Freeze -= CCBMFreeze;
        SuppressVanillaCassetteBlockManager.Dispose();
        SuppressVanillaCassetteBlockManagerOnLevelStart.Dispose();

        // Cassette mover
        On.Celeste.CassetteBlockManager.SilentUpdateBlocks -= CassetteTickReader.OnSilentUpdateBlocks;
        IL.Celeste.CassetteBlockManager.AdvanceMusic -= CassetteTickReader.IL_AdvanceMusic;
        On.Celeste.CassetteBlockManager.StopBlocks -= CassetteMovingBlockPath.OnStopBlocks;

        // // Single hooks // //

        // Audio replacer
        On.Celeste.Audio.GetEventDescription -= AdvancedAudioReplacer.OnGetEventDescription;

        // Pause on pause controller
        On.Celeste.Level.StartPauseEffects -= PauseMusicOnPause.OnStartPauseEffects;

        // Cassette loop controller
        IL.Celeste.CassetteBlockManager.AdvanceMusic -= CassetteLoopController.IL_CassetteLoopController;

        // Blueberry pulse
        IL.Celeste.Strawberry.OnAnimate -= IL_BlueberryPulseHook;

        // srt
        SpeedrunToolIop.Unload();
    }
}