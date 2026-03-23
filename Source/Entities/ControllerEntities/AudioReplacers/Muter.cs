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
public class Muter : SimpleAudioReplacer {

    public Muter(EntityData data, Vector2 offset) : base(data,offset){
        OldEvent = data.Attr("EventToMute");
        NewEvent = "event:/none";
	}

    public override void Added(Scene scene)
    {
        EventPairs[OldEvent] = this;
        base.Added(scene);
    }
    public override void Update()
    {
        base.Update();
    }
}