﻿using System;
using System.Collections.Generic;
using TREnvironmentEditor;
using TREnvironmentEditor.Model;
using TREnvironmentEditor.Model.Types;
using TRGE.Core;
using TRLevelReader.Helpers;
using TRLevelReader.Model.Enums;
using TRRandomizerCore.Helpers;
using TRRandomizerCore.Levels;
using TRRandomizerCore.Textures;

namespace TRRandomizerCore.Randomizers
{
    public class TR2EnvironmentRandomizer : BaseTR2Randomizer
    {
        internal bool EnforcedModeOnly => !Settings.RandomizeEnvironment;
        internal TR2TextureMonitorBroker TextureMonitor { get; set; }

        private List<EMType> _disallowedTypes;
        private List<TR2ScriptedLevel> _levelsToMirror;

        public override void Randomize(int seed)
        {
            _generator = new Random(seed);

            _disallowedTypes = new List<EMType>();
            if (!Settings.RandomizeWaterLevels)
            {
                _disallowedTypes.Add(EMType.Flood);
                _disallowedTypes.Add(EMType.Drain);
            }
            if (!Settings.RandomizeSlotPositions)
            {
                _disallowedTypes.Add(EMType.MoveSlot);
                _disallowedTypes.Add(EMType.SwapSlot);
            }
            if (!Settings.RandomizeLadders)
            {
                _disallowedTypes.Add(EMType.Ladder);
            }

            _levelsToMirror = Levels.RandomSelection(_generator, (int)Settings.MirroredLevelCount, exclusions:new HashSet<TR2ScriptedLevel>
            {
                Levels.Find(l => l.Is(TR2LevelNames.ASSAULT))
            });

            foreach (TR2ScriptedLevel lvl in Levels)
            {
                LoadLevelInstance(lvl);

                RandomizeEnvironment(_levelInstance);

                SaveLevelInstance();

                if (!TriggerProgress())
                {
                    break;
                }
            }
        }

        private void RandomizeEnvironment(TR2CombinedLevel level)
        {
            EMEditorMapping mapping = EMEditorMapping.Get(GetResourcePath(@"TR2\Environment\" + level.Name + "-Environment.json"));
            if (mapping != null)
            {
                if (level.IsUKBox)
                {
                    // The mapping is configured for EPC and Multipatch texture indices, but should
                    // have alternate mapping defined for UKBox, so switch to it in this case.
                    mapping.AlternateTextures();
                }
                ApplyMappingToLevel(level, mapping);
            }

            if (!EnforcedModeOnly && (_levelsToMirror.Contains(level.Script) || (level.IsAssault && Settings.MirrorAssaultCourse)))
            {
                MirrorLevel(level, mapping);
            }
        }

        private void ApplyMappingToLevel(TR2CombinedLevel level, EMEditorMapping mapping)
        {
            EMType[] emptyExclusions = new EMType[] { };

            // Process enforced packs first. We do not pass disallowed types here.
            // These generally fix OG issues such as problems with box overlaps and
            // textures.
            mapping.All.ApplyToLevel(level.Data, emptyExclusions);
            
            if (!EnforcedModeOnly || !Settings.PuristMode)
            {
                // Non-purist packs generally make return paths available.
                // These are applied only if Purist mode is off or if Environment
                // rando is on as a whole, because some other categories may rely
                // on these changes having been made.
                mapping.NonPurist.ApplyToLevel(level.Data, emptyExclusions);
            }

            if (!EnforcedModeOnly)
            {
                if (mapping.Any.Count > 0)
                {
                    // Pick a random number of packs to apply, but at least 1
                    int packCount = _generator.Next(1, mapping.Any.Count + 1);
                    List<EMEditorSet> randomSet = mapping.Any.RandomSelection(_generator, packCount);
                    foreach (EMEditorSet mod in randomSet)
                    {
                        mod.ApplyToLevel(level.Data, _disallowedTypes);
                    }
                }

                // AllWithin means one from each set will be applied. Used for the likes of choosing a new
                // keyhole position from a set.
                foreach (List<EMEditorSet> modList in mapping.AllWithin)
                {
                    EMEditorSet mod = modList[_generator.Next(0, modList.Count)];
                    mod.ApplyToLevel(level.Data, _disallowedTypes);
                }

                // ConditionalAllWithin is similar to above, but different sets of mods can be returned based
                // on a given condition. For example, move a slot to a room, but only if a specific entity exists.
                foreach (EMConditionalEditorSet conditionalSet in mapping.ConditionalAllWithin)
                {
                    List<EMEditorSet> modList = conditionalSet.GetApplicableSets(level.Data);
                    if (modList != null && modList.Count > 0)
                    {
                        EMEditorSet mod = modList[_generator.Next(0, modList.Count)];
                        mod.ApplyToLevel(level.Data, _disallowedTypes);
                    }
                }

                // OneOf is used for a leader-follower situation, but where only one follower from
                // a group is wanted. An example is removing a ladder (the leader) and putting it in 
                // a different position, so the followers are the different positions from which we pick one.
                foreach (EMEditorGroupedSet mod in mapping.OneOf)
                {
                    EMEditorSet follower = mod.Followers[_generator.Next(0, mod.Followers.Count)];
                    mod.ApplyToLevel(level.Data, follower, _disallowedTypes);
                }
            }

            // Similar to All, but these mods will have conditions configured so may
            // or may not apply. Process these last so that conditions based on other
            // mods can be used.
            foreach (EMConditionalSingleEditorSet mod in mapping.ConditionalAll)
            {
                mod.ApplyToLevel(level.Data, emptyExclusions);
            }
        }

        private void MirrorLevel(TR2CombinedLevel level, EMEditorMapping mapping)
        {
            EMMirrorFunction mirrorer = new EMMirrorFunction();
            mirrorer.ApplyToLevel(level.Data);

            if (mapping != null)
            {
                // Process packs that need to be applied after mirroring.
                mapping.Mirrored.ApplyToLevel(level.Data, new EMType[] { });
            }

            // Notify the texture monitor that this level has been flipped
            TextureMonitor<TR2Entities> monitor = TextureMonitor.CreateMonitor(level.Name);
            monitor.UseMirroring = true;
        }
    }
}