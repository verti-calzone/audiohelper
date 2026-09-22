local CassetteRotateSpinner = {}
local styles = {
    ["Blade"] = 0,
    ["Dust"] = 1,
    ["Starfish"] = 2
}



CassetteRotateSpinner.name = "audiohelper/CassetteRotateSpinner"
CassetteRotateSpinner.depth = -50
CassetteRotateSpinner.nodeLimits = {1, -1}
CassetteRotateSpinner.nodeLineRenderType = "line"
CassetteRotateSpinner.fieldInformation = {
    Style = {
        options = styles,
        editable = false,
    },
}
CassetteRotateSpinner.placements = {
    name = "cassetterotatespinner",
    data = {
        Style = 0,
        Tempo = 1.0,
        Radius = 16,
        TicksPerCycle = 4,
        AngleOffset = 0,
    },
}

local textureStyles = {
    [0] = "danger/blade00",
    [1] = "danger/dustcreature/base00",
    [2] = "danger/starfish00",
}

function CassetteRotateSpinner.texture(room, entity)
    return textureStyles[entity.Style]
end

function CassetteRotateSpinner.nodeColor()
    return {1.0, 1.0, 1.0, 0.5}
end

return CassetteRotateSpinner