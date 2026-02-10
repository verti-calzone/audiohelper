local MusicalBooster = {}

MusicalBooster.name = "audiohelper/MusicalBooster"
MusicalBooster.depth = -8500
MusicalBooster.placements = {
    name = "musicalbooster",
    data = {
        red = false,
        EnterSound = "",
        StartSound = "",
        LoopSound = "",
        ExitSound = "",
        SpawnSound = "",
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