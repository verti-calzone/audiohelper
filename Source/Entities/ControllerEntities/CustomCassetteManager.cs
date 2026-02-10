using Celeste.Mod.Entities;
using MonoMod.RuntimeDetour;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.audiohelper.Entities;

[CustomEntity("audiohelper/CustomCassetteBlockManager")]
[Tracked]

public class CustomCassetteBlockManager : Entity {
    public float Tempo;
    public int CountInLength;
    public int LoopStart;
    public int LoopEnd;
    public int NotesPerTick;
    public int TicksPerSwap;
    public int StartingColour;
    public bool UseBlue;
    public bool UsePink;
    public bool UseYellow;
    public bool UseGreen;

    public string Song;
    public string TickSound;
    public string SwapSound;

    public bool UsesFlag;
    public string EnableFlag;
    public bool FreezeMode;

  public CustomCassetteBlockManager(EntityData data)
    {
        Tempo = data.Float("Tempo");
        CountInLength = data.Int("CountInLength");
        LoopStart = data.Int("LoopStart");
        LoopEnd = data.Int("LoopEnd");
        NotesPerTick = data.Int("NotesPerTick");
        TicksPerSwap = data.Int("TicksPerSwap");
        StartingColour = data.Int("StartingColour");
        UseBlue = data.Bool("UseBlue");
        UsePink = data.Bool("UsePink");
        UseYellow = data.Bool("UseYellow");
        UseGreen = data.Bool("UseGreen");

        Song = data.Attr("CassetteSong");
        TickSound = data.Attr("TickSound");
        SwapSound = data.Attr("SwapSound");

        UsesFlag = data.Bool("UsesFlag");
        EnableFlag = data.Attr("Flag");
        FreezeMode = data.Bool("FreezeMode");
    }
    public override void Awake(Scene scene)
    {
        base.Awake(scene);
		foreach (CassetteBlockManager oldManager in scene.Entities.FindAll<CassetteBlockManager>())
        {
            oldManager.RemoveSelf();
        }
        if(!UseBlue && !UsePink && !UseYellow && !UseGreen) RemoveSelf();
    }
    public override void Removed(Scene scene)
    {
        base.Removed(scene);
    }
}