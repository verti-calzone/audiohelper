local BusMuter = {}
local BusNames = {
    "bus:/gameplay_sfx",
    "bus:/music",
    "bus:/music/stings",
    "bus:/ui_sfx"
}

BusMuter.name = "audiohelper/BusMuter"
BusMuter.depth = -8500
BusMuter.texture = "objects/audiohelper/BusMuter"
BusMuter.fieldInformation = {
    Bus = {
        options = BusNames,
        editable = true
    },
}
BusMuter.placements = {
    name = "busmuter",
    data = {
        Bus = "",
        Flag = "",
    }
}
return BusMuter