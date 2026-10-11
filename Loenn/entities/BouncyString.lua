local drawableLine = require("structs.drawable_line")
local drawableSprite = require("structs.drawable_sprite")
local utils = require("utils")

local BouncyString = {}

BouncyString.name = "audiohelper/BouncyString"
BouncyString.depth = -50
BouncyString.warnBelowSize = {16, 8}
BouncyString.placements = {
    name = "bouncystring",
    data = {
        width = 32
    }
}

function BouncyString.sprite(room, entity)
    local sprites = {}
    local x, y, width = entity.x or 0, entity.y or 0, entity.width or 8
    local capData = {
        depth = -60,
        x = x,
        y = y + 4,
    }

    table.insert(sprites, drawableLine.fromPoints({x, y+3.5, x + width, y+3.5}, "e2b2a0", 1))
    table.insert(sprites, drawableLine.fromPoints({x, y+4.5, x + width, y+4.5}, "b38374", 1))
    table.insert(sprites, drawableLine.fromPoints({x, y+5.5, x + width, y+5.5}, "3b1a17", 1))

    local leftCap = drawableSprite.fromTexture("objects/audiohelper/bouncystring/wirecap", capData)
    local rightCap = drawableSprite.fromTexture("objects/audiohelper/bouncystring/wirecap", capData)

    rightCap:addPosition(width,0)
    rightCap:setScale(-1,1)

    table.insert(sprites, leftCap)
    table.insert(sprites, rightCap)

    return sprites
end

function BouncyString.selection(room, entity)
    return utils.rectangle(entity.x, entity.y, entity.width, 8)
end

return BouncyString