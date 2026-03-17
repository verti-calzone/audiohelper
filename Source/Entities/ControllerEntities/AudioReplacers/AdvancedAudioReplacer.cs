using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using Celeste.Mod.Entities;
using FMOD.Studio;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.audiohelper.Entities;

[CustomEntity("audiohelper/AdvancedAudioReplacer")]
[Tracked]
public class AdvancedAudioReplacer : Entity {

    public string OldEvent;
    public string NewEvent;
    public string FlagName;
    public bool OldFlag;
    public float ResetTimer = 0f;
    public Musicalizer Musicalizer;
    public float ResetTime;
    public string MusicParam;
    public float ParamValue;
    public bool IncMode = false;
    public bool CanReset = false;
    [SpeedrunToolIop.Static]
    public static Dictionary<string,AdvancedAudioReplacer> EventPairs = new();
    

    public AdvancedAudioReplacer(EntityData data, Vector2 offset) {
        OldEvent = data.Attr("OldEvent");
        NewEvent = data.Attr("NewEvent");
        FlagName = data.Attr("Flag");
        ResetTime = data.Float("ResetTime");
        MusicParam = data.Attr("MusicParameter");
        ParamValue = data.Float("ParameterValue");
        IncMode = data.Bool("IncrementMode");
        if(!string.IsNullOrEmpty(MusicParam)) Musicalizer = new Musicalizer();
	}

    public override void Added(Scene scene)
    {
        base.Added(scene);
        if (SceneAs<Level>().Session.GetFlag(FlagName) || string.IsNullOrEmpty(FlagName)) EventPairs[OldEvent] = this;
        OldFlag = SceneAs<Level>().Session.GetFlag(FlagName);
    }

    public override void Update()
    {
        base.Update();
        if(!string.IsNullOrEmpty(FlagName))
        {
            if(SceneAs<Level>().Session.GetFlag(FlagName) == true && OldFlag == false) EventPairs[OldEvent] = this;
            else if(SceneAs<Level>().Session.GetFlag(FlagName) == false && OldFlag == true) if(EventPairs.TryGetValue(OldEvent,out var replacer) && replacer == this) EventPairs.Remove(OldEvent);
        OldFlag = SceneAs<Level>().Session.GetFlag(FlagName);
        }
        if (ResetTimer > 0)
        {
            ResetTimer -= Engine.DeltaTime;
            CanReset = true;
        }
        else if(CanReset)
        {
            Musicalizer.ResetParameter(MusicParam,ParamValue,IncMode);
            CanReset = false;
        }
    }

    public static EventDescription OnGetEventDescription(On.Celeste.Audio.orig_GetEventDescription orig, string path)
    {
        if(EventPairs.TryGetValue(path,out var replacer))
        {
            if(replacer is not SimpleAudioReplacer && !string.IsNullOrEmpty(replacer.MusicParam))
            {
                replacer.ResetTimer = replacer.ResetTime;
                replacer.Musicalizer.SetParameter(replacer.MusicParam,replacer.ParamValue,replacer.IncMode);
            }
            return orig(replacer.NewEvent);
        }
        return orig(path);
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