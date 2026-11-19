local drawableLine = require("structs.drawable_line")
local drawableSprite = require("structs.drawable_sprite")
local utils = require("utils")

local noteNames = {
    {"A2", 24},
    {"G#/Ab2", 23},
    {"G2", 22},
    {"F#/Gb2", 21},
    {"F2", 20},
    {"E2", 19},
    {"D#/Eb2", 18},
    {"D2", 17},
    {"C#/Db2", 16},
    {"C2", 15},
    {"B1", 14},
    {"A#/Bb1", 13},
    {"A1", 12},
    {"G#/Ab1", 11},
    {"G1", 10},
    {"F#/Gb1", 9},
    {"F1", 8},
    {"E1", 7},
    {"D#/Eb1", 6},
    {"D1", 5},
    {"C#/Db1", 4},
    {"C1", 3},
    {"B0", 2},
    {"A#/Bb0", 1},
    {"A0", 0},
}

local sounds = {
    "event:/vert_audiohelper/bouncystring/harp"
}

local BouncyString = {}

BouncyString.name = "audiohelper/BouncyString"
BouncyString.depth = -50
BouncyString.warnBelowSize = {16, 8}
BouncyString.fieldInformation = {
    pitch = {
        options = noteNames,
        editable = false
    },
    sound = {
        options = sounds,
        editable = true
    },
}
BouncyString.placements = {
    name = "bouncystring",
    data = {
        width = 32,
        sound = "event:/vert_audiohelper/bouncystring/harp",
        pitch = 12,
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