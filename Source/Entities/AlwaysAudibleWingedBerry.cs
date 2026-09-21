using System;
using System.Collections;
using System.Runtime.CompilerServices;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod;

namespace Celeste.Mod.audiohelper.Entities;

[RegisterStrawberry(tracked: true, blocksCollection: false)]
[CustomEntity("audiohelper/AlwaysAudibleWingedBerry")]
[Tracked]
public class AlwaysAudibleWingedBerry : Strawberry
{
    public bool FlapSound;
    public AlwaysAudibleWingedBerry(EntityData data, Vector2 offset, EntityID id) : base(data, offset, id)
    {
        FlapSound = data.Bool("IncludeFlapSound", true);
        Winged = true;
        Add(new DashListener(NewOnDash));
    }

    public override void Added(Scene scene)
    {
        base.Added(scene);
        if (FlapSound) sprite.OnFrameChange = NewOnAnimate;
    }

    public void NewOnAnimate(string id)
    {
        if (!flyingAway && sprite.CurrentAnimationFrame % 9 == 4)
        {
            Audio.Play("event:/vert_audiohelper/winged_berry/strawberry_wingflap", Position);
            flapSpeed = -50f;
        }
        if (sprite.CurrentAnimationFrame == 25)
        {
            lightTween.Start();
            float alpha;
            if (!collected && (CollideCheck<FakeWall>() || CollideCheck<Solid>())) alpha = 0.1f;
            else alpha = 0.2f;
            Audio.Play("event:/game/general/strawberry_pulse", Position);
            SceneAs<Level>().Displacement.AddBurst(Position, 0.6f, 4f, 28f, alpha);
        }
    }

    private void NewOnDash(Vector2 dir)
    {
        if (!flyingAway)
        {
            Depth = -1000000;
            Add(new Coroutine(NewFlyAwayRoutine()));
            flyingAway = true;
        }
    }

    public IEnumerator NewFlyAwayRoutine()
    {
        rotateWiggler.Start();
        flapSpeed = -200f;
        Tween tween = Tween.Create(Tween.TweenMode.Oneshot, Ease.CubeOut, 0.25f, start: true);
        tween.OnUpdate = [MethodImpl(MethodImplOptions.NoInlining)] (Tween t) =>
        {
            flapSpeed = MathHelper.Lerp(-200f, 0f, t.Eased);
        };
        Add(tween);
        yield return 0.1f;
        Audio.Play("event:/vert_audiohelper/winged_berry/strawberry_laugh", Position);
        yield return 0.2f;
        if (!Follower.HasLeader)
        {
            Audio.Play("event:/vert_audiohelper/winged_berry/strawberry_flyaway", Position);
        }
        tween = Tween.Create(Tween.TweenMode.Oneshot, null, 0.5f, start: true);
        tween.OnUpdate = [MethodImpl(MethodImplOptions.NoInlining)] (Tween t) =>
        {
            flapSpeed = MathHelper.Lerp(0f, -200f, t.Eased);
        };
        Add(tween);
    }
}