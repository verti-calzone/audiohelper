using System;
using Celeste.Mod.Entities;
using Celeste.Mod.Helpers;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.audiohelper.Entities;

[CustomEntity("audiohelper/CassetteMovingBlockPathWire")]
[Tracked]

public class CassetteMovingBlockPathWire : Component {
    public SimpleCurve curve1, curve2;
    public Vector2 start, end, midPoint, midPointDown, offset, vector, perpendicular;
    public Vector2 sag = new Vector2(0f, 4f);
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

    public CassetteMovingBlockPathWire(Vector2 startPoint, Vector2 endPoint, float shift) : base(active: true, visible: true)
    {
        start = startPoint;
        end = endPoint;
        
        vector = end - start;
        perpendicular = vector.Perpendicular().SafeNormalize() * shift;

        curve1 = new SimpleCurve(start + perpendicular, end + perpendicular, Vector2.Zero);
        curve2 = new SimpleCurve(start - perpendicular, end - perpendicular, Vector2.Zero);

        midPoint = (start + end) / 2;
        offset = sag;
        midPointDown = midPoint + offset;

    }

    public void Tighten()
    {
        tightTimer = 0.5f;
        looseTimer = 0;
        progress = 0;
    }
    public void Loosen()
    {
        looseTimer = 1f;
        tightTimer = 0;
        progress = 0;
    }

    public override void Update()
    {
        if (tightTimer > 0)
        {
            progress = Calc.Approach(progress, 1, Engine.DeltaTime / tightTimer);
            offset = Vector2.Lerp(sag, Vector2.Zero, Taut(progress));
            tightTimer -= Engine.DeltaTime;
        }
        else if (looseTimer > 0)
        {
            progress = Calc.Approach(progress, 1, Engine.DeltaTime / looseTimer);
            offset = Vector2.Lerp(Vector2.Zero, sag, Sag(progress));
            tightTimer -= Engine.DeltaTime;
        }
        base.Update();
    }

    public override void Render()
    {
        if (IsVisible())
        {
            curve1.Control = midPoint + offset + perpendicular;
            curve1.Render(colour, 8, 2f);

            curve2.Control = midPoint + offset - perpendicular;
            curve2.Render(colour, 8, 2f);
        }
        base.Render();
    }

    private bool IsVisible()
    {
        return CullHelper.IsCurveVisible(curve1, 2f) || CullHelper.IsCurveVisible(curve2, 2f);
    }
}