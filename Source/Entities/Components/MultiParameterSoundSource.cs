using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System.Runtime.CompilerServices;
using FMOD.Studio;

namespace Celeste.Mod.audiohelper.Entities;

[CustomEntity("audiohelper/MultiParameterSoundSource")]
[Tracked]

public class MultiParameterSoundSource : SoundSource {
  [MethodImpl(MethodImplOptions.NoInlining)]
	public SoundSource Play(string path, string param = null, float value = 0f, string param2 = null, float value2 = 0f, bool replace = false)
	{
		if (replace)
		{
			Stop();
		}
		EventName = path;
		EventDescription eventDescription = Audio.GetEventDescription(path);
		if (eventDescription != null)
		{
			eventDescription.createInstance(out instance);
			eventDescription.is3D(out is3D);
			eventDescription.isOneshot(out isOneshot);
		}
		if (instance != null)
		{
			if (is3D)
			{
				Vector2 position = Position;
				if (base.Entity != null)
				{
					position += base.Entity.Position;
				}
				Audio.Position(instance, position);
			}
			if (param != null)
			{
				instance.setParameterValue(param, value);
			}
			if (param2 != null)
			{
				instance.setParameterValue(param2, value2);
			}
			instance.start();
			Playing = true;
		}
		return this;
	}
}