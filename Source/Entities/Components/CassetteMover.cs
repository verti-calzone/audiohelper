using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections.Generic;

namespace Celeste.Mod.audiohelper.Entities;

[CustomEntity("audiohelper/CassetteMover")]
[Tracked(true)]

public class CassetteMover : CassetteTickReader
{
	public Vector2[] vertices;
	public List<Vector2> vertexList = [];
	public float progress, easedProgress;
	public bool moving = false, frozen = false, skipNextEnd = false;
	public int activeVertex, tickOffset = 0;

	public enum Easers { SineInOut, CubeIn }
	public enum Speeds { SlowContinuous, SlowStop, FastContinuous, FastStop, Custom }
	public Easers easer;
	public Speeds speed;

	public string customSpeed;
	public int customTicksPerWait, customTicksPerMove = 1;
	public float tickTimer;
	public int BpT, TpS, ticksPerMove, ticksPerWait, tickCounter, tickCounterLength;

	public Action startMoveAction, endMoveAction, silentUpdateAction;
	public Action<Vector2> moveAction;

	public CassetteMover(Action<Vector2> OnMove, Action OnStartMove, Action OnEndMove, Action OnSilentUpdate) : base()
	{
		moveAction = OnMove;
		startMoveAction = OnStartMove;
		endMoveAction = OnEndMove;
		silentUpdateAction = OnSilentUpdate;
	}

	public override void EntityAdded(Scene scene)
	{
		base.EntityAdded(scene);
		if (speed == Speeds.Custom)
		{
			string[] CustomSpeeds = customSpeed?.Split(',');
			if (CustomSpeeds.Length != 2) return;
			int.TryParse(CustomSpeeds[0], out customTicksPerWait);
			int.TryParse(CustomSpeeds[1], out customTicksPerMove);
			if (customTicksPerWait < 0) customTicksPerWait = 0;
			if (customTicksPerMove <= 0) customTicksPerMove = 1;
		}
	}

	public void FakeAwake(int BpT, int TpS, float tempoMult)
	{
		tickTimer = 1f / 6 * (BpT / tempoMult);

		switch (speed)
		{
			case Speeds.SlowContinuous:
				ticksPerMove = TpS;
				ticksPerWait = 0;
				break;
			case Speeds.FastContinuous:
				ticksPerMove = 1;
				ticksPerWait = 0;
				break;
			case Speeds.SlowStop:
				ticksPerMove = TpS;
				ticksPerWait = TpS;
				break;
			case Speeds.FastStop:
				ticksPerMove = 1;
				ticksPerWait = TpS - 1;
				break;
			case Speeds.Custom:
				ticksPerMove = customTicksPerMove;
				ticksPerWait = customTicksPerWait;
				break;
		}
		tickCounterLength = ticksPerMove + ticksPerWait;
	}

	public override void SilentUpdate(int ticksUntilReset, int BpT, int TpS, float tempoMult)
	{
		FakeAwake(BpT, TpS, tempoMult);

		// Applies offset here
		ticksUntilReset += tickOffset;

		// sets the tick counter to reach 0 on the next swap
		tickCounter = ((tickCounterLength - ticksUntilReset) % tickCounterLength + tickCounterLength) % tickCounterLength;

		// INTENTIONAL INTEGER DIVISION ALERT, this sets the active vertex back one for each cycle that needs to be done before the next blue swap
		int VerticesBehind = ticksUntilReset / tickCounterLength;
		activeVertex = (vertices.Length - ((1 + VerticesBehind) % vertices.Length)) % vertices.Length;

		if (((tickCounter - 1 + tickCounterLength) % tickCounterLength) >= ticksPerWait) // if the object is supposed to be moving when the tickcounter is at its current value
		{
			activeVertex++;
			activeVertex %= vertices.Length;
		}
		NewPosition(vertices[activeVertex]);

		silentUpdateAction();
	}

	public override void Update()
	{
		base.Update();
		ElapseTime(Engine.DeltaTime);
	}

	public override void ElapseTime(float time)
	{
		if (moving)
		{
			progress = Calc.Approach(progress, 1, time / (ticksPerMove * tickTimer));
			Move();
		}
	}

	public override void Tick()
	{
		if (tickCounter == 0 && moving)
		{
			progress = 1;
			EndMove();
		}
		if (tickCounter == ticksPerWait && !frozen)
		{
			moving = true;
			StartMove();

		}
		tickCounter++;
		tickCounter %= tickCounterLength;
	}

	public void Move()
	{
		easedProgress = easer == Easers.SineInOut ? Ease.SineInOut(progress) : Ease.CubeIn(progress);
		NewPosition(Vector2.Lerp(vertices[activeVertex], vertices[(activeVertex + 1) % vertices.Length], easedProgress));
	}
	public void NewPosition(Vector2 newPosition)
	{
		moveAction(newPosition);
	}

	public void StartMove()
	{
        if (vertices[activeVertex] == vertices[(activeVertex + 1) % vertices.Length])
        {
            skipNextEnd = true;
            return;
        }

        startMoveAction();
	}

	public void EndMove()
	{
        Move();
		progress = 0;
		moving = false;
		activeVertex++;
		activeVertex %= vertices.Length;

		if (skipNextEnd)
        {
            skipNextEnd = false;
            return;
        }
		endMoveAction();
	}
}