local drawableNinePatch = require("structs.drawable_nine_patch")
local drawableSprite = require("structs.drawable_sprite")
local drawableLine = require("structs.drawable_line")
local utils = require("utils")

local CassetteRotatingBlock = {}
local degToRad = math.pi / 180

CassetteRotatingBlock.name = "audiohelper/CassetteRotatingBlock"
CassetteRotatingBlock.depth = -50
CassetteRotatingBlock.nodeLimits = {1, 1}
CassetteRotatingBlock.warnBelowSize = {16, 16}
CassetteRotatingBlock.nodeVisibility = "always"
CassetteRotatingBlock.nodeLineRenderType = false
CassetteRotatingBlock.fieldInformation = {
    Colour = {
        fieldType = "color"
    }
}
CassetteRotatingBlock.placements = {
    name = "cassetterotatingblock",
    data = {
        Tempo = 1.0,
        Radius = 16,
        TicksPerCycle = 4,
        AngleOffset = 0,
        Clockwise = true,
        width = 16,
        height = 16,
        SoundIndex = 35,
        texture = "default",
        Colour = "ffffff",
    },
}

local ninePatchOptions = {
    mode = "fill",
    borderMode = "repeat",
    fillMode = "repeat"
}

local frameTexture = "objects/audiohelper/cassettemovingblock/%s/block"
local smallSpoolTexture = "objects/audiohelper/cassettemovingblock/%s/spool_small/spin00"
local bigSpoolTexture = "objects/audiohelper/cassettemovingblock/%s/spool_big/spin04"
local smallGearTexture = "objects/audiohelper/cassettemovingblock/%s/gear_small/spin00"
local bigGearTexture = "objects/audiohelper/cassettemovingblock/%s/gear_big/spin04"

function CassetteRotatingBlock.sprite(room, entity)
    local radius, x, y = entity.Radius or 16, entity.x or 0, entity.y or 0
    local width, height = entity.width or 16, entity.height or 16

    local addx = math.sin(entity.AngleOffset * degToRad) * radius
    local addy = -1 * (math.cos(entity.AngleOffset * degToRad) * radius)

    local objx = math.floor(x + addx - width/2 + 0.5)
    local objy = math.floor(y + addy - height/2 + 0.5)

    local style = entity.Texture or "default"
    local frame = string.format(frameTexture, style)

    local ninePatch = drawableNinePatch.fromTexture(frame, ninePatchOptions, objx, objy, width, height)
    ninePatch:setColor(entity.Colour)

    local big = true
    if width < 32 or height < 32 then
        big = false
    end

    local gearSprite
    local spoolSprite
    local smallSpool = string.format(smallSpoolTexture, style)
    local bigSpool = string.format(bigSpoolTexture, style)
    local smallGear = string.format(smallGearTexture, style)
    local bigGear = string.format(bigGearTexture, style)
    local blockData = {
        x = math.floor(x + addx + 0.5),
        y = math.floor(y + addy + 0.5),
        depth = -50
    }
    if big then
        gearSprite = drawableSprite.fromTexture(bigGear, blockData)
        spoolSprite = drawableSprite.fromTexture(bigSpool, blockData)
    else
        gearSprite = drawableSprite.fromTexture(smallGear, blockData)
        spoolSprite = drawableSprite.fromTexture(smallSpool, blockData)
    end

    local sprites = ninePatch:getDrawableSprite()
    table.insert(sprites, gearSprite)
    table.insert(sprites, spoolSprite)

    CassetteRotatingBlock.drawLines(sprites, radius, x, y, addx, addy)

    return sprites
end

function CassetteRotatingBlock.drawLines(sprites, radius, x, y, addx, addy)
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

function CassetteRotatingBlock.nodeTexture(room, entity, node)
    if entity.Clockwise then
        return "objects/audiohelper/cassetterotatingblock/cw"
    else
        return "objects/audiohelper/cassetterotatingblock/ccw"
    end
