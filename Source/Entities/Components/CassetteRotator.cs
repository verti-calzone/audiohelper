using System;
using System.Collections.Generic;
using System.Diagnostics;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.audiohelper.Entities;

[CustomEntity("audiohelper/CassetteRotator")]
[Tracked(true)]

public class CassetteRotator : CassetteTickReader
{
    public float tickProgress, loopProgress, radius, angleOffset;
    public bool moving = false, frozen = false, readyToLeave = false;
    public int tickOffset = 0;
    public Vector2 pivot;

    public float tickTimer;
    public int BpT, TpS, tickCounter, ticksPerLoop;

    public Action silentUpdateAction;
    public Action<Vector2> moveAction;

    public CassetteRotator(Action<Vector2> OnMove, Action OnSilentUpdate, Vector2 entityPosition, float offset, float ctorRadius, int TpL) : base()
    {
        moveAction = OnMove;
        silentUpdateAction = OnSilentUpdate;

        pivot = entityPosition;

        offset %= 360f;
        if (offset < 0) offset += 360;

        angleOffset = Calc.DegToRad * offset;

        radius = ctorRadius;
        ticksPerLoop = TpL;
    }

    public void FakeAwake(int BpT, int TpS, float tempoMult)
    {
        tickTimer = 1f / 6 * (BpT / tempoMult);
    }

    public override void SilentUpdate(int ticksUntilReset, int BpT, int TpS, float tempoMult)
    {
        // only run this on the rotator's first SilentUpdate
        if (readyToLeave) return;
        readyToLeave = true;

        FakeAwake(BpT, TpS, tempoMult);

        // Calculate loopProgress

        loopProgress = 0 - (ticksUntilReset / ticksPerLoop);
        loopProgress %= 1;
        if (loopProgress < 0) loopProgress += 1;

        Move(loopProgress);

        silentUpdateAction();
    }

    public override void Update()
    {
        base.Update();
        if (!frozen) ElapseTime(Engine.DeltaTime);
    }

    public void ElapseTime(float time)
    {
        tickProgress = Calc.Approach(tickProgress, 1, time / tickTimer);
        //Logger.Info("audiohelper", "tickProgress is: " + tickProgress);
        loopProgress = (tickCounter + (tickProgress > 0 ? tickProgress : 0 )) / ticksPerLoop;
        Move(loopProgress);
    }

    public override void Tick()
    {
        tickProgress = 0;
        tickCounter++;
        tickCounter %= ticksPerLoop;
    }

    public void Move(float progress)
    {
        progress *= 2*MathF.PI;
        Vector2 newPosition = pivot + Calc.AngleToVector(progress + angleOffset, radius);
        moveAction(newPosition);
    }
}