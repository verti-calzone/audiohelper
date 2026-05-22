local drawableSprite = require("structs.drawable_sprite")
local MusicalBell = {}
local noteNames = {
    {"E6", 24},
    {"D#/Eb6", 23},
    {"D6", 22},
    {"C#/Db6", 21},
    {"C6", 20},
    {"B5", 19},
    {"A#/Bb5", 18},
    {"A5", 17},
    {"G#/Ab5", 16},
    {"G5", 15},
    {"F#/Gb5", 14},
    {"F5", 13},
    {"E5", 12},
    {"D#/Eb5", 11},
    {"D5", 10},
    {"C#/Db5", 9},
    {"C5", 8},
    {"B4", 7},
    {"A#/Bb4", 6},
    {"A4", 5},
    {"G#/Ab4", 4},
    {"G4", 3},
    {"F#/Gb4", 2},
    {"F4", 1},
    {"E4",  0},
}
local modeNames = {
    {"Get Reset Value", 1},
    {"Set Reset Value", 2},
    {"Increment Mode", 3}
}
local sounds ={
    "event:/vert_audiohelper/bell",
    "event:/vert_audiohelper/chime"
}

MusicalBell.name = "audiohelper/MusicalBell"
MusicalBell.depth = -8500
MusicalBell.fieldInformation = {
    pitch = {
        fieldType = "integer",
        options = noteNames,
        editable = false
    },
    sound = {
        options = sounds,
        editable = true
    },
    colour = {
        fieldType = "color"
    },
    VolumeBoost = {
       minimum = 0,
       maximum = 1
    },
    Mode = {
        minimum = 0,
        maximum = 1,
        options = modeNames,
        editable = false
    }
}
MusicalBell.placements = {
    name = "musicalbell",
    data = {
        sound = "event:/vert_audiohelper/bell",
        pitch = 0,
        colour = "c0c0c0",
        VolumeBoost = 0,
        SetFlag = "",
        MusicParameter = "",
        ParameterValue = 0,
        ParameterResetValue = 0,
        Timer = 1.0,
        Mode = 2
    }
}
function MusicalBell.sprite(room, entity)
    local sprite = drawableSprite.fromTexture("objects/audiohelper/bell/idle00", entity)
    sprite:setJustification(0.5, 0.167)
    return sprite
end

return MusicalBell