end

function CassetteRotatingBlock.selection(room, entity)
    local pivotRectTable = {}
    
    local x, y = entity.x or 0, entity.y or 0
    local angle, radius, width, height = entity.AngleOffset, entity.Radius, entity.width or 16, entity.height or 16
    local pivotRectangle = utils.rectangle(x-8, y-8, 16, 16)
    table.insert(pivotRectTable, pivotRectangle)

    local objx = math.floor(x + math.sin(angle * degToRad) * radius - width/2 + 0.5)
    local objy = math.floor(y + -1 * (math.cos(angle * degToRad) * radius) - height/2 + 0.5)
    local objectRectangle = utils.rectangle(objx, objy, width, height)

    return objectRectangle, pivotRectTable
end

function CassetteRotatingBlock.move(room, entity, nodeIndex, offsetX, offsetY)
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
            CassetteRotatingBlock.flipAngle(entity)
        end
        entity.Radius = math.abs(offset)
    elseif above == true then
        if offset > entity.Radius then -- pass over 0 case
            CassetteRotatingBlock.flipAngle(entity)
            entity.Radius = offset - entity.Radius
        else
            entity.Radius = entity.Radius - offset
        end
    elseif above == false then
        if -offset > entity.Radius then -- pass over 0 case
            CassetteRotatingBlock.flipAngle(entity)
            entity.Radius = -offset - entity.Radius
        else
            entity.Radius = entity.Radius + offset
        end
    end
end

function CassetteRotatingBlock.updateMoveSelection(room, entity, nodeIndex, selection, offsetX, offsetY)
    -- do normal motion for the pivot
    if nodeIndex ~= 0 then
        selection.x = selection.x + offsetX
        selection.y = selection.y + offsetY
        return
    end
    CassetteRotatingBlock.fixSelection(entity,selection)
end

function CassetteRotatingBlock.updateResizeSelection(room, entity, node, selection, offsetX, offsetY, directionX, directionY)
    -- when moving the left/up handle then the entity also moves, this block undoes that motion. "should" be done in a handled resize function, but that would require remaking a lot of existing code so its easer to just undo the one part we dont want
    if directionX < 0 then
        entity.x = entity.x + offsetX
    end
    if directionY < 0 then
        entity.y = entity.y + offsetY
    end

    CassetteRotatingBlock.fixSelection(entity,selection)
    selection.width = entity.width
    selection.height = entity.height
end

function CassetteRotatingBlock.fixSelection(entity, selection)
    local x, y = entity.x or 0, entity.y or 0
    local angle, radius, width, height = entity.AngleOffset, entity.Radius, entity.width or 16, entity.height or 16
    selection.x = x + math.sin(angle * degToRad) * radius - width/2
    selection.y = y + -1 * (math.cos(angle * degToRad) * radius) - height/2
end

function CassetteRotatingBlock.flipAngle(entity)
    entity.AngleOffset = (entity.AngleOffset + 180) % 360
end

function CassetteRotatingBlock.delete(room, entity, nodeIndex)
    local roomEntities = room.entities
    for i, e in ipairs(roomEntities) do
        if e == entity then
            table.remove(roomEntities, i)
        end
    end
    return true
end

-- todo: when this function gets a nodeIndex param, have the object run the current code, but the pivot change the rotate direction
function CassetteRotatingBlock.flip(room, entity, horizontal, vertical)
    if horizontal then
        entity.AngleOffset = -entity.AngleOffset % 360
    else
        entity.AngleOffset = (180 - entity.AngleOffset) % 360
    end
    return true
end

-- todo: when this function gets a nodeIndex param, have this only work when selecting the object
function CassetteRotatingBlock.rotate(room, entity, direction)
    entity.AngleOffset = (entity.AngleOffset + 15 * direction) % 360
    return true
end

return CassetteRotatingBlock