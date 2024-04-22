﻿using System.Diagnostics;
using TRLevelControl.Build;
using TRLevelControl.Helpers;
using TRLevelControl.Model;

namespace TRLevelControl;

public class TR1LevelControl : TRLevelControlBase<TR1Level>
{
    public TR1LevelControl(ITRLevelObserver observer = null)
        : base(observer) { }

    protected override TR1Level CreateLevel(TRFileVersion version)
    {
        TR1Level level = new()
        {
            Version = new()
            {
                Game = TRGameVersion.TR1,
                File = version
            }
        };

        TestVersion(level, TRFileVersion.TR1);
        return level;
    }

    protected override void Read(TRLevelReader reader)
    {
        uint numImages = reader.ReadUInt32();
        _level.Images8 = reader.ReadImage8s(numImages);

        // Unused, always 0 in OG
        _level.Version.LevelNumber = reader.ReadUInt32();

        ushort numRooms = reader.ReadUInt16();
        _level.Rooms = new();
        for (int i = 0; i < numRooms; i++)
        {
            TRRoom room = new()
            {
                //Grab info
                Info = new TRRoomInfo
                {
                    X = reader.ReadInt32(),
                    Z = reader.ReadInt32(),
                    YBottom = reader.ReadInt32(),
                    YTop = reader.ReadInt32()
                },

                //Grab data
                NumDataWords = reader.ReadUInt32()
            };
            _level.Rooms.Add(room);

            room.Data = new ushort[room.NumDataWords];
            for (int j = 0; j < room.NumDataWords; j++)
            {
                room.Data[j] = reader.ReadUInt16();
            }

            //Store what we just read
            room.RoomData = ConvertToRoomData(room);

            //Portals
            room.NumPortals = reader.ReadUInt16();
            room.Portals = new TRRoomPortal[room.NumPortals];
            for (int j = 0; j < room.NumPortals; j++)
            {
                room.Portals[j] = TR2FileReadUtilities.ReadRoomPortal(reader);
            }

            //Sectors
            room.NumZSectors = reader.ReadUInt16();
            room.NumXSectors = reader.ReadUInt16();
            room.Sectors = new TRRoomSector[room.NumXSectors * room.NumZSectors];
            for (int j = 0; j < (room.NumXSectors * room.NumZSectors); j++)
            {
                room.Sectors[j] = TR2FileReadUtilities.ReadRoomSector(reader);
            }

            //Lighting
            room.AmbientIntensity = reader.ReadInt16();
            room.NumLights = reader.ReadUInt16();
            room.Lights = new TRRoomLight[room.NumLights];
            for (int j = 0; j < room.NumLights; j++)
            {
                room.Lights[j] = TRFileReadUtilities.ReadRoomLight(reader);
            }

            //Static meshes
            room.NumStaticMeshes = reader.ReadUInt16();
            room.StaticMeshes = new TRRoomStaticMesh[room.NumStaticMeshes];
            for (int j = 0; j < room.NumStaticMeshes; j++)
            {
                room.StaticMeshes[j] = TRFileReadUtilities.ReadRoomStaticMesh(reader);
            }

            room.AlternateRoom = reader.ReadInt16();
            room.Flags = reader.ReadInt16();
        }

        uint numFloorData = reader.ReadUInt32();
        _level.FloorData = reader.ReadUInt16s(numFloorData).ToList();

        ReadMeshData(reader);

        //Animations
        uint numAnimations = reader.ReadUInt32();
        _level.Animations = new();
        for (int i = 0; i < numAnimations; i++)
        {
            _level.Animations.Add(TR2FileReadUtilities.ReadAnimation(reader));
        }

        //State Changes
        uint numStateChanges = reader.ReadUInt32();
        _level.StateChanges = new();
        for (int i = 0; i < numStateChanges; i++)
        {
            _level.StateChanges.Add(TR2FileReadUtilities.ReadStateChange(reader));
        }

        //Animation Dispatches
        uint numAnimDispatches = reader.ReadUInt32();
        _level.AnimDispatches = new();
        for (int i = 0; i < numAnimDispatches; i++)
        {
            _level.AnimDispatches.Add(TR2FileReadUtilities.ReadAnimDispatch(reader));
        }

        //Animation Commands
        uint numAnimCommands = reader.ReadUInt32();
        _level.AnimCommands = new();
        for (int i = 0; i < numAnimCommands; i++)
        {
            _level.AnimCommands.Add(TR2FileReadUtilities.ReadAnimCommand(reader));
        }

        //Mesh Trees
        uint numMeshTrees = reader.ReadUInt32() / 4;
        _level.MeshTrees = new();
        for (int i = 0; i < numMeshTrees; i++)
        {
            _level.MeshTrees.Add(TR2FileReadUtilities.ReadMeshTreeNode(reader));
        }

        //Frames
        uint numFrames = reader.ReadUInt32();
        _level.Frames = new();
        for (int i = 0; i < numFrames; i++)
        {
            _level.Frames.Add(reader.ReadUInt16());
        }

        //Models
        uint numModels = reader.ReadUInt32();
        _level.Models = new();
        for (int i = 0; i < numModels; i++)
        {
            _level.Models.Add(TR2FileReadUtilities.ReadModel(reader));
        }

        //Static Meshes
        uint numStaticMeshes = reader.ReadUInt32();
        _level.StaticMeshes = new();
        for (int i = 0; i < numStaticMeshes; i++)
        {
            _level.StaticMeshes.Add(TR2FileReadUtilities.ReadStaticMesh(reader));
        }

        uint numObjectTextures = reader.ReadUInt32();
        _level.ObjectTextures = new();
        for (int i = 0; i < numObjectTextures; i++)
        {
            _level.ObjectTextures.Add(TR2FileReadUtilities.ReadObjectTexture(reader));
        }

        uint numSpriteTextures = reader.ReadUInt32();
        _level.SpriteTextures = new();
        for (int i = 0; i < numSpriteTextures; i++)
        {
            _level.SpriteTextures.Add(TR2FileReadUtilities.ReadSpriteTexture(reader));
        }

        uint numSpriteSequences = reader.ReadUInt32();
        _level.SpriteSequences = new();
        for (int i = 0; i < numSpriteSequences; i++)
        {
            _level.SpriteSequences.Add(TR2FileReadUtilities.ReadSpriteSequence(reader));
        }

        //Cameras
        uint numCameras = reader.ReadUInt32();
        _level.Cameras = new();
        for (int i = 0; i < numCameras; i++)
        {
            _level.Cameras.Add(TR2FileReadUtilities.ReadCamera(reader));
        }

        uint numSoundSources = reader.ReadUInt32();
        _level.SoundSources = new();
        for (int i = 0; i < numSoundSources; i++)
        {
            _level.SoundSources.Add(TR2FileReadUtilities.ReadSoundSource(reader));
        }

        //Boxes
        uint numBoxes = reader.ReadUInt32();
        _level.Boxes = new();
        for (int i = 0; i < numBoxes; i++)
        {
            _level.Boxes.Add(TRFileReadUtilities.ReadBox(reader));
        }

        //Overlaps & Zones
        uint numOverlaps = reader.ReadUInt32();
        _level.Overlaps = reader.ReadUInt16s(numOverlaps).ToList();

        ushort[] zoneData = reader.ReadUInt16s(numBoxes * 6);
        _level.Zones = TR1BoxUtilities.ReadZones(numBoxes, zoneData);

        reader.ReadUInt32(); // Total count of ushorts
        ushort numGroups = reader.ReadUInt16();
        _level.AnimatedTextures = new();
        for (int i = 0; i < numGroups; i++)
        {
            _level.AnimatedTextures.Add(TR2FileReadUtilities.ReadAnimatedTexture(reader));
        }

        //Entities
        uint numEntities = reader.ReadUInt32();
        _level.Entities = reader.ReadTR1Entities(numEntities);

        _level.LightMap = new(reader.ReadBytes(TRConsts.LightMapSize));
        _level.Palette = reader.ReadColours(TRConsts.PaletteSize);

        //Cinematic Frames
        ushort numCinematicFrames = reader.ReadUInt16();
        _level.CinematicFrames = new();
        for (int i = 0; i < numCinematicFrames; i++)
        {
            _level.CinematicFrames.Add(TR2FileReadUtilities.ReadCinematicFrame(reader));
        }

        ushort numDemoData = reader.ReadUInt16();
        _level.DemoData = reader.ReadBytes(numDemoData);

        ReadSoundEffects(reader);
    }

