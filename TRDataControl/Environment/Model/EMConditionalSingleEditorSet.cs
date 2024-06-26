﻿using TRLevelControl.Model;

namespace TRDataControl.Environment;

public class EMConditionalSingleEditorSet : ITextureModifier
{
    public BaseEMCondition Condition { get; set; }
    public EMEditorSet OnTrue { get; set; }
    public EMEditorSet OnFalse { get; set; }

    public void ApplyToLevel(TR1Level level, EMOptions options = null)
    {
        EMEditorSet edits = Condition.GetResult(level) ? OnTrue : OnFalse;
        edits?.ApplyToLevel(level, options);
    }

    public void ApplyToLevel(TR2Level level, EMOptions options = null)
    {
        EMEditorSet edits = Condition.GetResult(level) ? OnTrue : OnFalse;
        edits?.ApplyToLevel(level, options);
    }

    public void ApplyToLevel(TR3Level level, EMOptions options = null)
    {
        EMEditorSet edits = Condition.GetResult(level) ? OnTrue : OnFalse;
        edits?.ApplyToLevel(level, options);
    }

    public void RemapTextures(Dictionary<ushort, ushort> indexMap)
    {
        OnTrue?.RemapTextures(indexMap);
        OnFalse?.RemapTextures(indexMap);
    }

    public void SetCommunityPatch(bool isCommunityPatch)
    {
        OnTrue?.SetCommunityPatch(isCommunityPatch);
        OnFalse?.SetCommunityPatch(isCommunityPatch);
    }
}
