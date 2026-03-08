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
public class SimpleAudioReplacer : Entity {
    public bool Global;
    public string OldEvent;
    public string NewEvent;

    [SpeedrunToolIop.Static]
    public static Dictionary<string,SimpleAudioReplacer> EventPairs = new();

    public SimpleAudioReplacer(EntityData data, Vector2 offset){
        OldEvent = data.Attr("OldEvent");
        NewEvent = data.Attr("NewEvent");
	}
    public static EventDescription OnGetEventDescription(On.Celeste.Audio.orig_GetEventDescription orig, String path)
    {
        if(EventPairs.TryGetValue(path,out var replacer)) return orig(replacer.NewEvent);
        return orig(path);
    }

    public override void Added(Scene scene)
    {
        EventPairs[OldEvent] = this;
        base.Added(scene);
    }

    public override void Removed(Scene scene)
    {
        if(EventPairs.TryGetValue(OldEvent,out var replacer) && replacer == this) EventPairs.Remove(OldEvent);
        base.Removed(scene);
    }

    public override void SceneEnd(Scene scene)
    {
        EventPairs.Remove(OldEvent);
        base.SceneEnd(scene);
    }
}