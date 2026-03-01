local MusicalBumper = {}
local coreStates = {
    ["Hot Only"] = 0,
    ["Cold Only"] = 1,
    ["React to Core Mode"] = 2,
}

MusicalBumper.name = "audiohelper/MusicalBumper"
MusicalBumper.depth = -8500
MusicalBumper.fieldInformation = {
    CoreState = {
        fieldType = "integer",
        options = coreStates,
        editable = false,
    },
}
MusicalBumper.placements = {
    name = "musicalbumper",
    data = {
        BumpSound = "event:/game/06_reflection/pinballbumper_hit",
        FireSound = "event:/game/09_core/hotpinball_activate",
        SpawnSound = "event:/game/06_reflection/pinballbumper_reset",
        MusicParameter = "",
        ParameterValue = 0,
        IncrementMode = false;
        CoreState = 2,
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