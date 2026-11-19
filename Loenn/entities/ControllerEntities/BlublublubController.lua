local BlublublubController = {}

local modes = {
    ["Vanilla"] = 0,
    ["Any"] = 1,
    ["All"] = 2,
    ["Never"] = 3
}

BlublublubController.name = "audiohelper/BlublublubController"
BlublublubController.depth = -8500
BlublublubController.texture = "objects/audiohelper/BlublublubController"
BlublublubController.fieldInformation = {
    Mode = {
        options = modes,
        editable = false,
    },
}
BlublublubController.placements = {
    name = "blublublubcontroller",
    data = {
        Flag = "",
        UsesFlag = false,
        Mode = 0,
        FishMode = false,
        SplashFix = false
    }
}
return BlublublubController