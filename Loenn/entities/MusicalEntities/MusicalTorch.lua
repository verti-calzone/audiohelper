local MusicalTorch = {}

local modeNames = {
    {"Get Reset Value", 1},
    {"Set Reset Value", 2},
    {"Increment Mode", 3}
}

MusicalTorch.name = "audiohelper/MusicalTorch"
MusicalTorch.depth = -8500
MusicalTorch.texture = "objects/temple/torch00"
MusicalTorch.fieldInformation = {
    Colour = {
        fieldType = "color"
    },
    Mode = {
        minimum = 0,
        maximum = 1,
        options = modeNames,
        editable = false
    }
}
MusicalTorch.placements = {
    name = "musicaltorch",
    data = {
        Alpha = 1.0,
        StartRadius = 48,
        EndRadius = 64,
        Colour = "ffffff",
        StayLit = true,
        ActivationSound = "event:/game/05_mirror_temple/torch_activate",
        MusicParameter = "",
        ParameterValue = 0.0,
        IncrementMode = false,
        ParameterResetValue = 0,
        Mode = 2
    }
}


return MusicalTorch