using System.Runtime.CompilerServices;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.audiohelper.Entities;

[CustomEntity("audiohelper/PlaySoundOnDash")]
[Tracked]

public class PlaySoundOnDash : Entity {

    public string Event;
    public string Parameter;

    public PlaySoundOnDash(EntityData data, Vector2 offset) : base(data.Position + offset)
  {
    Event = data.Attr("event","event:/game/06_reflection/supersecret_dashflavour");
    Parameter = data.Attr("param","dash_direction");
  }
    private DashListener dashListener;

    public override void Awake(Scene scene) {
      base.Awake(scene);
      Add(dashListener = new DashListener());
      dashListener.OnDash = [MethodImpl] (Vector2 dir) =>
      {
        int dashdir = 0;
        if (dir.X < 0f && dir.Y == 0f)
        {
          dashdir = 1;
        }
        else if (dir.X < 0f && dir.Y < 0f)
        {
          dashdir = 2;
        }
        else if (dir.X == 0f && dir.Y < 0f)
        {
          dashdir = 3;
        }
        else if (dir.X > 0f && dir.Y < 0f)
        {
          dashdir = 4;
        }
        else if (dir.X > 0f && dir.Y == 0f)
        {
          dashdir = 5;
        }
        else if (dir.X > 0f && dir.Y > 0f)
        {
          dashdir = 6;
        }
        else if (dir.X == 0f && dir.Y > 0f)
        {
          dashdir = 7;
        }
        else if (dir.X < 0f && dir.Y > 0f)
        {
          dashdir = 8;
        }
        Audio.Play(Event, base.Scene.Tracker.GetEntity<Player>()?.Position ?? Vector2.Zero, Parameter, dashdir);
      };
    }

    public override void Removed(Scene scene)
    {
        base.Removed(scene);
    }
}