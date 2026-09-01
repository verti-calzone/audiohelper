local drawableNinePatch = require("structs.drawable_nine_patch")
local drawableSprite = require("structs.drawable_sprite")
local drawableLine = require("structs.drawable_line")
local utils = require("utils")

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
CassetteMovingBlock.depth = -9999
CassetteMovingBlock.nodeLimits = {1, -1}
CassetteMovingBlock.nodeLineRenderType = "line"
CassetteMovingBlock.warnBelowSize = {16, 16}
CassetteMovingBlock.fieldInformation = {
    Easer = {
        options = easers,
        editable = false,
    },
    Speed = {
        options = speeds,
        editable = false,
    },
    Colour = {
        fieldType = "color"
    }
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
        Texture = "default",
        Colour = "ffffff",
        SoundIndex = 35
    },
}

local ninePatchOptions = {
    mode = "fill",
    borderMode = "repeat",
    fillMode = "repeat"
}

-- RENDERING -- 

local frameTexture = "objects/audiohelper/cassettemovingblock/%s/block"
local smallSpoolTexture = "objects/audiohelper/cassettemovingblock/%s/spool_small/spin00"
local bigSpoolTexture = "objects/audiohelper/cassettemovingblock/%s/spool_big/spin04"
local smallGearTexture = "objects/audiohelper/cassettemovingblock/%s/gear_small/spin00"
local bigGearTexture = "objects/audiohelper/cassettemovingblock/%s/gear_big/spin04"

local function addWires(sprites, entity, big, x, y, newx, newy)
    local addWidth = math.floor(entity.width / 2)
    local addHeight = math.floor(entity.height / 2)
    local points = {x + addWidth, y + addHeight, newx + addWidth, newy + addHeight}
    local leftLine = drawableLine.fromPoints(points, "201828", 1)
    local rightLine = drawableLine.fromPoints(points, "201828", 1)
    local wireOffset

    if big then
        wireOffset = 7
    else 
        wireOffset = 4
    end

    leftLine:setOffset(0, wireOffset)
    rightLine:setOffset(0, -wireOffset)

    leftLine.depth = 5000
    rightLine.depth = 5000

    for _, sprite in ipairs(leftLine:getDrawableSprite()) do
        table.insert(sprites, sprite)
    end

    for _, sprite in ipairs(rightLine:getDrawableSprite()) do
        table.insert(sprites, sprite)
    end
end

function CassetteMovingBlock.sprite(room, entity)
    local x, y = entity.x or 0, entity.y or 0
    local width, height = entity.width or 16, entity.height or 16
    local nodes = entity.nodes

    local style = entity.Texture or "default"
    local frame = string.format(frameTexture, style)
    local smallSpool = string.format(smallSpoolTexture, style)
    local bigSpool = string.format(bigSpoolTexture, style)
    local smallGear = string.format(smallGearTexture, style)
    local bigGear = string.format(bigGearTexture, style)

    local ninePatch = drawableNinePatch.fromTexture(frame, ninePatchOptions, x, y, width, height)
    ninePatch:setColor(entity.Colour)

    local big = true
    if width < 32 or height < 32 then
        big = false
    end

    local gearSprite
    local spoolSprite
    if big then
        gearSprite = drawableSprite.fromTexture(bigGear, entity)
        spoolSprite = drawableSprite.fromTexture(bigSpool, entity)
    else
        gearSprite = drawableSprite.fromTexture(smallGear, entity)
        spoolSprite = drawableSprite.fromTexture(smallSpool, entity)
    end
    gearSprite:addPosition(math.floor(width / 2), math.floor(height / 2))
    spoolSprite:addPosition(math.floor(width / 2), math.floor(height / 2))

    local sprites = ninePatch:getDrawableSprite()
    table.insert(sprites, gearSprite)
    table.insert(sprites, spoolSprite)
    
    addWires(sprites, entity, big, x, y, nodes[1].x, nodes[1].y)
    if #nodes ~= 1 then
        addWires(sprites, entity, big, x, y, nodes[#nodes].x, nodes[#nodes].y)
    end

    local gearSprites = {}
    for i, node in ipairs(nodes) do
        if big then
            gearSprites[i] = drawableSprite.fromTexture(bigGear, node)
        else
            gearSprites[i] = drawableSprite.fromTexture(smallGear, node)
        end
        gearSprites[i]:addPosition(math.floor(width / 2 ), math.floor(height / 2))
        table.insert(sprites, gearSprites[i])

        if i < #nodes then
            addWires(sprites, entity, big, node.x, node.y, nodes[i+1].x, nodes[i+1].y)
        end
    end

    

    return sprites
end

function CassetteMovingBlock.nodeSprite(room, entity, node)
    local x, y = node.x or 0, node.y or 0
    local width, height = entity.width or 16, entity.height or 16

    local style = entity.Texture or "default"
    local frame = string.format(frameTexture, style)
    local ninePatch = drawableNinePatch.fromTexture(frame, ninePatchOptions, x, y, width, height)

    local alphaColour = utils.getColor(entity.Colour)
    alphaColour[4] = 0.33
    ninePatch:setColor(alphaColour)

    local sprites = ninePatch:getDrawableSprite()
    return sprites
end

function CassetteMovingBlock.selection(room, entity)
    local x, y = entity.x or 0, entity.y or 0
    local width, height = entity.width or 16, entity.height or 16
    local halfWidth, halfHeight = math.floor(entity.width / 2), math.floor(entity.height / 2)

    local nodes = entity.nodes or {{x = 0, y = 0}}
    local nodeX, nodeY, centerNodeX, centerNodeY = {}, {}, {}, {}
    for i, node in ipairs(nodes) do
        centerNodeX[i] = node.x + halfWidth
        centerNodeY[i] = node.y + halfHeight
    end

    local style = entity.Texture or "default"
    local smallGear = string.format(smallGearTexture, style)
    local bigGear = string.format(bigGearTexture, style)
    local gearSprite
    if width < 32 or height < 32 then
        gearSprite = drawableSprite.fromTexture(smallGear, entity)
    else
        gearSprite = drawableSprite.fromTexture(bigGear, entity)
    end

    local gearWidth, gearHeight = gearSprite.meta.width, gearSprite.meta.height

    local mainRectangle = utils.rectangle(x, y, width, height)
    local nodeRectangles = {}
    for i, _ in ipairs(nodes) do
        nodeRectangles[i] = utils.rectangle(centerNodeX[i] - math.floor(gearWidth / 2), centerNodeY[i] - math.floor(gearHeight / 2), gearWidth, gearHeight)
    end

    return mainRectangle, nodeRectangles
end

return CassetteMovingBlock