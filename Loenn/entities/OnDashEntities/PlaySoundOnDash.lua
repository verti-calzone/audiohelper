local PlaySoundOnDash = {}

PlaySoundOnDash.name = "audiohelper/PlaySoundOnDash"
PlaySoundOnDash.depth = -8500
PlaySoundOnDash.texture = "objects/audiohelper/PlaySoundOnDash"
PlaySoundOnDash.placements = {
    name = "playsoundondash",
    data = {
        event = "event:/game/06_reflection/supersecret_dashflavour",
        param = "dash_direction",
    }
}

return PlaySoundOnDash