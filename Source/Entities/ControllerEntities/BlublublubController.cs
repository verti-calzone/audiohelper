using System.Collections.Generic;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.RuntimeDetour;

namespace Celeste.Mod.audiohelper.Entities;

[CustomEntity("audiohelper/BlublublubController")]
[Tracked]
public class BlublublubController : Entity {

    public string flag;
    public bool splashFix, fishMode, usesFlag, bigFaceFix;

    public enum Modes { vanilla, any, all, never };
    public Modes mode;

    public BlublublubController(EntityData data, Vector2 offset){
        flag = data.Attr("Flag");
        usesFlag = data.Bool("UsesFlag");
        mode = data.Enum("Mode", Modes.vanilla);
        fishMode = data.Bool("FishMode");

        splashFix = data.Bool("SplashFix");
	}

    public bool IsActive(Level level)
    {
        if (usesFlag && !level.Session.GetFlag(flag)) return false;
        else return true;
    }

    public override void Added(Scene scene)
    {
        base.Added(scene);
        foreach (BlublublubController blub in scene.Tracker.GetEntities<BlublublubController>()) if (blub != this) Remove();
    }

    // // Hooks // //

    public static Hook SubmergedHook;

    public delegate bool orig_Crushblock_get_Submerged(CrushBlock self);
    public static bool GetSubmerged(orig_Crushblock_get_Submerged orig, CrushBlock self)
    {
        BlublublubController blub = self.Scene.Tracker.GetEntity<BlublublubController>();
        if (blub is null) return orig(self);
        if (!blub.IsActive(self.SceneAs<Level>())) return orig(self);
        
        bool blublublub;
        
        switch (blub.mode)
        {
            case Modes.vanilla:
            default:
                if (blub.bigFaceFix) blublublub = self.Scene.CollideCheck<Water>(new Rectangle((int)self.CenterX - 8, (int)self.CenterY + 4, 16, 6));
                else blublublub = orig(self);
                break;

            case Modes.any:
                blublublub = self.Scene.CollideCheck<Water>(new Rectangle((int)self.X, (int)self.Y, (int)self.Width, (int)self.Height));
                break;

            case Modes.all:
                List<Entity> waters = self.Scene.Tracker.Entities[typeof(Water)];
                blublublub = true;

                while (blublublub)
                {
                    for (int i = 0; i <= self.Width / 8; i++)
                    {
                        for (int j = 0; j <= self.Height / 8; j++)
                        {
                            bool inWater = false;
                            foreach (Water water in waters)
                            {
                                if (water.Collidable && water.CollidePoint(new(self.X + 8 * i, self.Y + 8 * j)))
                                {
                                    inWater = true;
                                }
                            }
                            if (!inWater) blublublub = false;
                        }
                    }
                }
                break;

            case Modes.never:
                blublublub = false;
                break;
        }
        return blub.fishMode ? !blublublub : blublublub;
    }
}