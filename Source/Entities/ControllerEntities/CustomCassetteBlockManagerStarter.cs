using Celeste.Mod.Entities;
using MonoMod.RuntimeDetour;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.audiohelper.Entities;

[CustomEntity("audiohelper/CustomCassetteBlockManagerStarter")]
[Tracked]

public class CustomCassetteBlockManagerStarter : Entity {
    public CustomCassetteBlockManagerStarter() {}
    public override void Awake(Scene scene)
    {
        base.Awake(scene);
        foreach(CustomCassetteBlockManager ccbm in scene.Tracker.GetEntities<CustomCassetteBlockManager>()) ccbm.OnLevelStart();
    }

    public override void Removed(Scene scene)
    {
        base.Removed(scene);
    }
}