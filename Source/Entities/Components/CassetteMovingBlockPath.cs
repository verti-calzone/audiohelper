using System;
using System.Collections.Generic;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;

namespace Celeste.Mod.audiohelper.Entities;

[CustomEntity("audiohelper/CassetteMovingBlockPath")]
[Tracked]

public class CassetteMovingBlockPath : Entity
{
	public Vector2[] pathVertices;
	public Vector2 positionOffset;
    public List<CassetteMovingBlockPathWire> wireList = [];
	public int length;
	public float spinRate;


	public List<Sprite> spriteList = [];
	public CassetteMovingBlock cmb;

    public bool spinning;
    public static ParticleType P_Sparks;

    public CassetteMovingBlockPath(CassetteMovingBlock block) : base()
	{
		cmb = block;
		pathVertices = cmb.mover.vertices;
		positionOffset = cmb.Center - cmb.Position;
		length = pathVertices.Length;

        Depth = Depths.BGDecals - 1;



        // create wires
        for (int i = 0; i < length; i++)
        {
            bool makeNewWire = true;
            int i1 = (i+1) % length;

            for (int j = 0; j < i; j++)
            {
                if ((pathVertices[i] == pathVertices[j] && pathVertices[i1] == pathVertices[j+1]) || (pathVertices[i] == pathVertices[j+1] && pathVertices[i1] == pathVertices[j]))
                {
                    makeNewWire = false;
                }
            }
            if (makeNewWire && pathVertices[i] != pathVertices[i1])
            {
                CassetteMovingBlockPathWire wire = new(pathVertices[i] + positionOffset, pathVertices[i1] + positionOffset, cmb.bigSprite ? 7.5f : 4);
                Add(wire);
                wireList.Add(wire);
            }
        }

        // create sprites
        for (int i = 0; i < length; i++)
        {
            bool makeNewSprite = true;
            for (int j = 0; j < i; j++)
            {
                if (pathVertices[i] == pathVertices[j]) makeNewSprite = false;
            }
            if (makeNewSprite)
            {
                Sprite sprite = GFX.SpriteBank.Create("cassettemovingblock_gear_" + (cmb.bigSprite ? "big_" : "small_") + cmb.texture);
                Add(sprite);
                sprite.Position = pathVertices[i] + positionOffset;
                sprite.Rate = 0f;
                spriteList.Add(sprite);
            }
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
        if (spinning)
        {
            Level level = SceneAs<Level>();
            float angle = -0.25f;
            if (Scene.OnInterval(cmb.bigSprite ? 0.2f : 0.3f))
            {
                foreach (CassetteMovingBlockPathWire wire in wireList)
                {
                    level.ParticlesBG.Emit(CassetteMovingBlockPath.P_Sparks, 2, wire.curve1.Begin, Vector2.One, wire.vector.Angle() + MathF.PI + angle);
                    level.ParticlesBG.Emit(CassetteMovingBlockPath.P_Sparks, 2, wire.curve2.Begin, Vector2.One, wire.vector.Angle() + angle);
                }
            }
            else if(Scene.OnInterval(cmb.bigSprite ? 0.1f : 0.15f))
            {
                foreach (CassetteMovingBlockPathWire wire in wireList)
                {
                    level.ParticlesBG.Emit(CassetteMovingBlockPath.P_Sparks, 2, wire.curve1.End, Vector2.One, wire.vector.Angle() + MathF.PI + angle);
                    level.ParticlesBG.Emit(CassetteMovingBlockPath.P_Sparks, 2, wire.curve2.End, Vector2.One, wire.vector.Angle() + angle);
                }
            }
        }

        foreach (Sprite sprite in spriteList) sprite.Rate = spinRate;

        base.Update();
	}
    public override void Render()
    {
        foreach(Sprite sprite in spriteList) sprite.Render();
        base.Render();
    }
	public void Start()
	{
		foreach (CassetteMovingBlockPathWire wire in wireList) wire.Tighten();
        spinning = true;
    }
	public void Stop()
	{
        foreach (CassetteMovingBlockPathWire wire in wireList) wire.Loosen();
        spinning = false;
    }

    // HOOK //

    public static void OnStopBlocks(On.Celeste.CassetteBlockManager.orig_StopBlocks orig, CassetteBlockManager cbm)
    {
        orig(cbm);
        foreach (CassetteMovingBlockPath cmbp in cbm.Scene.Tracker.GetEntities<CassetteMovingBlockPath>()) cmbp.spinning = false;
    }
}