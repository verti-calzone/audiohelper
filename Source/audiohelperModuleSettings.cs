namespace Celeste.Mod.audiohelper;

public class audiohelperModuleSettings : EverestModuleSettings {
    [SettingInGame(true)]
    public bool BlueberryPulseSound { get; set; } = true;
}