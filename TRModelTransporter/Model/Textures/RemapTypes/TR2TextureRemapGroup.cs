﻿using TRLevelControl.Model;
using TRLevelControl.Model.Enums;
using TRModelTransporter.Packing;

namespace TRModelTransporter.Model.Textures;

public class TR2TextureRemapGroup : AbstractTextureRemapGroup<TR2Entities, TR2Level>
{
    protected override IEnumerable<TR2Entities> GetModelTypes(TR2Level level)
    {
        List<TR2Entities> types = new();
        foreach (TRModel model in level.Models)
        {
            types.Add((TR2Entities)model.ID);
        }
        return types;
    }

    protected override AbstractTexturePacker<TR2Entities, TR2Level> CreatePacker(TR2Level level)
    {
        return new TR2TexturePacker(level);
    }
}
