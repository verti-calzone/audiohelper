using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using Celeste.Mod.Entities;
using FMOD.Studio;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.audiohelper.Entities;

[CustomEntity("audiohelper/SimpleAudioReplacer")]
[Tracked]
public class SimpleAudioReplacer : AdvancedAudioReplacer {

    public SimpleAudioReplacer(EntityData data, Vector2 offset) : base(data,offset){
        OldEvent = data.Attr("OldEvent");
        NewEvent = data.Attr("NewEvent");
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