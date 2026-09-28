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
    public float tickProgress = -1f, loopProgress, radius, radianOffset;
    public bool moving = false, frozen = false, readyToLeave = false, clockwise = true;

    public float tickTimer;
    public int BpT, ticksPerSwap, tickCounter = -1, ticksPerLoop, tickSwapCounter;

    public Action silentUpdateAction, swapAction;
    public Action<Vector2> moveAction;

    public CassetteRotator(Action<Vector2> OnMove, Action OnSilentUpdate, Action OnSwap, float degreeOffset, float ctorRadius, int TpL, bool direction) : base()
    {
        moveAction = OnMove;
        silentUpdateAction = OnSilentUpdate;
        swapAction = OnSwap;

        degreeOffset -= 90;
        degreeOffset %= 360f;
        if (degreeOffset < 0) degreeOffset += 360;
        radianOffset = Calc.DegToRad * degreeOffset;

        radius = ctorRadius;
        ticksPerLoop = TpL > 0 ? TpL : 1;

        clockwise = direction;
    }

    public override void SilentUpdate(int ticksUntilReset, int BpT, int TpS, float tempoMult)
    {
        // only run this on the rotator's first SilentUpdate
        if (readyToLeave) return;
        readyToLeave = true;

        // establishing CBM-dependant fields
        tickTimer = 1f / 6 * (BpT / tempoMult);
        tickCounter = 0 - ticksUntilReset;
        tickSwapCounter = 0 - ticksUntilReset;
        ticksPerSwap = TpS;

        // Calculate loopProgress
        loopProgress = 0 - (ticksUntilReset / (float)ticksPerLoop);
        loopProgress %= 1;
        if (loopProgress < 0) loopProgress += 1;

        // moves to its "home" spot to attach static movers, then moves to where it needs to be to sync
        Move(0);
        silentUpdateAction();
        Move(loopProgress);
    }

    public override void Update()
    {
        base.Update();
        if (!frozen) ElapseTime(Engine.DeltaTime);
    }

    public void ElapseTime(float time)
    {
        if (tickProgress >= 0)
        {
            tickProgress = Calc.Approach(tickProgress, 1, time / tickTimer);
            loopProgress = (tickCounter - 1 + tickProgress) / ticksPerLoop; // oboe because the tickCounter has already advanced by the time this code runs
        }
        Move(loopProgress);
    }

    public override void Tick()
    {
        tickProgress = 0;
        tickCounter++;
        tickCounter %= ticksPerLoop;

        if (tickSwapCounter == 0) swapAction();
        tickSwapCounter++;
        if (tickSwapCounter > 0) tickSwapCounter %= ticksPerSwap;
    }

    public void Move(float progress)
    {
        if (!clockwise) progress = 1 - progress;
        progress *= (2 * MathF.PI);
        progress += radianOffset;
        Vector2 newPosition = Calc.AngleToVector(progress, radius);
        moveAction(newPosition);
    }
}