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
	private bool Musicalizing = false;
  	public Musicalizer() : base(active: true, visible: false) {}

	public void SetParameter(string Parameter, float ParameterValue, bool IncMode, int Mode)
	{
		if(!Musicalizing)
		{
			switch (Mode)
			{
				case 0: //legacy case, uses IncMode
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
					break;
				case 1: // Resets to "Get" value
					Audio.CurrentMusicEventInstance.getParameterValue(Parameter, out OldParameter, out _);
					Audio.SetMusicParam(Parameter,ParameterValue);
					break;
				case 2: // Resets to the reset value
					Audio.SetMusicParam(Parameter,ParameterValue);
					break;
				case 3: // Increment Mode
					Audio.CurrentMusicEventInstance.getParameterValue(Parameter, out OldParameter, out _);
					Audio.SetMusicParam(Parameter,OldParameter + ParameterValue);
					break;
			}
		}
		Musicalizing = true;
	}
	public void ResetParameter(string Parameter, float ParameterValue, bool IncMode, int Mode, float ResetParameterValue)
	{
		switch (Mode)
		{
			case 0: //legacy case, uses IncMode
				if (IncMode)
				{
					Audio.CurrentMusicEventInstance.getParameterValue(Parameter, out OldParameter, out _);
					Audio.SetMusicParam(Parameter,OldParameter - ParameterValue);
					//Logger.Info("audiohelper","resetting legacy increment mode");
				}
				else
				{
					Audio.SetMusicParam(Parameter, OldParameter);
					//Logger.Info("audiohelper","resetting legacy get mode");
				}
				break;
			case 1: // Resets to "Get" value
				Audio.SetMusicParam(Parameter, OldParameter);
				//Logger.Info("audiohelper","resetting get mode");
				break;
			case 2: // Resets to the reset value
				Audio.CurrentMusicEventInstance.getParameterValue(Parameter, out OldParameter, out _);
				Audio.SetMusicParam(Parameter,ResetParameterValue);
				//Logger.Info("audiohelper","resetting reset mode");
				break;
			case 3: // Increment Mode
				Audio.CurrentMusicEventInstance.getParameterValue(Parameter, out OldParameter, out _);
				Audio.SetMusicParam(Parameter,OldParameter - ParameterValue);
				//Logger.Info("audiohelper","resetting increment mode");
				break;
		}
		Musicalizing = false;
	}
}