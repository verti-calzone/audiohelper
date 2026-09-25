local drawableSprite = require("structs.drawable_sprite")
local drawableLine = require("structs.drawable_line")
local utils = require("utils")

local CassetteRotatingSpinner = {}
local styles = {
    ["Blade"] = 0,
    ["Dust"] = 1,
    ["Starfish"] = 2
}
local degToRad = math.pi / 180

CassetteRotatingSpinner.name = "audiohelper/CassetteRotatingSpinner"
CassetteRotatingSpinner.depth = -10100
CassetteRotatingSpinner.fieldInformation = {
    Style = {
        options = styles,
        editable = false,
    },
    Radius = {
        fieldType = "integer",
    },
    TicksPerCycle = {
        fieldType = "integer",
    }
}
CassetteRotatingSpinner.placements = {
    name = "cassetterotatingspinner",
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

local textureStyles = {
    [0] = "danger/blade00",
    [1] = "danger/dustcreature/base00",
    [2] = "danger/starfish00",
}
local cwStyles = {
    [0] = "objects/audiohelper/cassetterotatingspinner/blade_cw",
    [1] = "objects/audiohelper/cassetterotatingspinner/dust_cw",
    [2] = "objects/audiohelper/cassetterotatingspinner/starfish_cw",
}
local ccwStyles = {
    [0] = "objects/audiohelper/cassetterotatingspinner/blade_ccw",
    [1] = "objects/audiohelper/cassetterotatingspinner/dust_ccw",
    [2] = "objects/audiohelper/cassetterotatingspinner/starfish_ccw",
}

function CassetteRotatingSpinner.sprite(room, entity)
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
    spinnerSprite.depth = -50
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

function CassetteRotatingSpinner.selection(room, entity)
    local nodeRect = {}
    
    local x, y = entity.x or 0, entity.y or 0
    local angle, radius = entity.AngleOffset, entity.Radius
    local mainRectangle = utils.rectangle(x-8, y-8, 16, 16)

    local spinnerx = x + math.sin(angle * degToRad) * radius
    local spinnery = y + -1 * (math.cos(angle * degToRad) * radius)
    local spinnerRectangle = utils.rectangle(spinnerx-8, spinnery-8, 16, 16)
    table.insert(nodeRect, spinnerRectangle)

    return mainRectangle, nodeRect
end

local lastEntityX = 0
local lastEntityY = 0

function CassetteRotatingSpinner.onMove(room, entity, nodeIndex, offsetX, offsetY)
    if nodeIndex ~= 0 then
        if entity.x == lastEntityX then -- checks to see if the entity has moved since the last time onMove was called. if it has, that means the main selection is being moved too, so we skip the following offset
            entity.x = entity.x + offsetX
        end
        if entity.y == lastEntityY then
            entity.y = entity.y + offsetY
        end
    end
    lastEntityX = entity.x
    lastEntityY = entity.y
end

function CassetteRotatingSpinner.delete(room, entity, nodeIndex)
    local roomEntities = room.entities
    for i, e in ipairs(roomEntities) do
        if e == entity then
            table.remove(roomEntities, i)
        end
    end
    return true
end

function CassetteRotatingSpinner.flip(room, entity, horizontal, vertical)
    if horizontal then
        entity.AngleOffset = -entity.AngleOffset % 360
    elseif vertical then
        entity.AngleOffset = (180 - entity.AngleOffset) % 360
    end
end

function CassetteRotatingSpinner.rotate(room, entity, direction)
    entity.AngleOffset = (entity.AngleOffset + 90 * direction) % 360
end

return CassetteRotatingSpinner