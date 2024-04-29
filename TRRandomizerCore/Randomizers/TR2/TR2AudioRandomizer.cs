﻿using Newtonsoft.Json;
using System.Numerics;
using TRFDControl;
using TRFDControl.FDEntryTypes;
using TRFDControl.Utilities;
using TRGE.Core;
using TRLevelControl;
using TRLevelControl.Helpers;
using TRLevelControl.Model;
using TRRandomizerCore.Helpers;
using TRRandomizerCore.Levels;
using TRRandomizerCore.SFX;

namespace TRRandomizerCore.Randomizers;

public class TR2AudioRandomizer : BaseTR2Randomizer
{
    private AudioRandomizer _audioRandomizer;

    private List<TR2SFXDefinition> _soundEffects;
    private List<TRSFXGeneralCategory> _sfxCategories;
    private List<TR2ScriptedLevel> _uncontrolledLevels;

    public override void Randomize(int seed)
    {
        _generator = new Random(seed);

        LoadAudioData();
        ChooseUncontrolledLevels();

        foreach (TR2ScriptedLevel lvl in Levels)
        {
            LoadLevelInstance(lvl);

            RandomizeMusicTriggers(_levelInstance);

            RandomizeSoundEffects(_levelInstance);

            RandomizeWibble(_levelInstance);

            SaveLevelInstance();

            if (!TriggerProgress())
            {
                break;
            }
        }
    }

    private void ChooseUncontrolledLevels()
    {
        TR2ScriptedLevel assaultCourse = Levels.Find(l => l.Is(TR2LevelNames.ASSAULT));
        ISet<TR2ScriptedLevel> exlusions = new HashSet<TR2ScriptedLevel> { assaultCourse };

        _uncontrolledLevels = Levels.RandomSelection(_generator, (int)Settings.UncontrolledSFXCount, exclusions: exlusions);
        if (Settings.AssaultCourseWireframe)
        {
            _uncontrolledLevels.Add(assaultCourse);
        }
    }

    public bool IsUncontrolledLevel(TR2ScriptedLevel level)
    {
        return _uncontrolledLevels.Contains(level);
    }

    private void RandomizeMusicTriggers(TR2CombinedLevel level)
    {
        FDControl floorData = new();
        floorData.ParseFromLevel(level.Data);

        if (Settings.ChangeTriggerTracks)
        {
            RandomizeFloorTracks(level.Data, floorData);
        }

        if (Settings.SeparateSecretTracks)
        {
            RandomizeSecretTracks(level.Data, floorData);
        }

        floorData.WriteToLevel(level.Data);
    }

    private void RandomizeFloorTracks(TR2Level level, FDControl floorData)
    {
        _audioRandomizer.ResetFloorMap();
        foreach (TR2Room room in level.Rooms)
        {
            _audioRandomizer.RandomizeFloorTracks(room.Sectors, floorData, _generator, sectorIndex =>
            {
                // Get the midpoint of the tile in world coordinates
                return new Vector2
                (
                    TRConsts.Step2 + room.Info.X + sectorIndex / room.NumZSectors * TRConsts.Step4,
                    TRConsts.Step2 + room.Info.Z + sectorIndex % room.NumZSectors * TRConsts.Step4
                );
            });
        }
    }

    private void RandomizeSecretTracks(TR2Level level, FDControl floorData)
    {
        // Generate new triggers for secrets to allow different sounds for each one
        List<TRAudioTrack> secretTracks = _audioRandomizer.GetTracks(TRAudioCategory.Secret);
        Dictionary<int, TR2Entity> secrets = GetSecretItems(level);
        foreach (int entityIndex in secrets.Keys)
        {
            TR2Entity secret = secrets[entityIndex];
            TRRoomSector sector = FDUtilities.GetRoomSector(secret.X, secret.Y, secret.Z, secret.Room, level, floorData);
            if (sector.FDIndex == 0)
            {
                // The secret is positioned on a tile that currently has no FD, so create it
                floorData.CreateFloorData(sector);
            }

            List<FDEntry> entries = floorData.Entries[sector.FDIndex];
            FDTriggerEntry existingTriggerEntry = entries.Find(e => e is FDTriggerEntry) as FDTriggerEntry;
            bool existingEntityPickup = false;
            if (existingTriggerEntry != null)
            {
                if (existingTriggerEntry.TrigType == FDTrigType.Pickup && existingTriggerEntry.TrigActionList[0].Parameter == entityIndex)
                {
                    // GW gold secret (default location) already has a pickup trigger to spawn the
                    // TRex (or whatever enemy) so we'll just append to that item list here
                    existingEntityPickup = true;
                }
                else
                {
                    // There is already a non-pickup trigger for this sector so while nothing is wrong with
                    // adding a pickup trigger, the game ignores it. So in this instance, the sound that
                    // plays will just be whatever is set in the script.
                    continue;
                }
            }

            // Generate a new music action
            FDActionListItem musicAction = new()
            {
                TrigAction = FDTrigAction.PlaySoundtrack,
                Parameter = secretTracks[_generator.Next(0, secretTracks.Count)].ID
            };

            // For GW default gold, just append it
            if (existingEntityPickup)
            {
                existingTriggerEntry.TrigActionList.Add(musicAction);
            }
            else
            {
                entries.Add(new FDTriggerEntry
                {
                    // The values here are replicated from Trigger#112 (in trview) in GW.
                    // The first action list must be the entity being picked up and so
                    // remaining action list items are actioned on pick up.
                    Setup = new FDSetup { Value = 1028 },
                    TrigSetup = new FDTrigSetup { Value = 15872 },
                    TrigActionList = new List<FDActionListItem>
                    {
                        new() {
                            TrigAction = FDTrigAction.Object,
                            Parameter = (ushort)entityIndex
                        },
                        musicAction
                    }
                });
            }
        }
    }

