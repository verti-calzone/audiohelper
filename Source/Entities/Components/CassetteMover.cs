using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System.Runtime.CompilerServices;
using FMOD.Studio;

namespace Celeste.Mod.audiohelper.Entities;

[CustomEntity("audiohelper/CassetteMover")]
[Tracked(false)]

public class CassetteMover : Component {
	public Vector2[] Verticies;
  	public CassetteMover() : base(active: true, visible: false)
	{
		
	}

	
}
