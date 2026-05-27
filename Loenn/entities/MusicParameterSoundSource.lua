local drawableSprite = require("structs.drawable_sprite")
local drawableFunc = require("structs.drawable_function")
local utils = require("utils")
local drawing = require("utils.drawing")

---@type EntityHandler<Entity>
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
    local sprite = drawableSprite.fromTexture("objects/audiohelper/MusicParameterSoundSource", entity)
    local circle = drawableFunc.fromFunction(function()
        drawing.callKeepOriginalColor(function()
            love.graphics.setColor {1,1,1,0.5}
            love.graphics.circle("line", entity.x, entity.y, entity.Radius*8 or 0);
        end)
    end)
    return {sprite,circle}
end

function MusicParameterSoundSource.selection(room, entity)
    return utils.rectangle(entity.x-12, entity.y-12, 24, 24)
end

return MusicParameterSoundSource