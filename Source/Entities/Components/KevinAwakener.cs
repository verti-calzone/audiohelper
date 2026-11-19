using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.audiohelper.Entities;

[CustomEntity("audiohelper/KevinAwakener")]
[Tracked]

public class KevinAwakener : Component {
    public KevinAwakener() : base(active: true, visible: false) { }
    public override void EntityAwake()
    {
        base.EntityAwake();

        BlublublubController blub = Entity.Scene.Tracker.GetEntity<BlublublubController>();
        if (blub is null) return;
        if (!blub.IsActive(Entity.SceneAs<Level>())) return;

        if (blub.splashFix)
        {
            foreach (Component component in Entity.Components)
            {
                if (component is WaterInteraction waterInteraction)
                {
                    foreach (Water water in Entity.Scene.Tracker.GetEntities<Water>())
                    {
                        if (waterInteraction.Check(water))
                        {
                            water.contains.Add(waterInteraction);
                        }
                    } return;
                }
            }
        }
    }

    public static void OnKevinCtor(On.Celeste.CrushBlock.orig_ctor_Vector2_float_float_Axes_bool orig, CrushBlock self, Vector2 position, float width, float height, CrushBlock.Axes axes, bool chillout)
    {
        orig(self, position, width, height, axes, chillout);
        self.Add(new KevinAwakener());
    }
}