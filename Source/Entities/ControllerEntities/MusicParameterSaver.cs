using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using Celeste.Mod.Entities;
using FMOD.Studio;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.audiohelper.Entities;

[CustomEntity("audiohelper/MusicParameterSaver")]
[Tracked]

public class MusicParameterSaver : Entity {

    public MusicParameterSaver(EntityData data, Vector2 offset){}

    public override void Added(Scene scene)
    {
        base.Added(scene);
    }
    public override void Update()
    {
        base.Update();
    }

    public override void Removed(Scene scene)
    {
        base.Removed(scene);
    }
    public override void SceneEnd(Scene scene)
    {
        base.SceneEnd(scene);
    }
}