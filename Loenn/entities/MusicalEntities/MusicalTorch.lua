local MusicalTorch = {}

MusicalTorch.name = "audiohelper/MusicalTorch"
MusicalTorch.depth = -8500
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
        -- Sprite = "torch",
    }
}

-- function MusicalTorch.texture(room, entity)
--     local texture = entity.Sprite or "objects/temple/torch"

--     return texture
-- end

return MusicalTorch