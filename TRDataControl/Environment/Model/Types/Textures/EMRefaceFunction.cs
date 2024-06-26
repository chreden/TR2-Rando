﻿using TRLevelControl.Model;

namespace TRDataControl.Environment;

public class EMRefaceFunction : BaseEMFunction, ITextureModifier
{
    public EMTextureMap TextureMap { get; set; }

    public override void ApplyToLevel(TR1Level level)
    {
        EMLevelData data = GetData(level);

        foreach (ushort texture in TextureMap.Keys)
        {
            foreach (int roomIndex in TextureMap[texture].Keys)
            {
                TR1Room room = level.Rooms[data.ConvertRoom(roomIndex)];
                ApplyTextures(texture, TextureMap[texture][roomIndex], room.Mesh.Rectangles, room.Mesh.Triangles);
            }
        }
    }

    public override void ApplyToLevel(TR2Level level)
    {
        EMLevelData data = GetData(level);

        foreach (ushort texture in TextureMap.Keys)
        {
            foreach (int roomIndex in TextureMap[texture].Keys)
            {
                TR2Room room = level.Rooms[data.ConvertRoom(roomIndex)];
                ApplyTextures(texture, TextureMap[texture][roomIndex], room.Mesh.Rectangles, room.Mesh.Triangles);
            }
        }
    }

    public override void ApplyToLevel(TR3Level level)
    {
        EMLevelData data = GetData(level);

        foreach (ushort texture in TextureMap.Keys)
        {
            foreach (int roomIndex in TextureMap[texture].Keys)
            {
                TR3Room room = level.Rooms[data.ConvertRoom(roomIndex)];
                ApplyTextures(texture, TextureMap[texture][roomIndex], room.Mesh.Rectangles, room.Mesh.Triangles);
            }
        }
    }

    private static void ApplyTextures(ushort texture, Dictionary<EMTextureFaceType, int[]> faceMap, List<TRFace> rectangles, List<TRFace> triangles)
    {
        foreach (EMTextureFaceType faceType in faceMap.Keys)
        {
            foreach (int faceIndex in faceMap[faceType])
            {
                switch (faceType)
                {
                    case EMTextureFaceType.Rectangles:
                        rectangles[faceIndex].Texture = texture;
                        break;
                    case EMTextureFaceType.Triangles:
                        triangles[faceIndex].Texture = texture;
                        break;
                }
            }
        }
    }

    public void RemapTextures(Dictionary<ushort, ushort> indexMap)
    {
        TextureMap.Remap(indexMap);
    }
}
