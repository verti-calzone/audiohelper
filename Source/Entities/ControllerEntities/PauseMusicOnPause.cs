using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.audiohelper.Entities;

[CustomEntity("audiohelper/PauseMusicOnPause")]
[Tracked]
public class PauseMusicOnPause : Entity {

    public string Flag;

    public PauseMusicOnPause(EntityData data, Vector2 offset){
        Flag = data.Attr("Flag");
	}

    public bool IsActive(Level level)
    {
        if (level.Session.GetFlag(Flag)) return true;
        else return false;
    }

    public static void OnStartPauseEffects(On.Celeste.Level.orig_StartPauseEffects orig, Level level)
    {
        orig(level);
        foreach (PauseMusicOnPause pmop in level.Tracker.GetEntities<PauseMusicOnPause>())
        {
            if (!pmop.IsActive(level)) return;
        }
        if(level.Tracker.GetEntity<PauseMusicOnPause>() is not null) Audio.PauseMusic = true;
    }
}