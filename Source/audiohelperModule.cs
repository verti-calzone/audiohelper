using System;
using Celeste.Mod.audiohelper.Entities;
using Microsoft.Xna.Framework;

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
    public override void Load() {
        On.Celeste.Booster.PlayerBoosted += MusicalBoosterStart;
        On.Celeste.Booster.PlayerReleased += MusicalBoosterRelease;
        On.Celeste.Booster.Respawn += MusicalBoosterRespawn;
    }

    public override void Unload() {
        On.Celeste.Booster.PlayerBoosted -= MusicalBoosterStart;
        On.Celeste.Booster.PlayerReleased -= MusicalBoosterRelease;
        On.Celeste.Booster.Respawn -= MusicalBoosterRespawn;
    }
}