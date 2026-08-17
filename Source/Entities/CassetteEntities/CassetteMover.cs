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

[CustomEntity("audiohelper/CassetteMover")]
[Tracked(true)]

public class CassetteMover : Entity {
	public Vector2[] Vertices;
	public List<Vector2> VertexList = [];
	public float Progress;
	public bool Moving = false, Frozen = false;
	public int ActiveVertex, TickOffset = 0;
	public enum Easers {SineInOut, CubeIn}
	public enum Speeds {SlowContinuous, SlowStop, FastContinuous, FastStop, Custom}
	public Easers Easer;
	public Speeds Speed;
	public CassetteBlockManager cbm;
	public CassetteListener cassetteListener;
	public float TickTimer;
	public int BpT, TpS, TicksPerMove, TicksPerWait, TickCounter, TickCounterLength;

  	public CassetteMover(EntityData data, Vector2 offset)
	{
		Add(cassetteListener = new CassetteListener(0)); // creates a cbm if one is not already present, and lets this entity set a custom tempo
		//Position = offset;
	}

    public void FakeAwake(int BpT, int TpS, float tempoMult)
    {
		TickTimer = 1f/6 * (BpT/tempoMult);

		switch (Speed)
		{
			case Speeds.SlowContinuous:
				TicksPerMove = TpS;
				TicksPerWait = 0;
				break;
			case Speeds.FastContinuous:
				TicksPerMove = 1;
				TicksPerWait = 0;
				break;
            case Speeds.SlowStop:
                TicksPerMove = TpS;
                TicksPerWait = TpS;
                break;
            case Speeds.FastStop:
                TicksPerMove = 1;
                TicksPerWait = TpS - 1;
                break;
			case Speeds.Custom:
				// TicksPerMove = CustomTicksPerMove;
                // TicksPerWait = CustomTicksPerWait;
                break;
        }
		TickCounterLength = TicksPerMove + TicksPerWait;
    }

	public void SilentUpdate(int TicksUntilReset, int BpT, int TpS, float tempoMult)
    {
        FakeAwake(BpT, TpS, tempoMult);

		// Applies offset here
		TicksUntilReset += TickOffset;

        // sets the tick counter to reach 0 on the next swap
        TickCounter = ((TickCounterLength - TicksUntilReset) % TickCounterLength + TickCounterLength) % TickCounterLength;

        // INTENTIONAL INTEGER DIVISION ALERT, this sets the active vertex back one for each cycle that needs to be done before the next blue swap
        int VerticesBehind = TicksUntilReset / TickCounterLength;
        ActiveVertex = (Vertices.Length - ((1 + VerticesBehind) % Vertices.Length)) % Vertices.Length;

		if (((TickCounter - 1 + TickCounterLength) % TickCounterLength ) >= TicksPerWait) // if the object is supposed to be moving when the tickcounter is at its current value
		{
			ActiveVertex++; // set it ahead one vertex
			ActiveVertex %= Vertices.Length;
		}
        Position = Vertices[ActiveVertex];
    }

    public override void Update()
	{
		base.Update();
        if (Moving)
		{
			Progress = Calc.Approach(Progress, 1, Engine.DeltaTime / (TicksPerMove*TickTimer));
            Move();
		}

    }

    public void Tick()
	{
        if (TickCounter == 0 && Moving)
		{
			Progress = 1;
			EndMove();
        }
        if (TickCounter == TicksPerWait && !Frozen)
		{
			Moving = true;
			StartMove();

        }
        TickCounter++;
		TickCounter %= TickCounterLength;
    }

    public void Move()
	{
		Position = Vector2.Lerp(Vertices[ActiveVertex], Vertices[(ActiveVertex + 1) % Vertices.Length], Easer == Easers.SineInOut ? Ease.SineInOut(Progress) : Ease.CubeIn(Progress));
    }

	public virtual void StartMove() {}

    public virtual void EndMove()
	{
		Move();
		Progress = 0;
		Moving = false;
		ActiveVertex++;
		ActiveVertex %= Vertices.Length;
	}


	// // HOOKS // //


	public static void OnSilentUpdateBlocks(On.Celeste.CassetteBlockManager.orig_SilentUpdateBlocks orig, CassetteBlockManager cbm)
    {
        orig(cbm);
		int TicksUntilReset = -1; // starts at -1 because the while loop always adds one more than it needs to
		int BpT = DynamicData.For(cbm).Get<int>("beatsPerTick");
		int TpS = DynamicData.For(cbm).Get<int>("ticksPerSwap");
		int BpS = BpT*TpS;
		float SwapProgress = (cbm.beatIndex % BpS) / (float)BpS;

        // sees if the cbm set back 1 or 2 swaps
        if (cbm.currentIndex == cbm.maxBeat - 2) TicksUntilReset += TpS;

        // additional ticks back
        float countdown = 1;
		while (SwapProgress < countdown)
		{
			countdown -= 1f/TpS;
			TicksUntilReset++;
        }

		Logger.Info("audiohelper", TicksUntilReset + " Ticks until Reset");

		foreach(CassetteMover mover in cbm.Scene.Tracker.GetEntities<CassetteMover>()) mover.SilentUpdate(TicksUntilReset, BpT, TpS, cbm.tempoMult);
    }

	public static void AdvanceMusicDelegate(CassetteBlockManager cbm)
    {
        int BpT = DynamicData.For(cbm).Get<int>("beatsPerTick");
        if (cbm.beatIndex % BpT == 0) foreach (CassetteMover mover in cbm.SceneAs<Level>().Tracker.GetEntities<CassetteMover>()) mover.Tick();
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
}