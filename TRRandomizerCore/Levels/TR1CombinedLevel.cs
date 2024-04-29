﻿using TRGE.Core;
using TRLevelControl.Helpers;
using TRLevelControl.Model;

namespace TRRandomizerCore.Levels;

public class TR1CombinedLevel
{
    private const string _steamPyramidChecksum = "2205228f27e5ff5eb9912d8ec0f001ef";

    /// <summary>
    /// The main level data stored in the corresponding .PHD file.
    /// </summary>
    public TR1Level Data { get; set; }

    /// <summary>
    /// The scripting information for the level stored in TR1X_gameflow.json5.
    /// </summary>
    public TR1ScriptedLevel Script { get; set; }

    /// <summary>
    /// The checksum of the backed up level file.
    /// </summary>
    public string Checksum { get; set; }

    /// <summary>
    /// The uppercase base file name of the level e.g. LEVEL1.PHD
    /// </summary>
    public string Name => Script.LevelFileBaseName.ToUpper();

    /// <summary>
    /// The level data for the cutscene at the end of this level, if any.
    /// </summary>
    public TR1CombinedLevel CutSceneLevel { get; set; }

    /// <summary>
    /// A reference to the main level if this is a CutScene level.
    /// </summary>
    public TR1CombinedLevel ParentLevel { get; set; }
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
    public bool IsAssault => Is(TR1LevelNames.ASSAULT);

    /// <summary>
    /// Tests if this level is the Steam/GoG version of Great Pyramid.
    /// </summary>
    public bool IsSteamPyramid => Is(TR1LevelNames.PYRAMID) && Checksum == _steamPyramidChecksum;

    public bool IsExpansion => TR1LevelNames.AsListGold.Contains(Name);

    /// <summary>
    /// Returns {Name}-Steam if IsSteamPyramid, otherwise just {Name}.
    /// </summary>
    public string JsonID => IsSteamPyramid ? Name + "-Steam" : Name;
}
