// TODO: Hook ExtVar to allow NoFreezeFramesAdvanceCassetteBlocks to actually work
//       Fix nonstandard timescales
//       Play during room transitions, passing Note and Song to the next manager?s

using MonoMod.RuntimeDetour;
using Microsoft.Xna.Framework;
using System.Runtime.CompilerServices;
using Celeste.Mod.Entities;
using Celeste.Mod.Meta;
using FMOD.Studio;
using Monocle;
using System.Collections.Generic;
using System.Data.Common;
using System;

namespace Celeste.Mod.audiohelper.Entities;

[CustomEntity("audiohelper/CustomCassetteBlockManager")]
[Tracked]

public class CustomCassetteBlockManager : Entity {
    // data variables
    public float Tempo;
    public int CountInLength;
    public int LoopStart;
    public int LoopEnd;
    public int NotesPerTick;
    public int TicksPerSwap;
    public int StartingColour;
    public int MaxBlocks = 0;
    public string SongParam;

    public string SongName;
    public string TickSound;
    public string SwapSound;

    public bool UsesFlag;
    public string FlagName;
    public bool FreezeMode;
    public bool LatencyFix;
    public int NoteOffset;

    // internal variables
    public EventInstance Song;
    public static EventInstance Snapshot;
    public static float NoteTimer;
    public static int Note = 0;
    public static int ActiveBlock;
    public EntityID id;
    public bool PrevFlagState = false;
    public static bool CountingIn = true;
    public bool CanStart = false;

