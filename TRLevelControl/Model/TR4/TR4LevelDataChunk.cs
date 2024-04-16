﻿using TRLevelControl.Serialization;
using TRLevelControl.Compression;

namespace TRLevelControl.Model;

public class TR4LevelDataChunk : ISerializableCompact
{
    public static readonly string SPRMarker = "SPR";
    public static readonly string TEXMarker = "TEX";

    public uint UncompressedSize { get; set; }

    public uint CompressedSize { get; set; }

    public uint Unused { get; set; }
    public List<TR4Room> Rooms { get; set; }
    public List<ushort> FloorData { get; set; }
    public List<TR4Mesh> Meshes { get; set; }
    public List<uint> MeshPointers { get; set; }
    public List<TR4Animation> Animations { get; set; }
    public List<TRStateChange> StateChanges { get; set; }
    public List<TRAnimDispatch> AnimDispatches { get; set; }
    public List<TRAnimCommand> AnimCommands { get; set; }
    public List<TRMeshTreeNode> MeshTrees { get; set; }
    public List<ushort> Frames { get; set; }
    public List<TRModel> Models { get; set; }
    public List<TRStaticMesh> StaticMeshes { get; set; }
    public List<TRSpriteTexture> SpriteTextures { get; set; }
    public List<TRSpriteSequence> SpriteSequences { get; set; }
    public List<TRCamera> Cameras { get; set; }
    public List<TR4FlyByCamera> FlybyCameras { get; set; }
    public List<TRSoundSource> SoundSources { get; set; }
    public List<TR2Box> Boxes { get; set; }
    public List<ushort> Overlaps { get; set; }
    public List<short> Zones { get; set; }
    public List<TRAnimatedTexture> AnimatedTextures { get; set; }
    public byte AnimatedTexturesUVCount { get; set; }
    public List<TR4ObjectTexture> ObjectTextures { get; set; }
    public List<TR4Entity> Entities { get; set; }
    public List<TR4AIEntity> AIEntities { get; set; }
    public byte[] DemoData { get; set; }
    public short[] SoundMap { get; set; }
    public List<TR3SoundDetails> SoundDetails { get; set; }
    public List<uint> SampleIndices { get; set; }

    public byte[] Seperator { get; set; }

    //Optional - mainly just for testing, this is just to store the raw zlib compressed chunk.
    public byte[] CompressedChunk { get; set; }

    public byte[] Serialize()
    {
        using MemoryStream stream = new();
        using (TRLevelWriter writer = new(stream))
        {
            writer.Write(Unused);

            writer.Write((ushort)Rooms.Count);
            foreach (TR4Room room in Rooms)
            {
                writer.Write(room.Serialize());
            }

            writer.Write((uint)FloorData.Count);
            writer.Write(FloorData);

            List<byte> meshData = Meshes.SelectMany(m => m.Serialize()).ToList();
            writer.Write((uint)meshData.Count / 2);
            writer.Write(meshData.ToArray());

            writer.Write((uint)MeshPointers.Count);
            foreach (uint data in MeshPointers)
            {
                writer.Write(data);
            }

            writer.Write((uint)Animations.Count);
            foreach (TR4Animation anim in Animations)
            {
                writer.Write(anim.Serialize());
            }

            writer.Write((uint)StateChanges.Count);
            foreach (TRStateChange sc in StateChanges)
            {
                writer.Write(sc.Serialize());
            }

            writer.Write((uint)AnimDispatches.Count);
            foreach (TRAnimDispatch ad in AnimDispatches)
            {
                writer.Write(ad.Serialize());
            }

            writer.Write((uint)AnimCommands.Count);
            foreach (TRAnimCommand ac in AnimCommands)
            {
                writer.Write(ac.Serialize());
            }

            writer.Write((uint)MeshTrees.Count * 4); //To get the correct number /= 4 is done during read, make sure to reverse it here.
            foreach (TRMeshTreeNode node in MeshTrees)
            {
                writer.Write(node.Serialize());
            }

            writer.Write((uint)Frames.Count);
            foreach (ushort frame in Frames)
            {
                writer.Write(frame);
            }

            writer.Write((uint)Models.Count);
            foreach (TRModel model in Models)
            {
                writer.Write(model.Serialize());
            }

            writer.Write((uint)StaticMeshes.Count);
            foreach (TRStaticMesh sm in StaticMeshes)
            {
                writer.Write(sm.Serialize());
            }

            writer.Write(SPRMarker.ToCharArray());

            writer.Write((uint)SpriteTextures.Count);
            foreach (TRSpriteTexture st in SpriteTextures)
            {
                writer.Write(st.Serialize());
            }

            writer.Write((uint)SpriteSequences.Count);
            foreach (TRSpriteSequence seq in SpriteSequences)
            {
                writer.Write(seq.Serialize());
            }

            writer.Write((uint)Cameras.Count);
            foreach (TRCamera cam in Cameras)
            {
                writer.Write(cam.Serialize());
            }

            writer.Write((uint)FlybyCameras.Count);
            foreach (TR4FlyByCamera flycam in FlybyCameras)
            {
                writer.Write(flycam.Serialize());
            }

            writer.Write((uint)SoundSources.Count);
            foreach (TRSoundSource ssrc in SoundSources)
            {
                writer.Write(ssrc.Serialize());
            }

            writer.Write((uint)Boxes.Count);
            foreach (TR2Box box in Boxes)
            {
                writer.Write(box.Serialize());
            }

            writer.Write((uint)Overlaps.Count);
            foreach (ushort overlap in Overlaps)
            {
                writer.Write(overlap);
            }

            foreach (short zone in Zones)
            {
                writer.Write(zone);
            }

            byte[] animTextureData = AnimatedTextures.SelectMany(a => a.Serialize()).ToArray();
            writer.Write((uint)(animTextureData.Length / sizeof(ushort)) + 1);
            writer.Write((ushort)AnimatedTextures.Count);
            writer.Write(animTextureData);
            writer.Write(AnimatedTexturesUVCount);

            writer.Write(TEXMarker.ToCharArray());

            writer.Write((uint)ObjectTextures.Count);
            foreach (TR4ObjectTexture otex in ObjectTextures)
            {
                writer.Write(otex.Serialize());
            }

            writer.Write((uint)Entities.Count);
            writer.Write(Entities);

            writer.Write((uint)AIEntities.Count);
            writer.Write(AIEntities);

            writer.Write((ushort)DemoData.Length);
            writer.Write(DemoData);

            foreach (short sound in SoundMap)
            {
                writer.Write(sound);
            }

            writer.Write((uint)SoundDetails.Count);
            foreach (TR3SoundDetails snd in SoundDetails)
            {
                writer.Write(snd.Serialize());
            }

            writer.Write((uint)SampleIndices.Count);
            foreach (uint sampleindex in SampleIndices)
            {
                writer.Write(sampleindex);
            }

            writer.Write(Seperator);
        }

        byte[] uncompressed = stream.ToArray();
        UncompressedSize = (uint)uncompressed.Length;

        byte[] compressed = TRZlib.Compress(uncompressed);
        CompressedSize = (uint)compressed.Length;

        return compressed;
    }
}
