using System.Runtime.CompilerServices;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.audiohelper.Entities;

[CustomEntity("audiohelper/PlaySoundOnDirectionalDash")]
[Tracked]

public class PlaySoundOnDirectionalDash : Entity {

    public string EventL;
    public string EventUL;
    public string EventU;
    public string EventUR;
    public string EventR;
    public string EventDR;
    public string EventD;
    public string EventDL;

    public PlaySoundOnDirectionalDash(EntityData data, Vector2 offset) : base(data.Position + offset)
  {
    EventL = data.Attr("eventL");
    EventUL = data.Attr("eventUL");
    EventU = data.Attr("eventU");
    EventUR = data.Attr("eventUR");
    EventR = data.Attr("eventR");
    EventDR = data.Attr("eventDR");
    EventD = data.Attr("eventD");
    EventDL = data.Attr("eventDL");
  }

    public static bool IsActive(Scene scene) {
        return scene.Tracker.GetEntity<PlaySoundOnDash>() is not null;
    }

    private DashListener dashListener;

    public override void Awake(Scene scene) {
      base.Awake(scene);
      Add(dashListener = new DashListener());
      dashListener.OnDash = [MethodImpl] (Vector2 dir) =>
      {
        if (dir.X < 0f && dir.Y == 0f && !string.IsNullOrEmpty(EventL))
        {
          Audio.Play(EventL, base.Scene.Tracker.GetEntity<Player>()?.Position ?? Vector2.Zero);
        }
        else if (dir.X < 0f && dir.Y < 0f && !string.IsNullOrEmpty(EventUL))
        {
          Audio.Play(EventUL, base.Scene.Tracker.GetEntity<Player>()?.Position ?? Vector2.Zero);
        }
        else if (dir.X == 0f && dir.Y < 0f && !string.IsNullOrEmpty(EventU))
        {
          Audio.Play(EventU, base.Scene.Tracker.GetEntity<Player>()?.Position ?? Vector2.Zero);
        }
        else if (dir.X > 0f && dir.Y < 0f && !string.IsNullOrEmpty(EventUR))
        {
          Audio.Play(EventUR, base.Scene.Tracker.GetEntity<Player>()?.Position ?? Vector2.Zero);
        }
        else if (dir.X > 0f && dir.Y == 0f && !string.IsNullOrEmpty(EventR))
        {
          Audio.Play(EventR, base.Scene.Tracker.GetEntity<Player>()?.Position ?? Vector2.Zero);
        }
        else if (dir.X > 0f && dir.Y > 0f && !string.IsNullOrEmpty(EventDR))
        {
          Audio.Play(EventDR, base.Scene.Tracker.GetEntity<Player>()?.Position ?? Vector2.Zero);
        }
        else if (dir.X == 0f && dir.Y > 0f && !string.IsNullOrEmpty(EventD))
        {
          Audio.Play(EventD, base.Scene.Tracker.GetEntity<Player>()?.Position ?? Vector2.Zero);
        }
        else if (dir.X < 0f && dir.Y > 0f && !string.IsNullOrEmpty(EventDL))
        {
          Audio.Play(EventDL, base.Scene.Tracker.GetEntity<Player>()?.Position ?? Vector2.Zero);
        }
      };
    }

    public override void Removed(Scene scene)
    {
        base.Removed(scene);
    }
}