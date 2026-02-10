// using Celeste.Mod.Entities;
// using Microsoft.Xna.Framework;
// using Monocle;

// namespace Celeste.Mod.audiohelper.Entities;

// [CustomEntity("audiohelper/AudioReplacer")]
// public class AudioReplacer : Entity {
//     public bool MapWide;
//     string OldEvent = null;
//     string NewEvent = null;
//     string OldParam1 = null;
//     public float OldParam1Val;
//     string OldParam2 = null;
//     public float OldParam2Val;
//     string NewParam1 = null;
//     public float NewParam1Val;
//     string NewParam2 = null;
//     public float NewParam2Val;
//     public AudioReplacer(EntityData data, Vector2 offset){
//         MapWide = data.Bool("Mapwide",false);
//         OldParam1Val = data.Float("Oldparam1val", 0f)
//         OldParam2Val = data.Float("Oldparam2val", 0f)
//         NewParam1Val = data.Float("Newparam1val", 0f)
//         NewParam2Val = data.Float("Newparam2val", 0f)
//     }
//     public override void Added(Scene scene){
//         base.Added(scene);
//         private static void AudioHelperHookCreateInstance(On.Celeste.Audio.orig_CreateInstance orig, Audio self){
//             if self.path != OldEvent{
//                 orig(self);
//             } elif OldParam1 != null{
                
//             }

//         }

//     }
// }