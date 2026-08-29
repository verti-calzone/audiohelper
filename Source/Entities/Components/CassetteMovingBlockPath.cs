using System;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.audiohelper.Entities;

[CustomEntity("audiohelper/CassetteMovingBlockPath")]
[Tracked]

public class CassetteMovingBlockPath : Entity
{
	public Vector2[] pathVertices, pathVectors, pathPerpendicularNormals;
	public Vector2 positionOffset;
	public CassetteMovingBlockPathWire[] wires1, wires2;
	public int length;
	public float shift, waitTime;
    public float wireSag = 4f;

	public Sprite[] sprites;
	public CassetteMovingBlock cmb;

    public bool sparking;
    public static ParticleType P_Sparks;

    public CassetteMovingBlockPath(CassetteMovingBlock block, float time) : base()
	{
		cmb = block;
        waitTime = time;
		pathVertices = cmb.mover.vertices;
		positionOffset = new Vector2(cmb.Width / 2, cmb.Height / 2);
		length = pathVertices.Length;
		pathVectors = new Vector2[length];
        pathPerpendicularNormals = new Vector2[length];
        sprites = new Sprite[length];
		shift = cmb.bigSprite ? 8 : 4;

        Depth = Depths.BGDecals - 1;

        for (int i = 0; i < length; i++)
		{
			pathVectors[i] = pathVertices[(i+1) % length] - pathVertices[i];
			pathPerpendicularNormals[i] = pathVectors[i].Perpendicular().SafeNormalize();
        }
		int numWires;
        if (length > 2)
        {
			wires1 = new CassetteMovingBlockPathWire[length];
            wires2 = new CassetteMovingBlockPathWire[length];
			numWires = length;
        }
        else
        {
            wires1 = new CassetteMovingBlockPathWire[1];
            wires2 = new CassetteMovingBlockPathWire[1];
			numWires = 1;
        }
        for (int i = 0; i < numWires; i++)
        {
			Add(wires1[i] = new CassetteMovingBlockPathWire(pathVertices[i] + positionOffset + pathPerpendicularNormals[i] * shift, pathVertices[(i + 1) % length] + positionOffset + pathPerpendicularNormals[i] * shift, wireSag));
            Add(wires2[i] = new CassetteMovingBlockPathWire(pathVertices[i] + positionOffset - pathPerpendicularNormals[i] * shift, pathVertices[(i + 1) % length] + positionOffset - pathPerpendicularNormals[i] * shift, wireSag));
        }
        for (int i = 0; i < length; i++)
        {
            Add(sprites[i] = GFX.SpriteBank.Create(cmb.bigSprite ? "audiohelper_cassette_endpoint_big" : "audiohelper_cassette_endpoint"));
            sprites[i].Rate = 0f;
            sprites[i].Color = Color.Gray;
        }

        P_Sparks = new ParticleType
        {
            Color = Color.White,
            LifeMin = 0.15f,
            LifeMax = 0.25f,
            Size = 1f,
            DirectionRange = 0.5f,
            SpeedMin = 30f,
            SpeedMax = 40f,
            UseActualDeltaTime = true
        };
    }
	public override void Update()
	{
        if (sparking && Scene.OnInterval(0.1f))
        {
            for (int i = 0; i < length; i++)
            {
                SceneAs<Level>().ParticlesBG.Emit(CassetteMovingBlockPath.P_Sparks, 2, pathVertices[i] + positionOffset + pathPerpendicularNormals[i] * (shift + 1), Vector2.One, pathVectors[i].Angle() + MathF.PI - 0.5f);
                SceneAs<Level>().ParticlesBG.Emit(CassetteMovingBlockPath.P_Sparks, 2, pathVertices[i] + positionOffset - pathPerpendicularNormals[i] * (shift + 1), Vector2.One, pathVectors[i].Angle() - 0.5f);

            }
        }
        base.Update();
	}
    public override void Render()
    {
		for (int i = 0; i < length; i++)
		{
			sprites[i].Position = pathVertices[i] + positionOffset + Vector2.One*1000;
        }
        base.Render();
    }
	public void Start()
	{
		foreach (Sprite endpoint in sprites) endpoint.Rate = 1f;
		foreach (CassetteMovingBlockPathWire cmbpw in wires1) cmbpw.Tighten(0.5f);
        foreach (CassetteMovingBlockPathWire cmbpw in wires2) cmbpw.Tighten(0.5f);
        sparking = true;
    }
	public void Stop()
	{
        foreach (Sprite endpoint in sprites) endpoint.Rate = 0f;
        foreach (CassetteMovingBlockPathWire cmbpw in wires1) cmbpw.Loosen(1f);
        foreach (CassetteMovingBlockPathWire cmbpw in wires2) cmbpw.Loosen(1f);
        sparking = false;
    }
}