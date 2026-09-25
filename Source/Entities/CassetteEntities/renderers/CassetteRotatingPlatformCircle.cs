using System;
using Celeste.Mod.Entities;
using Celeste.Mod.Helpers;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.audiohelper.Entities;

[CustomEntity("audiohelper/CassetteRotatingPlatformCircle")]
[Tracked]

public class CassetteRotatingPlatformCircle : Entity {
    public Vector2 pivot;
    public float radius, segments;
    public Color outerColour = Calc.HexToColor("2a1923"), innerColour = Calc.HexToColor("160b12");

    public CassetteRotatingPlatformCircle(Vector2 centre, float length)
    {
        Depth = 9001;
        pivot = centre + 4 * Vector2.UnitY;
        radius = length;
        segments = Calc.Clamp(radius/4,2,16);
    }

    public override void Render()
    {
        Draw.Circle(pivot, radius - 1, outerColour, (int)segments);
        Draw.Circle(pivot, radius + 1, outerColour, (int)segments);
        Draw.Circle(pivot, radius, innerColour, (int)segments);
        base.Render();
    }
}