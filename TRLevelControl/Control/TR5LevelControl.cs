﻿using System.Diagnostics;
using TRLevelControl.Model;

namespace TRLevelControl;

public class TR5LevelControl : TRLevelControlBase<TR5Level>
{
    protected override TR5Level CreateLevel(TRFileVersion version)
    {
        TR5Level level = new()
        {
            Version = new()
            {
                Game = TRGameVersion.TR5,
                File = version
            }
        };

        TestVersion(level, TRFileVersion.TR45);
        return level;
    }

    protected override void Read(TRLevelReader reader)
    {
        // Texture chunk
        ushort roomCount = reader.ReadUInt16();
        ushort objectCount = reader.ReadUInt16();
        reader.ReadUInt16(); // Previously bump in TR4, no longer used

        _level.Texture32Chunk = new();
        using TRLevelReader reader32 = reader.Inflate(_level.Texture32Chunk);
        _level.Texture32Chunk.Rooms = reader32.ReadImage32s(roomCount);
        _level.Texture32Chunk.Objects = reader32.ReadImage32s(objectCount);

        _level.Texture16Chunk = new();
        using TRLevelReader reader16 = reader.Inflate(_level.Texture16Chunk);
        _level.Texture16Chunk.Rooms = reader16.ReadImage16s(roomCount);
        _level.Texture16Chunk.Objects = reader16.ReadImage16s(objectCount);

        _level.SkyAndFont32Chunk = new();
        using TRLevelReader skyReader = reader.Inflate(_level.SkyAndFont32Chunk);
        _level.SkyAndFont32Chunk.Textiles = skyReader.ReadImage32s(3); // Shine, Font, Sky

        //TR5 Specific
        _level.LaraType = reader.ReadUInt16();
        _level.WeatherType = reader.ReadUInt16();
        _level.Padding = reader.ReadBytes(28);

        //Level Data Chunk
        //Get Raw Chunk Data
        _level.LevelDataChunk = new TR5LevelDataChunk
        {
            UncompressedSize = reader.ReadUInt32(),
            CompressedSize = reader.ReadUInt32()
        };
        _level.LevelDataChunk.CompressedChunk = reader.ReadBytes((int)_level.LevelDataChunk.CompressedSize);

        //Decompress
        DecompressLevelDataChunk(_level);

        //Samples
        _level.NumSamples = reader.ReadUInt32();
        _level.Samples = new TR4Sample[_level.NumSamples];

        for (int i = 0; i < _level.NumSamples; i++)
        {
            TR4Sample sample = new()
            {
                UncompSize = reader.ReadUInt32(),
                CompSize = reader.ReadUInt32(),
            };

            _level.Samples[i] = sample;

            //Compressed chunk is actually NOT zlib compressed - it is simply a WAV file.
            _level.Samples[i].CompressedChunk = reader.ReadBytes((int)_level.Samples[i].CompSize);
        }
    }

    protected override void Write(TRLevelWriter writer)
    {
        // Texture chunk
        Debug.Assert(_level.Texture32Chunk.Rooms.Count == _level.Texture16Chunk.Rooms.Count);
        Debug.Assert(_level.Texture32Chunk.Objects.Count == _level.Texture16Chunk.Objects.Count);
        Debug.Assert(_level.SkyAndFont32Chunk.Textiles.Count == 3);

        writer.Write((ushort)_level.Texture32Chunk.Rooms.Count);
        writer.Write((ushort)_level.Texture32Chunk.Objects.Count);
        writer.Write((ushort)0); // No bump

        using TRLevelWriter writer32 = new();
        writer32.Write(_level.Texture32Chunk.Rooms);
        writer32.Write(_level.Texture32Chunk.Objects);
        writer.Deflate(writer32, _level.Texture32Chunk);

        using TRLevelWriter writer16 = new();
        writer16.Write(_level.Texture16Chunk.Rooms);
        writer16.Write(_level.Texture16Chunk.Objects);
        writer.Deflate(writer16, _level.Texture16Chunk);

        using TRLevelWriter skyWriter = new();
        skyWriter.Write(_level.SkyAndFont32Chunk.Textiles);
        writer.Deflate(skyWriter, _level.SkyAndFont32Chunk);

        writer.Write(_level.LaraType);
        writer.Write(_level.WeatherType);
        writer.Write(_level.Padding);

        //Note - a TR5 Level data chunk is not compressed.
        byte[] chunk = _level.LevelDataChunk.Serialize();
        writer.Write(_level.LevelDataChunk.UncompressedSize);
        writer.Write(_level.LevelDataChunk.CompressedSize);
        writer.Write(chunk);

        writer.Write(_level.NumSamples);

        foreach (TR4Sample sample in _level.Samples)
        {
            writer.Write(sample.UncompSize);
            writer.Write(sample.CompSize);
            writer.Write(sample.CompressedChunk);
        }
    }

    private static void DecompressLevelDataChunk(TR5Level lvl)
    {
        //TR5 level chunk is not compressed
        byte[] buffer = lvl.LevelDataChunk.CompressedChunk;

        //Is the decompressed chunk the size we expected?
        Debug.Assert(buffer.Length == lvl.LevelDataChunk.UncompressedSize);

        using MemoryStream stream = new(buffer, false);
        using BinaryReader lvlChunkReader = new(stream);
        TR5FileReadUtilities.PopulateRooms(lvlChunkReader, lvl);
        TR5FileReadUtilities.PopulateFloordata(lvlChunkReader, lvl);
        TR5FileReadUtilities.PopulateMeshes(lvlChunkReader, lvl);
        TR5FileReadUtilities.PopulateAnimations(lvlChunkReader, lvl);
        TR5FileReadUtilities.PopulateMeshTreesFramesModels(lvlChunkReader, lvl);
        TR5FileReadUtilities.PopulateStaticMeshes(lvlChunkReader, lvl);
        TR5FileReadUtilities.VerifySPRMarker(lvlChunkReader, lvl);
        TR5FileReadUtilities.PopulateSprites(lvlChunkReader, lvl);
        TR5FileReadUtilities.PopulateCameras(lvlChunkReader, lvl);
        TR5FileReadUtilities.PopulateSoundSources(lvlChunkReader, lvl);
        TR5FileReadUtilities.PopulateBoxesOverlapsZones(lvlChunkReader, lvl);
        TR5FileReadUtilities.PopulateAnimatedTextures(lvlChunkReader, lvl);
        TR5FileReadUtilities.VerifyTEXMarker(lvlChunkReader, lvl);
        TR5FileReadUtilities.PopulateObjectTextures(lvlChunkReader, lvl);
        TR5FileReadUtilities.PopulateEntitiesAndAI(lvlChunkReader, lvl);
        TR5FileReadUtilities.PopulateDemoSoundSampleIndices(lvlChunkReader, lvl);
        TR5FileReadUtilities.VerifyLevelDataFinalSeperator(lvlChunkReader, lvl);
    }
}
