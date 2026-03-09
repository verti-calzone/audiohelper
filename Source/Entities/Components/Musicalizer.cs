using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System.Runtime.CompilerServices;
using FMOD.Studio;

namespace Celeste.Mod.audiohelper.Entities;

[CustomEntity("audiohelper/Musicalizer")]
[Tracked(false)]

public class Musicalizer : Component {
	private float OldParameter = -1f;
  	public Musicalizer() : base(active: true, visible: false) {}

	public void SetParameter(string Parameter, float ParameterValue, bool IncMode)
	{
		if (IncMode)
		{
			Audio.CurrentMusicEventInstance.getParameterValue(Parameter, out OldParameter, out _);
    		Audio.SetMusicParam(Parameter,OldParameter + ParameterValue);
		}
		else
		{
			Audio.CurrentMusicEventInstance.getParameterValue(Parameter, out OldParameter, out _);
    		Audio.SetMusicParam(Parameter,ParameterValue);
		}
	}
	public void ResetParameter(string Parameter, float ParameterValue, bool IncMode)
	{
		if (IncMode)
		{
			Audio.CurrentMusicEventInstance.getParameterValue(Parameter, out OldParameter, out _);
			Audio.SetMusicParam(Parameter,OldParameter - ParameterValue);
		}
		else
		{
			Audio.SetMusicParam(Parameter, OldParameter);
		}
	}
}