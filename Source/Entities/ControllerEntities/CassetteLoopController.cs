using System;
using Celeste.Mod.Entities;
using Celeste.Mod.Helpers;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.Cil;

namespace Celeste.Mod.audiohelper.Entities;

[CustomEntity("audiohelper/CassetteLoopController")]
[Tracked]
public class CassetteLoopController : Entity {

    private int LoopStart, LoopEnd;
    private string Flag;

    public CassetteLoopController(EntityData data, Vector2 offset){
        LoopStart = data.Int("LoopStart");
        LoopEnd = data.Int("LoopEnd");
        Flag = data.Attr("Flag");
	}
    public bool IsActive()
    {
        if(SceneAs<Level>().Tracker.GetEntity<CassetteLoopController>() is not null)
        {
            if(SceneAs<Level>().Session.GetFlag(Flag) || string.IsNullOrEmpty(Flag)) return true;
        }
        return false;
    }
    public static int LoopDelegate(int orig, CassetteBlockManager cbm)
    {
        CassetteLoopController clc = cbm.Scene.Tracker.GetEntity<CassetteLoopController>();
        if(clc is not null && clc.IsActive())
        {
            if(cbm.beatIndex > clc.LoopEnd-1) 
            {
                return clc.LoopStart-1;
            }
        }
        return orig;
    }
    public static void IL_CassetteLoopController(ILContext il)
    {
        ILCursor cursor = new ILCursor(il);

        if(cursor.TryGotoNextBestFit(MoveType.After,
            instr => instr.MatchLdfld<CassetteBlockManager>("beatIndex"),
            instr => instr.MatchLdarg0(),
            instr => instr.MatchLdfld<CassetteBlockManager>("beatIndexMax"),
            instr => instr.MatchRem()))
        {
            cursor.EmitLdarg0();
            cursor.EmitDelegate(LoopDelegate);
        }
        else throw new Exception("Audiohelper: Could not make Cassette Loop Controller hook!");
    }
}