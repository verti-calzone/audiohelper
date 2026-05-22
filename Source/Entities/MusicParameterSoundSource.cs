using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System.Runtime.CompilerServices;
using System;

namespace Celeste.Mod.audiohelper.Entities;

[CustomEntity("audiohelper/MusicParameterSoundSource")]
[Tracked]

public class MusicParameterSoundSource : Entity {
    public string Param;
    public float EdgeValue;
    public float CentreValue;
    public float Radius;
    private float Distance;
    private float SetValue;
    public MusicParameterSoundSource(EntityData data, Vector2 offset) : base(data.Position + offset)
    {
        Param = data.Attr("MusicParameter");
        EdgeValue = data.Float("EdgeValue");
        CentreValue = data.Float("CentreValue");
        Radius = data.Float("Radius");
        Collider = new Circle(Radius*8);
        Add(new PlayerCollider(OnPlayer));
        Collider = new Hitbox(4,4,-2,-2);
    }

    private void OnPlayer(Player player)
    {
        Distance = (this.Position-player.Position).Length();
        SetValue = CentreValue-((CentreValue-EdgeValue)*Distance/((Radius*8)-0.5f));
        if(Distance > (Radius*8) - 0.5f) SetValue = EdgeValue;
        Audio.SetMusicParam(Param,SetValue);
        //Logger.Info("audiohelper","OnPlayer triggered with distance: "+Distance+" and parameter set to "+SetValue);
    }
}