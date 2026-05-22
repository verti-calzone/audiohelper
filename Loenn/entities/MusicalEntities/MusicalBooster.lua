local MusicalBooster = {}

local modeNames = {
    {"Get Reset Value", 1},
    {"Set Reset Value", 2},
    {"Increment Mode", 3}
}

MusicalBooster.name = "audiohelper/MusicalBooster"
MusicalBooster.depth = -8500
MusicalBooster.fieldInformation = {
    Mode = {
        minimum = 0,
        maximum = 1,
        options = modeNames,
        editable = false
    }
}
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
        IncrementMode = false,
        ParameterResetValue = 0,
        Mode = 2
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