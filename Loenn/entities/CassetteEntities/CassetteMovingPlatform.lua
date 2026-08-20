local CassetteMovingPlatform = {}
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

CassetteMovingPlatform.name = "audiohelper/CassetteMovingPlatform"
CassetteMovingPlatform.depth = -50
CassetteMovingPlatform.nodeLimits = {1, -1}
CassetteMovingPlatform.nodeLineRenderType = "line"
CassetteMovingPlatform.fieldInformation = {
    Easer = {
        options = easers,
        editable = false,
    },
    Speed = {
        options = speeds,
        editable = false,
    },
}
CassetteMovingPlatform.placements = {
    name = "cassettemovingplatform",
    data = {
        Easer = 0,
        Speed = 3,
        Texture = "",
        Tempo = 1.0,
        Offset = 0,
        CustomSpeed = "",
        width = 16
    },
}

-- function CassetteMovingPlatform.texture(room, entity)
--     return textureStyles[entity.Style]
-- end

function CassetteMovingPlatform.nodeColor()
    return {1.0, 1.0, 1.0, 0.5}
end

return CassetteMovingPlatform