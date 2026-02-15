local MusicalTorch = {}

MusicalTorch.name = "audiohelper/MusicalTorch"
MusicalTorch.depth = -8500
MusicalTorch.texture = "objects/temple/torch00"
MusicalTorch.fieldInformation = {
    Colour = {
        fieldType = "color"
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
    }
}


return MusicalTorch