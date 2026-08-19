local CassetteTrackSpinner = {}
local easers = {
    ["Sine In Out"] = 0,
    ["Cube In"] = 1
}

local speeds = {
    ["Slow Continuous"] = 0,
    ["Slow Stop"] = 1,
    ["Fast Continuous"] = 2,
    ["Fast Stop"] = 3,
    ["Custom"] = 4
}

local styles = {
    ["Blade"] = 0,
    ["Dust"] = 1,
    ["Starfish"] = 2
}



CassetteTrackSpinner.name = "audiohelper/CassetteTrackSpinner"
CassetteTrackSpinner.depth = -50
CassetteTrackSpinner.nodeLimits = {1, -1}
CassetteTrackSpinner.nodeLineRenderType = "line"
CassetteTrackSpinner.fieldInformation = {
    Easer = {
        options = easers,
        editable = false,
    },
    Speed = {
        options = speeds,
        editable = false,
    },
    Style = {
        options = styles,
        editable = false,
    },
}
CassetteTrackSpinner.placements = {
    name = "cassettetrackspinner",
    data = {
        Easer = 0,
        Speed = 3,
        Style = 0,
        Tempo = 1.0,
        Offset = 0,
        CustomSpeed = ""
    },
}

local textureStyles = {
    [0] = "danger/blade00",
    [1] = "danger/dustcreature/base00",
    [2] = "danger/starfish00",
}

function CassetteTrackSpinner.texture(room, entity)
    return textureStyles[entity.Style]
end

function CassetteTrackSpinner.nodeColor()
    return {1.0, 1.0, 1.0, 0.5}
end

return CassetteTrackSpinner