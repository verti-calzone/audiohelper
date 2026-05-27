using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using Celeste.Mod.Entities;
using FMOD.Studio;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.audiohelper.Entities;

[CustomEntity("audiohelper/BusMuter")]
[Tracked]
public class BusMuter : Entity {

    public string Flag;
    public string Bus;

    private bool PrevFlag;

    public BusMuter(EntityData data, Vector2 offset){
        Bus = data.Attr("Bus");
        Flag = data.Attr("Flag");
	}

    public override void Added(Scene scene)
    {
        base.Added(scene);
        if(SceneAs<Level>().Session.GetFlag(Flag)) Audio.BusMuted(Bus, true);
        //Logger.Info("audiohelper","Muting bus named "+Bus);
    }
    public override void Update()
    {
        base.Update();
        if(SceneAs<Level>().Session.GetFlag(Flag) && !PrevFlag) {
            Audio.BusMuted(Bus, true);
            //Logger.Info("audiohelper","Muting bus named "+Bus);
        }
        else if(!SceneAs<Level>().Session.GetFlag(Flag) && PrevFlag) Audio.BusMuted(Bus, false);
        PrevFlag = SceneAs<Level>().Session.GetFlag(Flag);
    }

    public override void Removed(Scene scene)
    {
        base.Removed(scene);
        Audio.BusMuted(Bus, false);
    }
    public override void SceneEnd(Scene scene)
    {
        base.SceneEnd(scene);
        Audio.BusMuted(Bus, false);
    }
}