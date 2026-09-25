local drawableSprite = require("structs.drawable_sprite")
local drawableLine = require("structs.drawable_line")
local resortPlatformHelper = require("helpers.resort_platforms")
local utils = require("utils")

local CassetteRotatingPlatform = {}
local textures = {
    "default", "cliffside"
}
local degToRad = math.pi / 180

CassetteRotatingPlatform.name = "audiohelper/CassetteRotatingPlatform"
CassetteRotatingPlatform.depth = -10100
CassetteRotatingPlatform.warnBelowSize = {8, 8}
CassetteRotatingPlatform.fieldInformation = {
    texture = {
        options = textures,
        editable = true,
    },
    Radius = {
        fieldType = "integer",
    },
    TicksPerCycle = {
        fieldType = "integer",
    }
}
CassetteRotatingPlatform.placements = {
    name = "cassetterotatingplatform",
    data = {
        Tempo = 1.0,
        Radius = 16,
        TicksPerCycle = 4,
        AngleOffset = 0,
        Clockwise = true,
        width = 16,
        SoundIndex = 5,
        texture = "default",
    },
}

function CassetteRotatingPlatform.sprite(room, entity)
    local sprites = {}
    local radius, x, y = entity.Radius or 16, entity.x or 0, entity.y or 0

    local centreSprite
    if entity.Clockwise then
        centreSprite = drawableSprite.fromTexture("objects/audiohelper/cassetterotatingplatform/cw", entity)
    else
        centreSprite = drawableSprite.fromTexture("objects/audiohelper/cassetterotatingplatform/ccw", entity)
    end
    table.insert(sprites, centreSprite)

    local addx = math.sin(entity.AngleOffset * degToRad) * entity.Radius
    local addy = -1 * (math.cos(entity.AngleOffset * degToRad) * entity.Radius)


    local platformData = {
        x = x + addx - entity.width/2,
        y = y + addy,
        depth = -50
    }
    resortPlatformHelper.addPlatformSprites(sprites, entity, platformData)

    local mainLine = drawableLine.fromPoints({x, y, x + addx, y + addy}, "202020", 1)
    mainLine.depth = 5000
    table.insert(sprites, mainLine)

    -- drawing the circle

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

function CassetteRotatingPlatform.addPlatformSprite()
    
end

function CassetteRotatingPlatform.selection(room, entity)
    local nodeRect = {}
    
    local x, y = entity.x or 0, entity.y or 0
    local angle, radius, width = entity.AngleOffset, entity.Radius, entity.width or 8
    local mainRectangle = utils.rectangle(x-8, y, 16, 8)

    local altx = x + math.sin(angle * degToRad) * radius
    local alty = y + -1 * (math.cos(angle * degToRad) * radius)
    local altRectangle = utils.rectangle(altx-width/2, alty, width, 8)
    table.insert(nodeRect, mainRectangle)

    return altRectangle, nodeRect
end

local lastEntityX = 0
local lastEntityY = 0

function CassetteRotatingPlatform.onMove(room, entity, nodeIndex, offsetX, offsetY)
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

function CassetteRotatingPlatform.delete(room, entity, nodeIndex)
    local roomEntities = room.entities
    for i, e in ipairs(roomEntities) do
        if e == entity then
            table.remove(roomEntities, i)
        end
    end
    return true
end

function CassetteRotatingPlatform.flip(room, entity, horizontal, vertical)
    if horizontal then
        entity.AngleOffset = -entity.AngleOffset % 360
    elseif vertical then
        entity.AngleOffset = (180 - entity.AngleOffset) % 360
    end
end

function CassetteRotatingPlatform.rotate(room, entity, direction)
    entity.AngleOffset = (entity.AngleOffset + 90 * direction) % 360
end

function CassetteRotatingPlatform.updateResizeSelection(room, entity, node, selection, offsetX, offsetY, directionX, directionY)
    selection.width = selection.width + offsetX
    if selection.width < 8 then
        selection.width = 8
    else
        selection.x = selection.x + offsetX * -0.5
        if directionX < 0 then
            entity.x = entity.x + offsetX
        end
    end
end

return CassetteRotatingPlatform