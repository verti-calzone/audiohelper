local drawableSprite = require("structs.drawable_sprite")
local utils = require("utils")

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

function MusicParameterSoundSource.draw(room, entity, viewport)
    local MusicParameterSoundSourceSprite = drawableSprite.fromTexture("objects/audiohelper/MusicParameterSoundSource", entity)
---@diagnostic disable-next-line: undefined-global
    love.graphics.circle("line", entity.x, entity.y, entity.Radius*8)
    MusicParameterSoundSourceSprite:draw()
end

function MusicParameterSoundSource.selection(room, entity)
    return utils.rectangle(entity.x-12, entity.y-12, 24, 24)
end

return MusicParameterSoundSource