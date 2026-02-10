// using Celeste.Mod.Entities;
// using Microsoft.Xna.Framework;

// namespace Celeste.Mod.audiohelper.Triggers;

// [CustomEntity("audiohelper/ReenableCassetteBlocks")]
// public class ReenableCassetteBlocks : Trigger {
//     public ReenableCassetteBlocks(EntityData data, Vector2 offset) : base(data, offset) {
//         OnlyOnce = data.Bool("onlyOnce", defaultValue: false);
//     }

//     [MethodImpl(MethodImplOptions.NoInlining)]
//     public override void OnEnter(Player player) {

//         // CassetteBlockManager cbm = Scene.Tracker.GetEntity<CassetteBlockManager>();
//         //cbm?.StopBlocks();

//         if onlyOnce == true {
//             RemoveSelf();
//         }
//     }
// }