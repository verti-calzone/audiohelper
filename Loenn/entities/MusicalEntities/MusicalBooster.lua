local MusicalBooster = {}

MusicalBooster.name = "audiohelper/MusicalBooster"
MusicalBooster.depth = -8500
MusicalBooster.placements = {
    name = "musicalbooster",
    data = {
        red = false,
        EnterSound = "event:/game/04_cliffside/greenbooster_enter",
        StartSound = "event:/game/04_cliffside/greenbooster_dash",
        LoopSound = "event:/game/05_mirror_temple/redbooster_move",
        ExitSound = "event:/game/04_cliffside/greenbooster_end",
        SpawnSound = "event:/game/04_cliffside/greenbooster_reappear",
        MusicParameter = "",
        ParameterValue = 0,
        IncrementMode = false;
    }
}
function MusicalBooster.texture(room, entity)
    local red = entity.red

    if red then
        return "objects/booster/boosterRed00"

    else
        return "objects/booster/booster00"
    end
end

return MusicalBooster