    private static Dictionary<int, TR2Entity> GetSecretItems(TR2Level level)
    {
        Dictionary<int, TR2Entity> entities = new();
        for (int i = 0; i < level.Entities.Count; i++)
        {
            if (TR2TypeUtilities.IsSecretType(level.Entities[i].TypeID))
            {
                entities[i] = level.Entities[i];
            }
        }

        return entities;
    }

    private void LoadAudioData()
    {
        // Get the track data from audio_tracks.json. Loaded from TRGE as it sets the ambient tracks initially.
        _audioRandomizer = new AudioRandomizer(ScriptEditor.AudioProvider.GetCategorisedTracks());

        // Decide which sound effect categories we want to randomize.
        _sfxCategories = AudioRandomizer.GetSFXCategories(Settings);

        // Only load the SFX if we are changing at least one category
        if (_sfxCategories.Count > 0)
        {
            _soundEffects = JsonConvert.DeserializeObject<List<TR2SFXDefinition>>(ReadResource(@"TR2\Audio\sfx.json"));

            Dictionary<string, TR2Level> levels = new();
            TR2LevelControl reader = new();
            foreach (TR2SFXDefinition definition in _soundEffects)
            {
                if (!levels.ContainsKey(definition.SourceLevel))
                {
                    levels[definition.SourceLevel] = reader.Read(Path.Combine(BackupPath, definition.SourceLevel));
                }

                TR2Level level = levels[definition.SourceLevel];
                definition.SoundEffect = level.SoundEffects[definition.InternalIndex];
            }
        }
    }

    private void RandomizeSoundEffects(TR2CombinedLevel level)
    {
        if (_sfxCategories.Count == 0)
        {
            // We haven't selected any SFX categories to change.
            return;
        }

        if (IsUncontrolledLevel(level.Script))
        {
            // Choose a random but unique pointer into MAIN.SFX for each sample.
            int maxSample = Enum.GetValues<TR2SFX>().Length;
            HashSet<uint> indices = new();
            foreach (var (_, effect) in level.Data.SoundEffects)
            {
                for (int i = 0; i < effect.Samples.Count; i++)
                {
                    uint sample;
                    do
                    {
                        sample = (uint)_generator.Next(0, maxSample + 1);
                    }
                    while (!indices.Add(sample));
                    effect.Samples[i] = sample;
                }
            }
        }
        else
        {
            // Run through the SoundMap for this level and get the SFX definition for each one.
            // Choose a new sound effect provided the definition is in a category we want to change.
            // Lara's SFX are not changed by default.
            foreach (TR2SFX internalIndex in Enum.GetValues<TR2SFX>())
            {
                TR2SFXDefinition definition = _soundEffects.Find(sfx => sfx.InternalIndex == internalIndex);
                if (!level.Data.SoundEffects.ContainsKey(internalIndex) || definition == null
                    || definition.Creature == TRSFXCreatureCategory.Lara || !_sfxCategories.Contains(definition.PrimaryCategory))
                {
                    continue;
                }

                // The following allows choosing to keep humans making human noises, and animals animal noises.
                // Other humans can use Lara's SFX.
                Predicate<TR2SFXDefinition> pred;
                if (Settings.LinkCreatureSFX && definition.Creature > TRSFXCreatureCategory.Lara)
                {
                    pred = sfx =>
                    {
                        return sfx.Categories.Contains(definition.PrimaryCategory) &&
                        sfx != definition &&
                        (
                            sfx.Creature == definition.Creature ||
                            (sfx.Creature == TRSFXCreatureCategory.Lara && definition.Creature == TRSFXCreatureCategory.Human)
                        );
                    };
                }
                else
                {
                    pred = sfx => sfx.Categories.Contains(definition.PrimaryCategory) && sfx != definition;
                }

                // Try to find definitions that match
                List<TR2SFXDefinition> otherDefinitions = _soundEffects.FindAll(pred);
                if (otherDefinitions.Count > 0)
                {
                    // Pick a new definition and try to import it into the level. This should only fail if
                    // the JSON is misconfigured e.g. missing sample indices. In that case, we just leave 
                    // the current sound effect as-is.
                    TR2SFXDefinition nextDefinition = otherDefinitions[_generator.Next(0, otherDefinitions.Count)];
                    if (nextDefinition != definition)
                    {
                        level.Data.SoundEffects[internalIndex] = nextDefinition.SoundEffect;
                    }
                }
            }
        }
    }

    private void RandomizeWibble(TR2CombinedLevel level)
    {
        if (Settings.RandomizeWibble)
        {
            foreach (var (_, effect) in level.Data.SoundEffects)
            {
                effect.RandomizePitch = true;
            }
        }
    }
}
