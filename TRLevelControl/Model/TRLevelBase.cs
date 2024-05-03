﻿namespace TRLevelControl.Model;

public abstract class TRLevelBase
{
    public TRVersion Version { get; set; }
    public FDControl FloorData { get; set; }
    public List<TRBox> Boxes { get; set; }
    public abstract IEnumerable<TRMesh> DistinctMeshes { get; }
}
