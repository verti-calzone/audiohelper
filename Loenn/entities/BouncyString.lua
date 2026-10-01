local drawableLine = require("structs.drawable_line")
local utils = require("utils")

local BouncyString = {}

BouncyString.name = "audiohelper/BouncyString"
BouncyString.placements = {
    name = "bouncystring",
    data = {
        width = 32
    }
}
function BouncyString.sprite(room, entity)
    return drawableLine.fromPoints({entity.x, entity.y+4, entity.x + entity.width, entity.y+4}, "808080", 2)
end

function BouncyString.selection(room, entity)
    return utils.rectangle(entity.x, entity.y, entity.width, 8)
end

return BouncyString