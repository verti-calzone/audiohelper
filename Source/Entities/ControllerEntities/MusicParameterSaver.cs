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


}