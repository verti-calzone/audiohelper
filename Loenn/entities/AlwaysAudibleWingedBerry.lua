local drawableSprite = require("structs.drawable_sprite")
local AlwaysAudibleWingedBerry = {}

AlwaysAudibleWingedBerry.name = "audiohelper/AlwaysAudibleWingedBerry"
AlwaysAudibleWingedBerry.depth = -8500
AlwaysAudibleWingedBerry.fieldInformation = {
    order = {
        fieldType = "integer",
    },
    checkpointID = {
        fieldType = "integer"
    }
}

AlwaysAudibleWingedBerry.placements = {
    name = "alwaysaudiblewingedberry",
    data = {
        IncludeFlapSound = true,
        order = -1,
        checkpointID = -1,
    }
}
function AlwaysAudibleWingedBerry.sprite(room, entity)
    local sprite = drawableSprite.fromTexture("collectables/strawberry/wings00", entity)
    return sprite
end

return AlwaysAudibleWingedBerry