    protected override void Write(TRLevelWriter writer)
    {
        writer.Write((uint)_level.Images8.Count);
        writer.Write(_level.Images8);

        writer.Write(_level.Version.LevelNumber);

        writer.Write((ushort)_level.Rooms.Count);
        foreach (TRRoom room in _level.Rooms) { writer.Write(room.Serialize()); }

        writer.Write((uint)_level.FloorData.Count);
        writer.Write(_level.FloorData);

        WriteMeshData(writer);

        writer.Write((uint)_level.Animations.Count);
        foreach (TRAnimation anim in _level.Animations) { writer.Write(anim.Serialize()); }
        writer.Write((uint)_level.StateChanges.Count);
        foreach (TRStateChange statec in _level.StateChanges) { writer.Write(statec.Serialize()); }
        writer.Write((uint)_level.AnimDispatches.Count);
        foreach (TRAnimDispatch dispatch in _level.AnimDispatches) { writer.Write(dispatch.Serialize()); }
        writer.Write((uint)_level.AnimCommands.Count);
        foreach (TRAnimCommand cmd in _level.AnimCommands) { writer.Write(cmd.Serialize()); }
        writer.Write((uint)(_level.MeshTrees.Count * 4)); //To get the correct number /= 4 is done during read, make sure to reverse it here.
        foreach (TRMeshTreeNode node in _level.MeshTrees) { writer.Write(node.Serialize()); }
        writer.Write((uint)_level.Frames.Count);
        foreach (ushort frame in _level.Frames) { writer.Write(frame); }

        writer.Write((uint)_level.Models.Count);
        foreach (TRModel model in _level.Models) { writer.Write(model.Serialize()); }
        writer.Write((uint)_level.StaticMeshes.Count);
        foreach (TRStaticMesh mesh in _level.StaticMeshes) { writer.Write(mesh.Serialize()); }

        writer.Write((uint)_level.ObjectTextures.Count);
        foreach (TRObjectTexture tex in _level.ObjectTextures) { writer.Write(tex.Serialize()); }
        writer.Write((uint)_level.SpriteTextures.Count);
        foreach (TRSpriteTexture tex in _level.SpriteTextures) { writer.Write(tex.Serialize()); }
        writer.Write((uint)_level.SpriteSequences.Count);
        foreach (TRSpriteSequence sequence in _level.SpriteSequences) { writer.Write(sequence.Serialize()); }

        writer.Write((uint)_level.Cameras.Count);
        foreach (TRCamera cam in _level.Cameras) { writer.Write(cam.Serialize()); }

        writer.Write((uint)_level.SoundSources.Count);
        foreach (TRSoundSource src in _level.SoundSources) { writer.Write(src.Serialize()); }

        writer.Write((uint)_level.Boxes.Count);
        foreach (TRBox box in _level.Boxes) { writer.Write(box.Serialize()); }
        writer.Write((uint)_level.Overlaps.Count);
        writer.Write(_level.Overlaps);
        writer.Write(TR1BoxUtilities.FlattenZones(_level.Zones));

        byte[] animTextureData = _level.AnimatedTextures.SelectMany(a => a.Serialize()).ToArray();
        writer.Write((uint)(animTextureData.Length / sizeof(ushort)) + 1);
        writer.Write((ushort)_level.AnimatedTextures.Count);
        writer.Write(animTextureData);

        writer.Write((uint)_level.Entities.Count);
        writer.Write(_level.Entities);

        Debug.Assert(_level.LightMap.Count == TRConsts.LightMapSize);
        Debug.Assert(_level.Palette.Count == TRConsts.PaletteSize);
        writer.Write(_level.LightMap.ToArray());
        writer.Write(_level.Palette);

        writer.Write((ushort)_level.CinematicFrames.Count);
        foreach (TRCinematicFrame cineframe in _level.CinematicFrames) { writer.Write(cineframe.Serialize()); }

        writer.Write((ushort)_level.DemoData.Length);
        writer.Write(_level.DemoData);

        WriteSoundEffects(writer);
    }

