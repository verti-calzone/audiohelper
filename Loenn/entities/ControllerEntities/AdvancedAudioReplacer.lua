local AdvancedAudioReplacer = {}

AdvancedAudioReplacer.name = "audiohelper/AdvancedAudioReplacer"
AdvancedAudioReplacer.depth = -8500
AdvancedAudioReplacer.texture = "objects/audiohelper/AdvancedAudioReplacer"
AdvancedAudioReplacer.placements = {
    name = "advancedaudioreplacer",
    data = {
        OldEvent = "",
        NewEvent = "",
        Flag = "",
        ResetTime = 1,
        MusicParameter = "",
        ParameterValue = 0,
        IncMode = false,
    }
}
return AdvancedAudioReplacer