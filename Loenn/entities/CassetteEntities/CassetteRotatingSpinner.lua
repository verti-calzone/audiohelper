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
CassetteRotatingSpinner.nodeLimits = {1, 1}
CassetteRotatingSpinner.nodeVisibility = "always"
CassetteRotatingSpinner.nodeLineRenderType = false
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
    local spinnerSprite
    local radius, x, y = entity.Radius, entity.x, entity.y

    local addx = math.sin(entity.AngleOffset * degToRad) * radius
    local addy = -1 * (math.cos(entity.AngleOffset * degToRad) * radius)

    local objx = math.floor(x + addx + 0.5)
    local objy = math.floor(y + addy + 0.5)

    local spinnerData = {
        x = objx,
        y = objy,
        depth = -50
    }

    spinnerSprite = drawableSprite.fromTexture(textureStyles[entity.Style], spinnerData)
    table.insert(sprites, spinnerSprite)

    CassetteRotatingSpinner.drawLines(sprites, radius, x, y, objx, objy)

    return sprites
end

function CassetteRotatingSpinner.drawLines(sprites, radius, x, y, objx, objy)
    local mainLine = drawableLine.fromPoints({x, y, objx, objy}, "303030", 1)
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

function CassetteRotatingSpinner.nodeTexture(room, entity)
    entity.nodes[1].x = entity.x
    entity.nodes[1].y = entity.y
    if entity.Clockwise then
        return cwStyles[entity.Style]
    else
        return ccwStyles[entity.Style]
    end
end

function CassetteRotatingSpinner.selection(room, entity)
    local pivotRectTable = {}
    
    local x, y = entity.x or 0, entity.y or 0
    local angle, radius = entity.AngleOffset, entity.Radius
    local pivotRectangle = utils.rectangle(x-8, y-8, 16, 16)
    table.insert(pivotRectTable, pivotRectangle)

    local objx = math.floor(x + math.sin(angle * degToRad) * radius + 0.5)
    local objy = math.floor(y + -1 * (math.cos(angle * degToRad) * radius) + 0.5)
    local objectRectangle = utils.rectangle(objx-8, objy-8, 16, 16)

    return objectRectangle, pivotRectTable
end

function CassetteRotatingSpinner.move(room, entity, nodeIndex, offsetX, offsetY)
    if nodeIndex ~= 0 then
        entity.x = entity.x + offsetX
        entity.y = entity.y + offsetY
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
            CassetteRotatingSpinner.flipAngle(entity)
        end
        entity.Radius = math.abs(offset)
    elseif above == true then
        if offset > entity.Radius then -- pass over 0 case
            CassetteRotatingSpinner.flipAngle(entity)
            entity.Radius = offset - entity.Radius
        else
            entity.Radius = entity.Radius - offset
        end
    elseif above == false then
        if -offset > entity.Radius then -- pass over 0 case
            CassetteRotatingSpinner.flipAngle(entity)
            entity.Radius = -offset - entity.Radius
        else
            entity.Radius = entity.Radius + offset
        end
    end
end

function CassetteRotatingSpinner.updateMoveSelection(room, entity, nodeIndex, selection, offsetX, offsetY)
    -- do normal motion for the pivot
    if nodeIndex ~= 0 then
        selection.x = selection.x + offsetX
        selection.y = selection.y + offsetY
        return
    end

    local x, y = entity.x or 0, entity.y or 0
    local angle, radius = entity.AngleOffset, entity.Radius
    selection.x = x + math.sin(angle * degToRad) * radius - 8
    selection.y = y + -1 * (math.cos(angle * degToRad) * radius) -8
end

function CassetteRotatingSpinner.flipAngle(entity)
    entity.AngleOffset = (entity.AngleOffset + 180) % 360
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

-- todo: when this function gets a nodeIndex param, have the object run the current code, but the pivot change the rotate direction
function CassetteRotatingSpinner.flip(room, entity, horizontal, vertical)
    if horizontal then
        entity.AngleOffset = -entity.AngleOffset % 360
    else
        entity.AngleOffset = (180 - entity.AngleOffset) % 360
    end
    return true
end

-- todo: when this function gets a nodeIndex param, have this only work when selecting the object
function CassetteRotatingSpinner.rotate(room, entity, direction)
    entity.AngleOffset = (entity.AngleOffset + 15 * direction) % 360
    return true
end

return CassetteRotatingSpinner