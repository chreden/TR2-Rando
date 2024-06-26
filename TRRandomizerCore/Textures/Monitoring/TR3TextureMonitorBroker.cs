﻿using TRImageControl.Textures;
using TRLevelControl.Helpers;
using TRLevelControl.Model;

namespace TRRandomizerCore.Textures;

public class TR3TextureMonitorBroker : AbstractTextureMonitorBroker<TR3Type>
{
    protected override Dictionary<TR3Type, TR3Type> ExpandedMonitorMap => null;

    protected override TextureDatabase<TR3Type> CreateDatabase()
    {
        return new TR3TextureDatabase();
    }

    protected override TR3Type TranslateAlias(string lvlName, TR3Type type)
    {
        return TR3TypeUtilities.GetAliasForLevel(lvlName, type);
    }
}
