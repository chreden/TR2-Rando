﻿using TRGE.Core;
using TRLevelReader.Helpers;
using TRLevelReader.Model;

namespace TRRandomizerCore.Levels
{
    public class TR3CombinedLevel
    {
        /// <summary>
        /// The main level data stored in the corresponding .TR2 file.
        /// </summary>
        public TR3Level Data { get; set; }

        /// <summary>
        /// The scripting information for the level stored in TOMBPC.dat.
        /// </summary>
        public TR3ScriptedLevel Script { get; set; }

        /// <summary>
        /// The uppercase base file name of the level e.g. KEEL.TR2
        /// </summary>
        public string Name => Script.LevelFileBaseName.ToUpper();

        /// <summary>
        /// The level data for the cutscene at the end of this level, if any.
        /// </summary>
        public TR3CombinedLevel CutSceneLevel { get; set; }

        /// <summary>
        /// A reference to the main level if this is a CutScene level.
        /// </summary>
        public TR3CombinedLevel ParentLevel { get; set; }

        /// <summary>
        /// True if this is a CutScene level, and so has a parent level.
        /// </summary>
        public bool IsCutScene => ParentLevel != null;

        /// <summary>
        /// Whether or not this level has a cutscene at the end.
        /// </summary>
        public bool HasCutScene => Script.HasCutScene;

        /// <summary>
        /// Gets the level's sequence in the game. If this is a CutScene level, this returns the parent sequence.
        /// </summary>
        public int Sequence => IsCutScene ? ParentLevel.Sequence : Script.Sequence;

        /// <summary>
        /// Compares the given file name or path against the base file name of the level (case-insensitive).
        /// </summary>
        public bool Is(string levelFileName) => Script.Is(levelFileName);

        /// <summary>
        /// Checks if the current level is the assault course.
        /// </summary>
        public bool IsAssault => Is(TR3LevelNames.ASSAULT);

        /// <summary>
        /// The exposure meter is hard-coded to the Antarctica and RX-Tech Mines level sequences.
        /// </summary>
        public bool HasExposureMeter => Sequence == 16 || Sequence == 17;
    }
}