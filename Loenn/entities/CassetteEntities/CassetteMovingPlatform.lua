local resortPlatformHelper = require("helpers.resort_platforms")
local drawableSprite = require("structs.drawable_sprite")
local drawableLine = require("structs.drawable_line")
local utils = require("utils")

local textures = {
    "default", "cliffside"
}

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
    texture = {
        options = textures,
        editable = true,
    }
}
CassetteMovingPlatform.placements = {
    name = "cassettemovingplatform",
    data = {
        Easer = 0,
        Speed = 3,
        texture = "default",
        Tempo = 1.0,
        Offset = 0,
        CustomSpeed = "",
        width = 16,
        SoundIndex = 5
    },
}

-- RENDERING -- 

function CassetteMovingPlatform.sprite(room, entity)
    local sprites = {}

    local x, y = entity.x or 0, entity.y or 0
    local nodes = entity.nodes

    local nodeX, nodeY = {}, {}
    for i, node in ipairs(nodes) do
        nodeX[i] = node.x
        nodeY[i] = node.y
    end

    resortPlatformHelper.addConnectorSprites(sprites, entity, x, y, nodeX[1], nodeY[1], entity.width)
    if #nodes ~= 1 then
        resortPlatformHelper.addConnectorSprites(sprites, entity, nodeX[#nodes], nodeY[#nodes], x, y, entity.width)
    end

    for i, _ in ipairs(nodes) do
        if i < #nodes then
            resortPlatformHelper.addConnectorSprites(sprites, entity, nodeX[i], nodeY[i], nodeX[i+1], nodeY[i+1], entity.width)
        end
    end

    resortPlatformHelper.addPlatformSprites(sprites, entity, entity)

    return sprites
end

function CassetteMovingPlatform.nodeSprite(room, entity, node)
    local sprites = {}
    resortPlatformHelper.addPlatformSprites(sprites, entity, node)
    return sprites
end

CassetteMovingPlatform.selection = resortPlatformHelper.getSelection

return CassetteMovingPlatform