    private void ReadMeshData(TRLevelReader reader)
    {
        TRObjectMeshBuilder builder = new(TRGameVersion.TR1, _observer);
        builder.BuildObjectMeshes(reader);

        _level.Meshes = builder.Meshes;
        _level.MeshPointers = builder.MeshPointers;
    }

    private void WriteMeshData(TRLevelWriter writer)
    {
        TRObjectMeshBuilder builder = new(TRGameVersion.TR1, _observer);
        builder.WriteObjectMeshes(writer, _level.Meshes, _level.MeshPointers);
    }

    private static TRRoomData ConvertToRoomData(TRRoom room)
    {
        int RoomDataOffset = 0;

        //Grab detailed room data
        TRRoomData RoomData = new()
        {
            //Room vertices
            NumVertices = UnsafeConversions.UShortToShort(room.Data[RoomDataOffset])
        };
        RoomData.Vertices = new TRRoomVertex[RoomData.NumVertices];

        RoomDataOffset++;

        for (int j = 0; j < RoomData.NumVertices; j++)
        {
            TRRoomVertex vertex = new()
            {
                Vertex = new TRVertex()
            };

            vertex.Vertex.X = UnsafeConversions.UShortToShort(room.Data[RoomDataOffset]);
            RoomDataOffset++;
            vertex.Vertex.Y = UnsafeConversions.UShortToShort(room.Data[RoomDataOffset]);
            RoomDataOffset++;
            vertex.Vertex.Z = UnsafeConversions.UShortToShort(room.Data[RoomDataOffset]);
            RoomDataOffset++;
            vertex.Lighting = UnsafeConversions.UShortToShort(room.Data[RoomDataOffset]);
            RoomDataOffset++;

            RoomData.Vertices[j] = vertex;
        }

        //Room rectangles
        RoomData.NumRectangles = UnsafeConversions.UShortToShort(room.Data[RoomDataOffset]);
        RoomData.Rectangles = new TRFace4[RoomData.NumRectangles];

        RoomDataOffset++;

        for (int j = 0; j < RoomData.NumRectangles; j++)
        {
            TRFace4 face = new()
            {
                Vertices = new ushort[4]
            };
            face.Vertices[0] = room.Data[RoomDataOffset];
            RoomDataOffset++;
            face.Vertices[1] = room.Data[RoomDataOffset];
            RoomDataOffset++;
            face.Vertices[2] = room.Data[RoomDataOffset];
            RoomDataOffset++;
            face.Vertices[3] = room.Data[RoomDataOffset];
            RoomDataOffset++;
            face.Texture = room.Data[RoomDataOffset];
            RoomDataOffset++;

            RoomData.Rectangles[j] = face;
        }

        //Room triangles
        RoomData.NumTriangles = UnsafeConversions.UShortToShort(room.Data[RoomDataOffset]);
        RoomData.Triangles = new TRFace3[RoomData.NumTriangles];

        RoomDataOffset++;

        for (int j = 0; j < RoomData.NumTriangles; j++)
        {
            TRFace3 face = new()
            {
                Vertices = new ushort[3]
            };
            face.Vertices[0] = room.Data[RoomDataOffset];
            RoomDataOffset++;
            face.Vertices[1] = room.Data[RoomDataOffset];
            RoomDataOffset++;
            face.Vertices[2] = room.Data[RoomDataOffset];
            RoomDataOffset++;
            face.Texture = room.Data[RoomDataOffset];
            RoomDataOffset++;

            RoomData.Triangles[j] = face;
        }

        //Room sprites
        RoomData.NumSprites = UnsafeConversions.UShortToShort(room.Data[RoomDataOffset]);
        RoomData.Sprites = new TRRoomSprite[RoomData.NumSprites];

        RoomDataOffset++;

        for (int j = 0; j < RoomData.NumSprites; j++)
        {
            TRRoomSprite face = new()
            {
                Vertex = UnsafeConversions.UShortToShort(room.Data[RoomDataOffset])
            };
            RoomDataOffset++;
            face.Texture = UnsafeConversions.UShortToShort(room.Data[RoomDataOffset]);
            RoomDataOffset++;

            RoomData.Sprites[j] = face;
        }

        Debug.Assert(RoomDataOffset == room.NumDataWords);

        return RoomData;
    }

