﻿using TRGE.Core;
using TRLevelControl.Model;
using TRRandomizerCore.Helpers;
using TRRandomizerCore.Randomizers;

namespace TRRandomizerCore.Editors;

public class TR1RemasteredEditor : TR1ClassicEditor
{
    public TR1RemasteredEditor(TRDirectoryIOArgs io, TREdition edition)
        : base(io, edition) { }

    protected override void ApplyConfig(Config config)
    {
        base.ApplyConfig(config);
        Settings.AllowReturnPathLocations = false;
        Settings.AddReturnPaths = false;
        Settings.FixOGBugs = false;
    }

    protected override int GetSaveTarget(int numLevels)
    {
        int target = 0;

        if (Settings.RandomizeItems)
        {
            target += numLevels;
            if (Settings.IncludeKeyItems)
            {
                target += numLevels;
            }
        }

        if (Settings.RandomizeStartPosition)
        {
            target += numLevels;
        }

        if (Settings.RandomizeAudio)
        {
            target += numLevels;
        }

        // Environment randomizer always runs
        target += numLevels * 2;

        return target;
    }

    protected override void SaveImpl(AbstractTRScriptEditor scriptEditor, TRSaveMonitor monitor)
    {
        List<TRRScriptedLevel> levels = new(
            scriptEditor.EnabledScriptedLevels.Cast<TRRScriptedLevel>().ToList()
        );

        if (scriptEditor.GymAvailable)
        {
            levels.Add(scriptEditor.AssaultLevel as TRRScriptedLevel);
        }

        string backupDirectory = _io.BackupDirectory.FullName;
        string wipDirectory = _io.WIPOutputDirectory.FullName;

        ItemFactory<TR1Entity> itemFactory = new(@"Resources\TR1\Items\repurposable_items.json");
        TR1RItemRandomizer itemRandomizer = new()
        {
            ScriptEditor = scriptEditor,
            Levels = levels,
            BasePath = wipDirectory,
            BackupPath = backupDirectory,
            SaveMonitor = monitor,
            Settings = Settings,
            ItemFactory = itemFactory
        };

        TR1REnvironmentRandomizer environmentRandomizer = new()
        {
            ScriptEditor = scriptEditor,
            Levels = levels,
            BasePath = wipDirectory,
            BackupPath = backupDirectory,
            SaveMonitor = monitor,
            Settings = Settings,
        };

        if (!monitor.IsCancelled && Settings.RandomizeItems)
        {
            monitor.FireSaveStateBeginning(TRSaveCategory.Custom, "Randomizing standard items");
            itemRandomizer.Randomize(Settings.ItemSeed);
        }

        if (!monitor.IsCancelled && Settings.RandomizeStartPosition)
        {
            monitor.FireSaveStateBeginning(TRSaveCategory.Custom, "Randomizing start positions");
            new TR1RStartPositionRandomizer
            {
                ScriptEditor = scriptEditor,
                Levels = levels,
                BasePath = wipDirectory,
                BackupPath = backupDirectory,
                SaveMonitor = monitor,
                Settings = Settings
            }.Randomize(Settings.StartPositionSeed);
        }

        if (!monitor.IsCancelled)
        {
            monitor.FireSaveStateBeginning(TRSaveCategory.Custom, Settings.RandomizeEnvironment ? "Randomizing environment" : "Applying default environment packs");
            environmentRandomizer.Randomize(Settings.EnvironmentSeed);
        }

        if (!monitor.IsCancelled && Settings.RandomizeItems && Settings.IncludeKeyItems)
        {
            monitor.FireSaveStateBeginning(TRSaveCategory.Custom, "Randomizing key items");
            itemRandomizer.RandomizeKeyItems();
        }

        if (!monitor.IsCancelled)
        {
            monitor.FireSaveStateBeginning(TRSaveCategory.Custom, "Finalizing environment changes");
            environmentRandomizer.FinalizeEnvironment();
        }

        if (!monitor.IsCancelled && Settings.RandomizeAudio)
        {
            monitor.FireSaveStateBeginning(TRSaveCategory.Custom, "Randomizing audio tracks");
            new TR1RAudioRandomizer
            {
                ScriptEditor = scriptEditor,
                Levels = levels,
                BasePath = wipDirectory,
                BackupPath = backupDirectory,
                SaveMonitor = monitor,
                Settings = Settings
            }.Randomize(Settings.AudioSeed);
        }
    }
}
