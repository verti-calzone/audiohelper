local drawableNinePatch = require("structs.drawable_nine_patch")
local drawableRectangle = require("structs.drawable_rectangle")
local drawableSprite = require("structs.drawable_sprite")

local MusicalKevin = {}

local axesOptions = {
    Both = "both",
    Vertical = "vertical",
    Horizontal = "horizontal"
}

local modeNames = {
    {"Get Reset Value", 1},
    {"Set Reset Value", 2},
    {"Increment Mode", 3}
}

MusicalKevin.name = "audiohelper/MusicalKevin"
MusicalKevin.depth = 0
MusicalKevin.warnBelowSize = {24, 24}
MusicalKevin.fieldInformation = {
    axes = {
        options = axesOptions,
        editable = false
    },
    Mode = {
        minimum = 0,
        maximum = 1,
        options = modeNames,
        editable = false
    }
}
MusicalKevin.placements = {
    name = "musicalkevin",
    data = {
        width = 24,
        height = 24,
        axes = "both",
        chillout = false,
        ActivateSound = "event:/game/06_reflection/crushblock_activate",
        MoveSound = "event:/game/06_reflection/crushblock_move_loop",
        ImpactSound = "event:/game/06_reflection/crushblock_impact",
        ReturnSound = "event:/game/06_reflection/crushblock_return_loop",
        WaypointSound = "event:/game/06_reflection/crushblock_rest_waypoint",
        RestSound = "event:/game/06_reflection/crushblock_rest",
        MusicParameter = "",
        ParameterValue = 0,
        IncrementMode = false,
        ParameterResetValue = 0,
        Mode = 2
    }
}

local frameTextures = {
    none = "objects/crushblock/block00",
    horizontal = "objects/crushblock/block01",
    vertical = "objects/crushblock/block02",
    both = "objects/crushblock/block03"
}

local ninePatchOptions = {
    mode = "border",
    borderMode = "repeat"
}

local kevinColor = {98 / 255, 34 / 255, 43 / 255}
local smallFaceTexture = "objects/crushblock/idle_face"
local giantFaceTexture = "objects/crushblock/giant_block00"

function MusicalKevin.sprite(room, entity)
    local x, y = entity.x or 0, entity.y or 0
    local width, height = entity.width or 24, entity.height or 24

    local axes = entity.axes or "both"
    local chillout = entity.chillout

    local giant = height >= 48 and width >= 48 and chillout
    local faceTexture = giant and giantFaceTexture or smallFaceTexture

    local frameTexture = frameTextures[axes] or frameTextures["both"]
    local ninePatch = drawableNinePatch.fromTexture(frameTexture, ninePatchOptions, x, y, width, height)

    local rectangle = drawableRectangle.fromRectangle("fill", x + 2, y + 2, width - 4, height - 4, kevinColor)
    local faceSprite = drawableSprite.fromTexture(faceTexture, entity)

    faceSprite:addPosition(math.floor(width / 2), math.floor(height / 2))

    local sprites = ninePatch:getDrawableSprite()

    table.insert(sprites, 1, rectangle:getDrawableSprite())
    table.insert(sprites, 2, faceSprite)

    return sprites
end

function MusicalKevin.rotate(room, entity, direction)
    local axes = (entity.axes or ""):lower()

    if axes == "horizontal" then
        entity.axes = "vertical"

    elseif axes == "vertical" then
        entity.axes = "horizontal"
    end

    return true
end

return MusicalKevin