    private void ReadSoundEffects(TRLevelReader reader)
    {
        _level.SoundEffects = new();
        short[] soundMap = reader.ReadInt16s(Enum.GetValues<TR1SFX>().Length);

        uint numSoundDetails = reader.ReadUInt32();
        List<TR1SoundEffect> sfx = new();

        Dictionary<int, ushort> sampleMap = new();
        for (int i = 0; i < numSoundDetails; i++)
        {
            sampleMap[i] = reader.ReadUInt16();
            sfx.Add(new()
            {
                Volume = reader.ReadUInt16(),
                Chance = reader.ReadUInt16(),
                Samples = new()
            });

            sfx[i].SetFlags(reader.ReadUInt16());
        }

        uint numSamples = reader.ReadUInt32();
        byte[] allSamples = reader.ReadUInt8s(numSamples);

        uint numSampleIndices = reader.ReadUInt32();
        uint[] sampleIndices = reader.ReadUInt32s(numSampleIndices);

        foreach (int soundID in sampleMap.Keys)
        {
            TR1SoundEffect effect = sfx[soundID];
            ushort baseIndex = sampleMap[soundID];
            for (int i = 0; i < effect.Samples.Capacity; i++)
            {
                uint start = sampleIndices[baseIndex + i];
                uint end = baseIndex + i + 1 == sampleIndices.Length
                    ? (uint)allSamples.Length
                    : sampleIndices[baseIndex + i + 1];

                byte[] sample = new byte[end - start];
                for (int j = 0; j < sample.Length; j++)
                {
                    sample[j] = allSamples[start + j];
                }

                effect.Samples.Add(sample);
            }
        }

        for (int i = 0; i < soundMap.Length; i++)
        {
            if (soundMap[i] == -1)
            {
                continue;
            }

            _level.SoundEffects[(TR1SFX)i] = sfx[soundMap[i]];
        }
    }

    private void WriteSoundEffects(TRLevelWriter writer)
    {
        short detailsIndex = 0;
        List<uint> samplePointers = new();
        List<byte> wavData = new();

        foreach (TR1SFX id in Enum.GetValues<TR1SFX>())
        {
            writer.Write(_level.SoundEffects.ContainsKey(id) ? detailsIndex++ : (short)-1);
        }

        writer.Write((uint)_level.SoundEffects.Count);
        foreach (var (_, effect) in _level.SoundEffects)
        {
            writer.Write((ushort)samplePointers.Count);
            writer.Write(effect.Volume);
            writer.Write(effect.Chance);
            writer.Write(effect.GetFlags());

            foreach (byte[] wav in effect.Samples)
            {
                samplePointers.Add((uint)wavData.Count);
                wavData.AddRange(wav);
            }
        }

        writer.Write((uint)wavData.Count);
        writer.Write(wavData);

        writer.Write((uint)samplePointers.Count);
        writer.Write(samplePointers);
    }
}
