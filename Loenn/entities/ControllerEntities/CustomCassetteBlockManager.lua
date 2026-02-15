local CustomCassetteBlockManager = {}
local colourNames = {
    ["Blue"] = 0,
    ["Pink"] = 1,
    ["Yellow"] = 2,
    ["Green"] = 3
}

CustomCassetteBlockManager.name = "audiohelper/CustomCassetteBlockManager"
CustomCassetteBlockManager.depth = -8500
CustomCassetteBlockManager.texture = "objects/audiohelper/CustomCassetteBlockManager"
CustomCassetteBlockManager.fieldInformation = {
    StartingColour = {
        fieldType = "integer",
        options = colourNames,
        editable = false,
    },
    NumberOfBlocks = {
        fieldType = "integer",
        options = {2,3,4},
        editable = false,
    }
}
CustomCassetteBlockManager.placements = {
    name = "customcassetteblockmanager",
    data = {
        Tempo = 90,
        CountInLength = 16,
        LoopStart = 0,
        LoopEnd = 255,
        NotesPerTick = 4,
        TicksPerSwap = 2,
        StartingColour = 0,
        NumberOfBlocks = 2,
        MusicParameter = "sixteenth_note",
        NoteOffset = 0,

        CassetteSong = "",
        TickSound = "event:/game/general/cassette_block_switch_1",
        SwapSound = "event:/game/general/cassette_block_switch_2",

        UsesFlag = false,
        Flag = "",
        FreezeMode = false,
    }
}

return CustomCassetteBlockManager