local MusicalBumper = {}
local coreStates = {
    ["Hot Only"] = 0,
    ["Cold Only"] = 1,
    ["React to Core Mode"] = 2,
}

local modeNames = {
    {"Get Reset Value", 1},
    {"Set Reset Value", 2},
    {"Increment Mode", 3}
}

MusicalBumper.name = "audiohelper/MusicalBumper"
MusicalBumper.depth = -8500
MusicalBumper.nodeLimits = {0, 1}
MusicalBumper.nodeLineRenderType = "line"
MusicalBumper.fieldInformation = {
    CoreState = {
        fieldType = "integer",
        options = coreStates,
        editable = false,
    },
    Mode = {
        minimum = 0,
        maximum = 1,
        options = modeNames,
        editable = false
    }
}
MusicalBumper.placements = {
    name = "musicalbumper",
    data = {
        BumpSound = "event:/game/06_reflection/pinballbumper_hit",
        FireSound = "event:/game/09_core/hotpinball_activate",
        SpawnSound = "event:/game/06_reflection/pinballbumper_reset",
        MusicParameter = "",
        ParameterValue = 0,
        CoreState = 2,
        BigShake = false,
        Mode = 2,
    }
}
function MusicalBumper.texture(room, entity)
    local corestate = entity.CoreState

    if corestate == 0 then
        return "objects/Bumper/Evil26"
    else
        return "objects/Bumper/Idle22"
    end
end

return MusicalBumper