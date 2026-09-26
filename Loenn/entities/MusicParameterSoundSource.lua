local drawableSprite = require("structs.drawable_sprite")
local drawableFunc = require("structs.drawable_function")
local drawableLine = require("structs.drawable_line")
local utils = require("utils")
local drawing = require("utils.drawing")

local MusicParameterSoundSource = {}
MusicParameterSoundSource.name = "audiohelper/MusicParameterSoundSource"
MusicParameterSoundSource.depth = 0
MusicParameterSoundSource.placements = {
    name = "musicparametersoundsource",
    data = {
        MusicParameter = "",
        EdgeValue = 0,
        CentreValue = 1,
        Radius = 8,
    }
}

function MusicParameterSoundSource.sprite(room, entity)
    local sprites = {}
    local sprite = drawableSprite.fromTexture("objects/audiohelper/MusicParameterSoundSource", entity)
    table.insert(sprites, sprite)


    local radius, x, y = entity.Radius * 8 or 16, entity.x or 0, entity.y or 0
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
        local lineSegment = drawableLine.fromPoints({x1, y1, x2, y2}, "c0c0c0", 1)
        lineSegment.depth = 5000
        table.insert(sprites, lineSegment)
    end

    return sprites
end

function MusicParameterSoundSource.selection(room, entity)
    return utils.rectangle(entity.x-12, entity.y-12, 24, 24)
end

return MusicParameterSoundSource