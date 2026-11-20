using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using Celeste.Mod.Entities;
using FMOD.Studio;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.audiohelper.Entities;

[CustomEntity("audiohelper/Muter")]
[Tracked]
public class Muter : AdvancedAudioReplacer {

    public Muter(EntityData data, Vector2 offset) : base(data,offset){
        OldEvent = data.Attr("EventToMute");
        NewEvent = "event:/char/madeline/footstep";
        FlagName = data.Attr("Flag");
        usesFlag = data.Bool("UsesFlag", false);
    }
}