// using Celeste.Mod.Entities;
// using MonoMod.RuntimeDetour;
// using Microsoft.Xna.Framework;
// using Monocle;

// namespace Celeste.Mod.audiohelper.Entities;

// [CustomEntity("audiohelper/SuppressCassetteDeactivationController")]
// [Tracked]

// public class SuppressCassetteDeactivationController : Entity {
//     public SuppressCassetteDeactivationController(EntityData data, Vector2 offset) : base(data.Position + offset) {}

//     public static bool IsActive(Scene scene) {
//         return scene.Tracker.GetEntity<SuppressCassetteDeactivationController>() is not null;
//     }

//     public override void Awake(Scene scene)
//     {
//         base.Awake(scene);
//     }

//     public override void Removed(Scene scene)
//     {
//         base.Removed(scene);
//     }
// }