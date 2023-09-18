﻿using TRLevelControl.Model;

namespace TRRandomizerCore.Helpers;

public class LevelPickupZoneDescriptor
{
    //Key items can be used to place secret artefacts (in games such as TR3) -
    //so this will hold a list of actual key item entities to randomize to avoid secrets being randomized.
    public List<TR3Type> AliasedExpectedKeyItems { get; set; }
    public List<TR3Type> BaseExpectedKeyItems { get; set; }

    //Per each entity - what rooms that entity is allowed to be placed in (Zones).
    public Dictionary<TR3Type, List<int>> AllowedRooms { get; set; }
}
