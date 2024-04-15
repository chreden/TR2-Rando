﻿namespace TRLevelControl.Model;

public class TR1Level : TRLevelBase
{
    public List<TRTexImage8> Images8 { get; set; }
    public List<TRRoom> Rooms { get; set; }
    public List<ushort> FloorData { get; set; }

    /// <summary>
    /// 4 bytes
    /// </summary>
    public uint NumMeshData { get; set; }

    /// <summary>
    /// 2 * NumMeshData, holds the raw data stored in Meshes
    /// </summary>
    public ushort[] RawMeshData { get; set; }

    /// <summary>
    /// Variable
    /// </summary>
    public TRMesh[] Meshes { get; set; }

    /// <summary>
    /// 4 bytes
    /// </summary>
    public uint NumMeshPointers { get; set; }

    /// <summary>
    /// NumMeshPointers * 4 bytes
    /// </summary>
    public uint[] MeshPointers { get; set; }

    public List<TRAnimation> Animations { get; set; }
    public List<TRStateChange> StateChanges { get; set; }
    public List<TRAnimDispatch> AnimDispatches { get; set; }
    public List<TRAnimCommand> AnimCommands { get; set; }
    public List<TRMeshTreeNode> MeshTrees { get; set; }
    public List<ushort> Frames { get; set; }
    public List<TRModel> Models { get; set; }
    public List<TRStaticMesh> StaticMeshes { get; set; }

    /// <summary>
    /// 4 bytes
    /// </summary>
    public uint NumObjectTextures { get; set; }

    /// <summary>
    /// NumObjectTextures * 20 bytes
    /// </summary>
    public TRObjectTexture[] ObjectTextures { get; set; }

    /// <summary>
    /// 4 bytes
    /// </summary>
    public uint NumSpriteTextures { get; set; }

    /// <summary>
    /// NumSpriteTextures * 16 bytes
    /// </summary>
    public TRSpriteTexture[] SpriteTextures { get; set; }

    /// <summary>
    /// 4 bytes
    /// </summary>
    public uint NumSpriteSequences { get; set; }

    /// <summary>
    /// NumSpriteSequences * 8 bytes
    /// </summary>
    public TRSpriteSequence[] SpriteSequences { get; set; }
    public List<TRCamera> Cameras { get; set; }
    public List<TRSoundSource> SoundSources { get; set; }
    public List<TRBox> Boxes { get; set; }
    public List<ushort> Overlaps { get; set; }
    public List<TRZoneGroup> Zones { get; set; }

    /// <summary>
    /// 4 bytes
    /// </summary>
    public uint NumAnimatedTextures { get; set; }

    /// <summary>
    /// NumAnimatesTextures * 2 bytes
    /// </summary>
    public TRAnimatedTexture[] AnimatedTextures { get; set; }
    public List<TR1Entity> Entities { get; set; }
    public List<byte> LightMap { get; set; }
    public List<TRColour> Palette { get; set; }
    public List<TRCinematicFrame> CinematicFrames { get; set; }

    /// <summary>
    /// 2 bytes
    /// </summary>
    public ushort NumDemoData { get; set; }

    /// <summary>
    /// NumDemoData bytes
    /// </summary>
    public byte[] DemoData { get; set; }

    /// <summary>
    /// 370 entries of 2 bytes each = 740 bytes
    /// </summary>
    public short[] SoundMap { get; set; }

    /// <summary>
    /// 4 bytes
    /// </summary>
    public uint NumSoundDetails { get; set; }

    /// <summary>
    /// NumSoundDetails * 8 bytes
    /// </summary>
    public TRSoundDetails[] SoundDetails { get; set; }

    public uint NumSamples { get; set; }

    public byte[] Samples { get; set; }

    /// <summary>
    /// 4 bytes
    /// </summary>
    public uint NumSampleIndices { get; set; }

    /// <summary>
    /// NumSampleIndices * 4 bytes
    /// </summary>
    public uint[] SampleIndices { get; set; }
}