  public CustomCassetteBlockManager(EntityData data, Vector2 offset, EntityID id) : base(data.Position + offset)
    {
        // Global tag allows the manager to stay active across respawn
        base.Tag = Tags.Global;
        Add(new TransitionListener
        {
            OnOutBegin = this.RemoveSelf,
        });

        // Data set in map editor
        Tempo = data.Float("Tempo");
        CountInLength = data.Int("CountInLength");
        LoopStart = data.Int("LoopStart");
        LoopEnd = data.Int("LoopEnd");
        NotesPerTick = data.Int("NotesPerTick");
        TicksPerSwap = data.Int("TicksPerSwap");
        StartingColour = data.Int("StartingColour");
        MaxBlocks = data.Int("NumberOfBlocks");
        NoteOffset = data.Int("NoteOffset");

        SongName = data.Attr("CassetteSong");
        TickSound = data.Attr("TickSound");
        SwapSound = data.Attr("SwapSound");

        SongParam = data.Attr("MusicParameter");
        UsesFlag = data.Bool("UsesFlag");
        FlagName = data.Attr("Flag");
        FreezeMode = data.Bool("FreezeMode");

        this.id = id;
    }
    public static bool IsActive(Scene scene)
    {
        return scene.Tracker.GetEntity<CustomCassetteBlockManager>() is not null;
    }
    public override void Added(Scene scene)
    {
        base.Added(scene);
        // Removes self if already present
        foreach(CustomCassetteBlockManager ccbm in scene.Entities.FindAll<CustomCassetteBlockManager>()) if(ccbm != this && ccbm.id.Key == id.Key) RemoveSelf();
    }
    public override void Awake(Scene scene)
    {
        base.Awake(scene);
        
        // remove the vanilla manager. further vanilla managers will be made on reload, but an ILHook prevents those from being created as long as a custom manager exists
        scene.Entities.FindFirst<CassetteBlockManager>()?.RemoveSelf();

        // Setting up the EventInstance
        if(!string.IsNullOrEmpty(SongName)) Song = Audio.CreateInstance(SongName);


        if (!CountingIn) CanStart = true;
        foreach(CustomCassetteBlockManager ccbm in scene.Entities.FindAll<CustomCassetteBlockManager>())
        {
            if(ccbm.CountInLength == 0 && !(UsesFlag && !SceneAs<Level>().Session.GetFlag(ccbm.FlagName)))
            {
                CountingIn = false;
                CanStart = true;
            }
        }

        // Continues running Update whilst transitioning into its room
        TransitionListener transitionListener = Get<TransitionListener>();
        if (transitionListener == null) return;
        transitionListener.OnIn = delegate
        {
            if (base.Scene != null && !CountingIn) Update();
        };

        // Adds a starter entity to call OnlevelStart 
        if(scene.Tracker.GetEntity<CustomCassetteBlockManagerStarter>() is null) scene.Add(new CustomCassetteBlockManagerStarter());
    }
    public void OnLevelStart()
    {
        // If you respawn *more* than halfway until the next swap, then set the active block to be *three* swaps before the starting colour
        if (Note % (NotesPerTick * TicksPerSwap) > NotesPerTick * TicksPerSwap / 2) ActiveBlock = (StartingColour - 3 + MaxBlocks) % MaxBlocks;
        // If you respawn *less* than halfway until the next swap, then set the active block to be *two* swaps before the starting colour
        else ActiveBlock = (StartingColour - 2 + MaxBlocks) % MaxBlocks;
        if((UsesFlag && SceneAs<Level>().Session.GetFlag(FlagName)) || !UsesFlag) SilentUpdateBlocks();
    }
    private void SilentUpdateBlocks()
    {
        Logger.Info("audiohelper","running silentupdate"+this.id.Key);        
        foreach (CassetteBlock entity in base.Scene.Tracker.GetEntities<CassetteBlock>())
        {
            if (entity.ID.Level == SceneAs<Level>().Session.Level) entity.SetActivatedSilently(entity.Index == ActiveBlock);
        }
        foreach (CassetteListener component in base.Scene.Tracker.GetComponents<CassetteListener>())
        {
            if (component.ID.ID == EntityID.None.ID || component.ID.Level == SceneAs<Level>().Session.Level) component.Start(component.Index == ActiveBlock);
        }
    }
    public override void Update()
    {
        base.Update();
        AdvanceMusic(Engine.DeltaTime);
        UpdateSnapshot();
        foreach(CustomCassetteBlockManager ccbm in Scene.Entities.FindAll<CustomCassetteBlockManager>())
        {
            // returns without disabling all blocks if an unactive manager uses freeze mode, or there exists an active manager
            if (ccbm.UsesFlag && !SceneAs<Level>().Session.GetFlag(ccbm.FlagName) && ccbm.FreezeMode || !(ccbm.UsesFlag && !SceneAs<Level>().Session.GetFlag(ccbm.FlagName))) return;
        }
        foreach (CassetteBlock entity in base.Scene.Tracker.GetEntities<CassetteBlock>()) if(entity.Activated && (Note + 1) % (NotesPerTick * TicksPerSwap) != 0) entity.WillToggle();
        foreach (CassetteBlock entity in base.Scene.Tracker.GetEntities<CassetteBlock>()) entity.Activated = entity.Index == -1;
        foreach (CassetteListener component in base.Scene.Tracker.GetComponents<CassetteListener>()) component.SetActivated(false);
    }

