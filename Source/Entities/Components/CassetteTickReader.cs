using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System.Runtime.CompilerServices;
using FMOD.Studio;
using MonoMod.Utils;
using System;
using MonoMod.Cil;
using Celeste.Mod.Helpers;
using Iced.Intel;
using System.Collections.Generic;

namespace Celeste.Mod.audiohelper.Entities;

[CustomEntity("audiohelper/CassetteTickReader")]
[Tracked(true)]

public class CassetteTickReader : Component {
  	public CassetteTickReader() : base(active: true, visible: false){}

    public virtual void SilentUpdate(int ticksUntilReset, int BpT, int TpS, float tempoMult){}

    public virtual void ElapseTime(float time){}

    public virtual void Tick(){}

	// // HOOKS // //

    public static void OnSilentUpdateBlocks(On.Celeste.CassetteBlockManager.orig_SilentUpdateBlocks orig, CassetteBlockManager cbm)
    {
        orig(cbm);
		int ticksUntilReset = -1; // starts at -1 because the while loop always adds one more than it needs to
		int BpT = DynamicData.For(cbm).Get<int>("beatsPerTick");
		int TpS = DynamicData.For(cbm).Get<int>("ticksPerSwap");
		int BpS = BpT*TpS;
		float swapProgress = (cbm.beatIndex % BpS) / (float)BpS;

        // sees if the cbm set back 1 or 2 swaps
        if (cbm.currentIndex == cbm.maxBeat - 2) ticksUntilReset += TpS;

        // additional ticks back
        float countdown = 1;
		while (swapProgress < countdown)
		{
			countdown -= 1f/TpS;
			ticksUntilReset++;
        }

		foreach(CassetteTickReader ctr in cbm.Scene.Tracker.GetComponents<CassetteTickReader>()) ctr.SilentUpdate(ticksUntilReset, BpT, TpS, cbm.tempoMult);
    }
	public static void AdvanceMusicDelegate(CassetteBlockManager cbm)
    {
        int BpT = DynamicData.For(cbm).Get<int>("beatsPerTick");
        if (cbm.beatIndex % BpT == 0) foreach (CassetteTickReader ctr in cbm.SceneAs<Level>().Tracker.GetComponents<CassetteTickReader>()) ctr.Tick();
    }
    public static void IL_AdvanceMusic(ILContext il)
    {
        ILCursor cursor = new ILCursor(il);

        if (cursor.TryGotoNextBestFit(MoveType.After,
            instr => instr.MatchLdcI4(1),
            instr => instr.MatchAdd(),
            instr => instr.MatchStfld<CassetteBlockManager>("beatIndex")))
        {
            cursor.EmitLdarg0();
            cursor.EmitDelegate(AdvanceMusicDelegate);
        }
        else throw new Exception("Audiohelper: Could not make AdvanceMusic hook!");
    }
	public static void FreezeDelegate(float time)
		{
		foreach (CassetteTickReader ctr in Engine.Scene.Tracker.GetComponents<CassetteTickReader>()) ctr.ElapseTime(time);
		}
	public static void IL_Freeze(ILContext il)
	{
		ILCursor cursor = new ILCursor(il);

		if (cursor.TryGotoNextBestFit(MoveType.After,
			instr => instr.MatchCallvirt<CassetteBlockManager>("AdvanceMusic")))
		{
			cursor.EmitLdarg0();
			cursor.EmitDelegate(FreezeDelegate);
		}
		else throw new Exception("Audiohelper: Could not make CassetteMover Freeze hook!");
	}
}