using System;
using System.Collections.Generic;
using Celeste.Mod.Entities;
using Celeste.Mod.Helpers;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.Cil;

namespace Celeste.Mod.audiohelper.Entities;

[CustomEntity("audiohelper/CassetteColourContinuityController")]
[Tracked]
public class CassetteColourContinuityController : Entity {
    public CassetteColourContinuityController(EntityData data, Vector2 offset) { }

    public static void OnOnLevelStart(On.Celeste.CassetteBlockManager.orig_OnLevelStart orig, CassetteBlockManager cbm)
    {
        if (cbm.SceneAs<Level>().Tracker.GetEntity<CassetteColourContinuityController>() is null)
        {
            orig(cbm);
            return;
        }
        
        cbm.SilentUpdateBlocks();
        return;
    }
}