    public void AdvanceMusic(float time)
    {
        if((UsesFlag && SceneAs<Level>().Session.GetFlag(FlagName)) || !UsesFlag){
        
            // logic for the timer between each note
            NoteTimer += time;
            if (NoteTimer < 60f/(Tempo*NotesPerTick)) return;
            NoteTimer -= 60f/(Tempo*NotesPerTick);
            Note++;
            if(Note>LoopEnd) Note = LoopStart;

            // Counts down the count-in value to create the intro, where the song doesnt play yet
            if (CountingIn)
            {
                CountInLength--;
                if (CountInLength <= NoteOffset)
                {
                    Note = LoopEnd+1-NoteOffset;
                    foreach(CustomCassetteBlockManager ccbm in Scene.Entities.FindAll<CustomCassetteBlockManager>()) ccbm.CanStart = true;
                    CountingIn = false;
                    //Audio.Play(SwapSound);
                }
            }
            // logic for each "swap"
            if (Note % (NotesPerTick*TicksPerSwap) == 0)
            {
                ActiveBlock++;
                ActiveBlock %= MaxBlocks;
                SetActiveIndex(ActiveBlock);
                if (!string.IsNullOrEmpty(SwapSound)) Audio.Play(SwapSound);
                Input.Rumble(RumbleStrength.Medium, RumbleLength.Short);
            }
            else
            {
                // if one note before a swap, warn the blocks that are about to swap
                if ((Note + 1) % (NotesPerTick * TicksPerSwap) == 0) SetWillActivate((ActiveBlock + 1) % MaxBlocks);
                // if a tick (but not a swap!), play the sound
                if (Note % NotesPerTick == 0 && !string.IsNullOrEmpty(TickSound)) Audio.Play(TickSound);
            }
            
            // Sets the music param to match the counter once the count-in ends
            if (!CountingIn) PlayNote(Note);
            // Begins the song on the first Update where the count-in is finished/skipped, and the flag is set (if used)
            if (CanStart)
            {
                CanStart = false;
                Song?.start();
            }
        }
    }
    public void PlayNote(int note)
    {
        note -= LoopStart;
        note += NoteOffset;
        note %= LoopEnd-LoopStart+1;
        note += LoopStart;
        (!string.IsNullOrEmpty(SongName)? Song : Audio.CurrentMusicEventInstance)?.setParameterValue(SongParam, note+1);
    }
    public void SetActiveIndex(int index)
    {
        // Controls which blocks are currently active
        foreach (CassetteBlock entity in base.Scene.Tracker.GetEntities<CassetteBlock>()) entity.Activated = entity.Index == index;
        foreach (CassetteListener component in base.Scene.Tracker.GetComponents<CassetteListener>()) component.SetActivated(component.Index == index);
    }
    public void SetWillActivate(int index)
    {
        // Controls which blocks will (de)activate on the next note (used for the bobbing animation)
        foreach (CassetteBlock entity in base.Scene.Tracker.GetEntities<CassetteBlock>()) if (entity.Index == index || entity.Activated) entity.WillToggle();
        foreach (CassetteListener component in base.Scene.Tracker.GetComponents<CassetteListener>()) if (component.Index == index || component.Activated) component.WillToggle();
    }
    public void UpdateSnapshot()
    {
        // try to start the snapshot 
        if (!Audio.IsSnapshotRunning(Snapshot))
        {
            foreach(CustomCassetteBlockManager ccbm in Scene.Entities.FindAll<CustomCassetteBlockManager>())
            {
                if(!(ccbm.UsesFlag && !SceneAs<Level>().Session.GetFlag(ccbm.FlagName)) && !string.IsNullOrEmpty(ccbm.SongName))
                {
                    Snapshot = Audio.CreateSnapshot("snapshot:/music_mains_mute");
                    return;
                }
            }
            return;
        }
        //try to stop the snapshot
        else
        {
            foreach(CustomCassetteBlockManager ccbm in Scene.Entities.FindAll<CustomCassetteBlockManager>())
            {
                if(!(ccbm.UsesFlag && !SceneAs<Level>().Session.GetFlag(ccbm.FlagName)) && !string.IsNullOrEmpty(ccbm.SongName))
                {
                    return;
                }
            }
            Audio.Stop(Snapshot);
            return;
        }

    }
    public override void SceneEnd(Scene scene)
    {
        base.SceneEnd(scene);
        if (!string.IsNullOrEmpty(SongName))
        {
            Audio.Stop(Snapshot);
            Audio.Stop(Song);
            CountingIn = true;
            Note = 0;
        }
    }
    public override void Removed(Scene scene)
    {
        base.Removed(scene);
        if (!string.IsNullOrEmpty(SongName)) Audio.Stop(Song);
        if(scene.Entities.AmountOf<CustomCassetteBlockManager>() == 0)
        {
            Logger.Info("audiohelper","resetting variables");
            CountingIn = true;
            Note = 0;
            Audio.Stop(Snapshot);
        }
    }
}