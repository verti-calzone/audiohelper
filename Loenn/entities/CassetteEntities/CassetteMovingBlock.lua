local CassetteMovingBlock = {}
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

CassetteMovingBlock.name = "audiohelper/CassetteMovingBlock"
CassetteMovingBlock.depth = -50
CassetteMovingBlock.nodeLimits = {1, -1}
CassetteMovingBlock.nodeLineRenderType = "line"
CassetteMovingBlock.fieldInformation = {
    Easer = {
        options = easers,
        editable = false,
    },
    Speed = {
        options = speeds,
        editable = false,
    },
}
CassetteMovingBlock.placements = {
    name = "cassettemovingblock",
    data = {
        Easer = 0,
        Speed = 3,
        Tempo = 1.0,
        Offset = 0,
        CustomSpeed = "",
        width = 16,
        height = 16,
        spriteName = "default"
    },
}

-- function CassetteMovingPlatform.texture(room, entity)
--     return textureStyles[entity.Style]
-- end

function CassetteMovingBlock.nodeColor()
    return {1.0, 1.0, 1.0, 0.5}
end

return CassetteMovingBlock