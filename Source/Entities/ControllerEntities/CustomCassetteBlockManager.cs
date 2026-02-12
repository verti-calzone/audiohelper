// TODO: Hook ExtVar to allow NoFreezeFramesAdvanceCassetteBlocks to actually work
//       Set up the flag logic

using MonoMod.RuntimeDetour;
using Microsoft.Xna.Framework;
using System.Runtime.CompilerServices;
using Celeste.Mod.Entities;
using Celeste.Mod.Meta;
using FMOD.Studio;
using Monocle;
using System.Collections.Generic;
using System.Data.Common;

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
    public bool Flag;
    public bool FreezeMode;
    public bool LatencyFix;
    public bool MuteMains;

    // implicit variables
    public EventInstance Song;    
    public EventInstance Snapshot;
    public float NoteTimer;
    public int Note;
    public int ActiveBlock;
    public EntityID id;

  public CustomCassetteBlockManager(EntityData data, Vector2 offset) : base(data.Position + offset)
    {
        // Global tag allows the manager to stay active across respawn
        base.Tag = Tags.Global;
        Add(new TransitionListener
        {
            OnOutBegin = RemoveSelf
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
        SongParam = data.Attr("MusicParameter");

        SongName = data.Attr("CassetteSong");
        TickSound = data.Attr("TickSound");
        SwapSound = data.Attr("SwapSound");

        UsesFlag = data.Bool("UsesFlag");
        FlagName = data.Attr("Flag");
        FreezeMode = data.Bool("FreezeMode");
        LatencyFix = data.Bool("LatencyFix");
    }
    public static bool IsActive(Scene scene)
    {
        return scene.Tracker.GetEntity<CustomCassetteBlockManager>() is not null;
    }
    public override void Added(Scene scene)
    {
        base.Added(scene);
        // Removes self if already present
        foreach(CustomCassetteBlockManager ccbm in scene.Entities.FindAll<CustomCassetteBlockManager>()) if(ccbm != this && ccbm.id.ID == this.id.ID) RemoveSelf();

        // Adds a starter entity to call OnlevelStart 
        if(scene.Tracker.GetEntity<CustomCassetteBlockManagerStarter>() is null) scene.Add(new CustomCassetteBlockManagerStarter());
    }
    public override void Awake(Scene scene)
    {
        base.Awake(scene);
        // remove the vanilla manager. further vanilla managers will be made on reload, but an ILHook prevents those from being created as long as a custom manager exists
        scene.Entities.FindFirst<CassetteBlockManager>()?.RemoveSelf();

        // sets the correct music eventinstance to the Song variable (TODO: copy to update to account for midroom music changes)
        if(string.IsNullOrEmpty(SongName)) Song = Audio.CurrentMusicEventInstance;
        else if(Song is null) Song = Audio.CreateInstance(SongName);

        // Mutes main music if not using it
        if (!string.IsNullOrEmpty(SongName)) Snapshot = Audio.CreateSnapshot("snapshot:/music_mains_mute");
    }
    public void OnLevelStart()
    {
        // If you respawn *more* than halfway until the next swap, then set the active block to be *three* swaps before the starting colour
        if (Note % (NotesPerTick * TicksPerSwap) > NotesPerTick * TicksPerSwap / 2) ActiveBlock = (StartingColour - 3 + MaxBlocks) % MaxBlocks;
        // If you respawsn *less* than halfway until the next swap, then set the active block to be *two* swaps before the starting colour
        else ActiveBlock = (StartingColour - 2 + MaxBlocks) % MaxBlocks;
        Logger.Info("audiohelper","activeblock is"+ActiveBlock);
        SilentUpdateBlocks();
    }
    private void SilentUpdateBlocks()
    {
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
        AdvanceMusic(Engine.DeltaTime*Tempo*NotesPerTick/(90f*4));
        if (UsesFlag && !SceneAs<Level>().Session.GetFlag(FlagName) && !FreezeMode)
        {
            foreach (CassetteBlock entity in base.Scene.Tracker.GetEntities<CassetteBlock>()) if(entity.Activated && (Note + 1) % (NotesPerTick * TicksPerSwap) != 0) entity.WillToggle();
            foreach (CassetteBlock entity in base.Scene.Tracker.GetEntities<CassetteBlock>()) entity.Activated = entity.Index == -1;
            foreach (CassetteListener component in base.Scene.Tracker.GetComponents<CassetteListener>()) component.SetActivated(false);
        }
    }

    public void AdvanceMusic(float time)
    {
        if((UsesFlag && SceneAs<Level>().Session.GetFlag(FlagName)) || !UsesFlag){
        
            // logic for the timer between each note
            NoteTimer += time;
            // if (NoteTimer < 60f/(Tempo*NotesPerTick)) return;
            // NoteTimer -= 60f/(Tempo*NotesPerTick);
            if (NoteTimer < 1f/6f) return;
            NoteTimer -= 1f/6f;
            Note++;
            if(Note>LoopEnd) Note = LoopStart;

            // logic for each "swap"
            if (Note % (NotesPerTick*TicksPerSwap) == 0)
            {
                ActiveBlock++;
                ActiveBlock %= MaxBlocks;
                SetActiveIndex(ActiveBlock);
                if (!string.IsNullOrEmpty(TickSound)) Audio.Play(TickSound);
                Input.Rumble(RumbleStrength.Medium, RumbleLength.Short);
            }
            else
            {
                // if one note before a swap, warn the blocks that are about to swap
                if ((Note + 1) % (NotesPerTick * TicksPerSwap) == 0)
                {
                    SetWillActivate((ActiveBlock + 1) % MaxBlocks);
                }
                // if a tick (but not a swap!), play the sound
                if (Note % NotesPerTick == 0 && !string.IsNullOrEmpty(SwapSound)) Audio.Play(SwapSound);
            }
            // Counts down the count-in value to create the intro, where the song doesnt play yet
            if (CountInLength > 0)
            {
                CountInLength--;
                // Starts the song a note early when using the latency fix, so that it sounds in time
                if (CountInLength == 1 && LatencyFix)
                {
                    Note = -1;
                    if (!string.IsNullOrEmpty(SongName)) Song?.start();
                }
                if (CountInLength == 0 && !LatencyFix)
                {
                    Note = 0;
                    if (!string.IsNullOrEmpty(SongName)) Song?.start();
                }
            }
            // Sets the music param to match the counter
            if (CountInLength <= 0)
            {
                // Cues every note to play one step early to account for latency
                if(LatencyFix)
                {
                    if(Note == LoopEnd) Song?.setParameterValue(SongParam, LoopStart+1);
                    else Song?.setParameterValue(SongParam, Note+2);
                }
                else Song?.setParameterValue(SongParam, Note+1);
            }
        }
    }
    public void SetActiveIndex(int index)
    {
        // Logger.Info("audiohelper","ran SetActiveIndex");
        // Controls which blocks are currently active
        foreach (CassetteBlock entity in base.Scene.Tracker.GetEntities<CassetteBlock>()) entity.Activated = entity.Index == index;
        foreach (CassetteListener component in base.Scene.Tracker.GetComponents<CassetteListener>()) component.SetActivated(component.Index == index);
    }
    public void SetWillActivate(int index)
    {
        // Logger.Info("audiohelper","ran SetWillActivate");
        // Controls which blocks will activate on the next note (used for the bobbing animation)
        foreach (CassetteBlock entity in base.Scene.Tracker.GetEntities<CassetteBlock>()) if (entity.Index == index || entity.Activated) entity.WillToggle();
        foreach (CassetteListener component in base.Scene.Tracker.GetComponents<CassetteListener>()) if (component.Index == index || component.Activated) component.WillToggle();
    }
    public override void SceneEnd(Scene scene)
    {
        base.SceneEnd(scene);
        if (!string.IsNullOrEmpty(SongName)) Audio.Stop(Snapshot); Audio.Stop(Song);
    }
    public override void Removed(Scene scene)
    {
        base.Removed(scene);
        if (!string.IsNullOrEmpty(SongName)) Audio.Stop(Snapshot); Audio.Stop(Song);
    }
}