local CassetteTrackSpinner = {}
local easers = {
    ["Sine In Out"] = 0,
    ["Cube In"] = 1,
}

local speeds = {
    ["Slow Continuous"] = 0,
    ["Slow Stop"] = 1,
    ["Fast Continuous"] = 2,
    ["Fast Stop"] = 3,
    ["Custom"] = 4,
}

CassetteTrackSpinner.name = "audiohelper/CassetteTrackSpinner"
CassetteTrackSpinner.depth = -50
CassetteTrackSpinner.nodeLimits = {1, -1}
CassetteTrackSpinner.nodeLineRenderType = "line"
CassetteTrackSpinner.fieldInformation = {
    Easer = {
        fieldType = "integer",
        options = easers,
        editable = false,
    },
    Speed = {
        fieldType = "integer",
        options = speeds,
        editable = false,
    },
}
CassetteTrackSpinner.placements = {
    name = "cassettetrackspinner",
    data = {
        Easer = "Sine In Out",
        Speed = "Fast Stop",
        Tempo = 1.0,
        Offset = 0
    }
}
function CassetteTrackSpinner.texture(room, entity)
    return "danger/blade00"
end

return CassetteTrackSpinner