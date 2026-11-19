using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.audiohelper.Entities;

[CustomEntity("audiohelper/OmniSoundSource")]
[Tracked]
public class OmniSoundSource : SoundSourceEntity {

    public string flag;
    public bool usesFlag, oneShotMode, playing = false;
    public float minDelay, maxDelay, delay;

    public OmniSoundSource(EntityData data, Vector2 offset) : base(data, offset) {
        flag = data.Attr("Flag");
        usesFlag = data.Bool("UsesFlag");
        oneShotMode = data.Bool("OneShotMode");

        minDelay = data.Float("OneShotMinimumDelay");
        maxDelay = data.Float("OneShotMaximumDelay");

        if (data.Bool("AttachToSolid")) Add(new StaticMover());
	}

    public override void Awake(Scene scene)
    {
        base.Awake(scene);
        if (usesFlag && !SceneAs<Level>().Session.GetFlag(flag)) return;
        Play();
    }

    public override void Update()
    {
        base.Update();
        if (!playing && SceneAs<Level>().Session.GetFlag(flag))
        {
            Play();
            return;
        }

        if (oneShotMode && Scene.OnInterval(delay)) Play();
    }

    public void Play()
    {
        playing = true;
        sfx.Play(eventName);
        if (oneShotMode) delay = Calc.Random.Range(minDelay, maxDelay);
    }
}