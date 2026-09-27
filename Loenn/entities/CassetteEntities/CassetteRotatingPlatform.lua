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
CassetteRotatingPlatform.nodeLimits = {1, 1}
CassetteRotatingPlatform.nodeVisibility = "always"
CassetteRotatingPlatform.nodeLineRenderType = false
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

    local addx = math.sin(entity.AngleOffset * degToRad) * radius
    local addy = -1 * (math.cos(entity.AngleOffset * degToRad) * radius)


    local platformData = {
        x = x + addx - entity.width/2,
        y = y + addy,
        depth = -50
    }
    resortPlatformHelper.addPlatformSprites(sprites, entity, platformData)

    CassetteRotatingPlatform.drawLines(sprites, radius, x, y, addx, addy)

    return sprites
end

function CassetteRotatingPlatform.drawLines(sprites, radius, x, y, addx, addy)
    local mainLine = drawableLine.fromPoints({x, y, x + addx, y + addy}, "303030", 1)
    mainLine.depth = 5000
    table.insert(sprites, mainLine)

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

function CassetteRotatingPlatform.nodeTexture(room, entity)
    if entity.Clockwise then
        return "objects/audiohelper/cassetterotatingplatform/cw"
    else
        return "objects/audiohelper/cassetterotatingplatform/ccw"
    end
end

function CassetteRotatingPlatform.selection(room, entity)
    local pivotRectTable = {}
    
    local x, y = entity.x or 0, entity.y or 0
    local angle, radius, width = entity.AngleOffset, entity.Radius, entity.width or 8
    local pivotRectangle = utils.rectangle(x-8, y, 16, 8)
    table.insert(pivotRectTable, pivotRectangle)

    local objx = x + math.sin(angle * degToRad) * radius
    local objy = y + -1 * (math.cos(angle * degToRad) * radius)
    local objectRectangle = utils.rectangle(objx-width/2, objy, width, 8)

    return objectRectangle, pivotRectTable
end

function CassetteRotatingPlatform.move(room, entity, nodeIndex, offsetX, offsetY)
    if nodeIndex ~= 0 then
        entity.x = entity.x + offsetX
        entity.y = entity.y + offsetY
        entity.nodes[1].x = entity.x
        entity.nodes[1].y = entity.y
        return
    end

    local angle = entity.AngleOffset % 360
    local above, vertical
    if angle <= 45 or angle >= 315 then
        above = true
        vertical = true
    elseif angle >= 135 and angle <= 225 then
        above = false
        vertical = true
    else
        vertical = false
        if angle < 180 then
            above = false
        else
            above = true
        end
    end

    local offset
    if vertical == true then
        offset = offsetY
    else
        offset = offsetX
    end
    if entity.Radius == 0 then -- pass through 0 case
        if (offset > 0 and above == true) or (offset < 0 and above == false) then
            CassetteRotatingPlatform.flipAngle(entity)
        end
        entity.Radius = math.abs(offset)
    elseif above == true then
        if offset > entity.Radius then -- pass over 0 case
            CassetteRotatingPlatform.flipAngle(entity)
            entity.Radius = offset - entity.Radius
        else
            entity.Radius = entity.Radius - offset
        end
    elseif above == false then
        if -offset > entity.Radius then -- pass over 0 case
            CassetteRotatingPlatform.flipAngle(entity)
            entity.Radius = -offset - entity.Radius
        else
            entity.Radius = entity.Radius + offset
        end
    end
end

function CassetteRotatingPlatform.updateMoveSelection(room, entity, nodeIndex, selection, offsetX, offsetY)
    -- do normal motion for the pivot
    if nodeIndex ~= 0 then
        selection.x = selection.x + offsetX
        selection.y = selection.y + offsetY
        return
    end
    CassetteRotatingPlatform.fixSelection(entity,selection)
end

function CassetteRotatingPlatform.updateResizeSelection(room, entity, node, selection, offsetX, offsetY, directionX, directionY)
    -- when moving the left handle then the entity also moves, this block undoes that motion. "should" be done in a handled resize function, but that would require remaking a lot of existing code so its easer to just undo the one part we dont want
    if directionX < 0 then
        entity.x = entity.x + offsetX
    end
    CassetteRotatingPlatform.fixSelection(entity,selection)
    selection.width = entity.width
end

function CassetteRotatingPlatform.fixSelection(entity, selection)
    local x, y = entity.x or 0, entity.y or 0
    local angle, radius, width = entity.AngleOffset, entity.Radius, entity.width or 8
    selection.x = x + math.sin(angle * degToRad) * radius - width/2
    selection.y = y + -1 * (math.cos(angle * degToRad) * radius)
end

function CassetteRotatingPlatform.flipAngle(entity)
    entity.AngleOffset = (entity.AngleOffset + 180) % 360
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

-- todo: when this function gets a nodeIndex param, have the object run the current code, but the pivot change the rotate direction
function CassetteRotatingPlatform.flip(room, entity, horizontal, vertical)
    if horizontal then
        entity.AngleOffset = -entity.AngleOffset % 360
    else
        entity.AngleOffset = (180 - entity.AngleOffset) % 360
    end
    return true
end

-- todo: when this function gets a nodeIndex param, have this only work when selecting the object
function CassetteRotatingPlatform.rotate(room, entity, direction)
    entity.AngleOffset = (entity.AngleOffset + 15 * direction) % 360
    return true
end

return CassetteRotatingPlatform