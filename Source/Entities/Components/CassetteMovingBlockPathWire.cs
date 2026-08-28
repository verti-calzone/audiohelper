using System;
using Celeste.Mod.Entities;
using Celeste.Mod.Helpers;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.audiohelper.Entities;

[CustomEntity("audiohelper/CassetteMovingBlockPathWire")]
[Tracked]

public class CassetteMovingBlockPathWire : Component {
    public SimpleCurve curve;
    public Vector2 midPoint, midPointDown, offset;
    public float baseOffset;
    public Color colour = Calc.HexToColor("201828");
    public float tightTimer, looseTimer;
    public float progress;

public delegate float Easer(float t);
    public static Easer Taut = delegate (float t)
    {
        if(t<0.25) return 4*t;
        return 1+0.5f*(float)Math.Sin(8*Math.PI*(double)t);
    };
    public static Easer Sag = delegate (float t)
    {
        double a = (double)t - 0.5;
        double b = 0.5 - 2 * a * a;
        double c = (float)Math.Sin(3 * Math.PI * (double)t);
        double d = (double)t - 1;
        double e = 1 + d * d * d;
        return (float)(e + c*b);
    };

    public CassetteMovingBlockPathWire(Vector2 start, Vector2 end, float sag) : base(active: true, visible: true)
    {
        curve = new SimpleCurve(start, end, Vector2.Zero);
        midPoint = (start + end) / 2;
        baseOffset = sag;
        midPointDown = new Vector2(midPoint.X, midPoint.Y + baseOffset);
        offset = midPointDown;
    }

    public void Tighten(float time)
    {
        tightTimer = time;
        looseTimer = 0;
        progress = 0;
    }
    public void Loosen(float time)
    {
        looseTimer = time;
        tightTimer = 0;
        progress = 0;
    }

    public override void Update()
    {
        if (tightTimer > 0)
        {
            progress = Calc.Approach(progress, 1, Engine.DeltaTime / tightTimer);
            offset = Vector2.Lerp(midPointDown, midPoint, Taut(progress));
            tightTimer -= Engine.DeltaTime;
        }
        else if (looseTimer > 0)
        {
            progress = Calc.Approach(progress, 1, Engine.DeltaTime / looseTimer);
            offset = Vector2.Lerp(midPoint, midPointDown, Sag(progress));
            tightTimer -= Engine.DeltaTime;
        }
        base.Update();
    }

    public override void Render()
    {
        if (IsVisible())
        {
            curve.Control = offset;
            curve.Render(colour,8,2f);
        }
        base.Render();
    }

    private bool IsVisible()
    {
        return CullHelper.IsCurveVisible(curve, 2f);
    }
}