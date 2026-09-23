local drawableSprite = require("structs.drawable_sprite")
local drawableLine = require("structs.drawable_line")
local utils = require("utils")

local CassetteRotateSpinner = {}
local styles = {
    ["Blade"] = 0,
    ["Dust"] = 1,
    ["Starfish"] = 2
}
local degToRad = math.pi / 180


CassetteRotateSpinner.name = "audiohelper/CassetteRotateSpinner"
CassetteRotateSpinner.depth = -50
CassetteRotateSpinner.fieldInformation = {
    Style = {
        options = styles,
        editable = false,
    },
    Radius = {
        fieldType = "integer",
    }
}
CassetteRotateSpinner.placements = {
    name = "cassetterotatespinner",
    data = {
        Style = 0,
        Tempo = 1.0,
        Radius = 16,
        TicksPerCycle = 4,
        AngleOffset = 0,
        Clockwise = true,
        AttachToSolid = true,
    },
}

-- RENDERING --

local textureStyles = {
    [0] = "danger/blade00",
    [1] = "danger/dustcreature/base00",
    [2] = "danger/starfish00",
}
local cwStyles = {
    [0] = "objects/audiohelper/cassetterotatespinner/blade_cw",
    [1] = "objects/audiohelper/cassetterotatespinner/dust_cw",
    [2] = "objects/audiohelper/cassetterotatespinner/starfish_cw",
}
local ccwStyles = {
    [0] = "objects/audiohelper/cassetterotatespinner/blade_ccw",
    [1] = "objects/audiohelper/cassetterotatespinner/dust_ccw",
    [2] = "objects/audiohelper/cassetterotatespinner/starfish_ccw",
}

function CassetteRotateSpinner.sprite(room, entity)
    local sprites = {}
    if entity.Clockwise then
        table.insert(sprites, drawableSprite.fromTexture(cwStyles[entity.Style], entity))
    else
        table.insert(sprites, drawableSprite.fromTexture(ccwStyles[entity.Style], entity))
    end
    local spinnerSprite
    local addx = math.sin(entity.AngleOffset * degToRad) * entity.Radius
    local addy = -1 * (math.cos(entity.AngleOffset * degToRad) * entity.Radius)
    spinnerSprite = drawableSprite.fromTexture(textureStyles[entity.Style], entity)
    spinnerSprite:addPosition(addx, addy)
    spinnerSprite:setAlpha(0.5)
    table.insert(sprites, spinnerSprite)

    local mainLine = drawableLine.fromPoints({entity.x, entity.y, entity.x + addx, entity.y + addy}, "202020", 1)
    mainLine.depth = 5000
    table.insert(sprites, mainLine)

    local radius, x, y = entity.Radius or 16, entity.x or 0, entity.y or 0
    local segments = radius
    if segments < 8 then
        segments = 8
    elseif segments > 32 then
        segments = 32
    end
    for i = 1, segments, 1 do
        local x1 = math.sin((i-1)*2*math.pi/segments) * radius + x
        local y1 = math.cos((i-1)*2*math.pi/segments) * radius + y
        local x2 = math.sin(i*2*math.pi/segments) * radius + x
        local y2 = math.cos(i*2*math.pi/segments) * radius + y
        local lineSegment = drawableLine.fromPoints({x1, y1, x2, y2}, "404040", 1)
        lineSegment.depth = 5000
        table.insert(sprites, lineSegment)
    end

    return sprites
end

function CassetteRotateSpinner.selection(room, entity)
    local nodeRect = {}
    
    local x, y = entity.x or 0, entity.y or 0
    local radius = entity.radius
    local mainRectangle = utils.rectangle(x-8, y-8, 16, 16)

    local spinnerx = x + math.sin(entity.AngleOffset * degToRad) * entity.Radius
    local spinnery = y + -1 * (math.cos(entity.AngleOffset * degToRad) * entity.Radius)
    local spinnerRectangle = utils.rectangle(spinnerx-8, spinnery-8, 16, 16)
    table.insert(nodeRect, spinnerRectangle)

    return mainRectangle, nodeRect
end

function CassetteRotateSpinner.onMove(room, entity, nodeIndex, offsetX, offsetY)
    if nodeIndex == 0 then
        return
    end
    entity.x = entity.x + offsetX
    entity.y = entity.y + offsetY
end

function CassetteRotateSpinner.flip(room, entity, horizontal, vertical)
    if horizontal then
        entity.AngleOffset = -entity.AngleOffset % 360
    elseif vertical then
        entity.AngleOffset = (180 - entity.AngleOffset) % 360
    end
end

function CassetteRotateSpinner.rotate(room, entity, direction)
    entity.AngleOffset = (entity.AngleOffset + 90 * direction) % 360
end

return CassetteRotateSpinner