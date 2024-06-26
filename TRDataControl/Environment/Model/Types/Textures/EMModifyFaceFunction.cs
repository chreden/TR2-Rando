﻿using TRLevelControl.Model;

namespace TRDataControl.Environment;

public class EMModifyFaceFunction : BaseEMFunction
{
    public EMFaceModification[] Modifications { get; set; }
    public EMFaceRotation[] Rotations { get; set; }

    public override void ApplyToLevel(TR1Level level)
    {
        EMLevelData data = GetData(level);

        if (Modifications != null)
        {
            foreach (EMFaceModification mod in Modifications)
            {
                TR1Room room = level.Rooms[data.ConvertRoom(mod.RoomNumber)];
                switch (mod.FaceType)
                {
                    case EMTextureFaceType.Rectangles:
                        ModifyRectangles(room, mod);
                        break;
                    case EMTextureFaceType.Triangles:
                        ModifyTriangles(room, mod);
                        break;
                }
            }
        }

        if (Rotations != null)
        {
            foreach (EMFaceRotation rot in Rotations)
            {
                TR1Room room = level.Rooms[data.ConvertRoom(rot.RoomNumber)];
                switch (rot.FaceType)
                {
                    case EMTextureFaceType.Rectangles:
                        RotateRectangles(room.Mesh.Rectangles, rot);
                        break;
                    case EMTextureFaceType.Triangles:
                        RotateTriangles(room.Mesh.Triangles, rot);
                        break;
                }
            }
        }
    }

    public override void ApplyToLevel(TR2Level level)
    {
        EMLevelData data = GetData(level);

        if (Modifications != null)
        {
            foreach (EMFaceModification mod in Modifications)
            {
                TR2Room room = level.Rooms[data.ConvertRoom(mod.RoomNumber)];
                switch (mod.FaceType)
                {
                    case EMTextureFaceType.Rectangles:
                        ModifyRectangles(room, mod);
                        break;
                    case EMTextureFaceType.Triangles:
                        ModifyTriangles(room, mod);
                        break;
                }
            }
        }

        if (Rotations != null)
        {
            foreach (EMFaceRotation rot in Rotations)
            {
                TR2Room room = level.Rooms[data.ConvertRoom(rot.RoomNumber)];
                switch (rot.FaceType)
                {
                    case EMTextureFaceType.Rectangles:
                        RotateRectangles(room.Mesh.Rectangles, rot);
                        break;
                    case EMTextureFaceType.Triangles:
                        RotateTriangles(room.Mesh.Triangles, rot);
                        break;
                }
            }
        }
    }

    public override void ApplyToLevel(TR3Level level)
    {
        EMLevelData data = GetData(level);

        if (Modifications != null)
        {
            foreach (EMFaceModification mod in Modifications)
            {
                TR3Room room = level.Rooms[data.ConvertRoom(mod.RoomNumber)];
                switch (mod.FaceType)
                {
                    case EMTextureFaceType.Rectangles:
                        ModifyRectangles(room, mod);
                        break;
                    case EMTextureFaceType.Triangles:
                        ModifyTriangles(room, mod);
                        break;
                }
            }
        }

        if (Rotations != null)
        {
            foreach (EMFaceRotation rot in Rotations)
            {
                TR3Room room = level.Rooms[data.ConvertRoom(rot.RoomNumber)];
                switch (rot.FaceType)
                {
                    case EMTextureFaceType.Rectangles:
                        RotateRectangles(room.Mesh.Rectangles, rot);
                        break;
                    case EMTextureFaceType.Triangles:
                        RotateTriangles(room.Mesh.Triangles, rot);
                        break;
                }
            }
        }
    }

    private static void ModifyRectangles(TR1Room room, EMFaceModification mod)
    {
        foreach (int faceIndex in mod.GetIndices())
        {
            TRFace rect = room.Mesh.Rectangles[faceIndex];
            foreach (int vertIndex in mod.VertexChanges.Keys)
            {
                TR1RoomVertex currentRoomVertex = room.Mesh.Vertices[rect.Vertices[vertIndex]];
                TRVertex newVertex = mod.VertexChanges[vertIndex];
                TR1RoomVertex newRoomVertex = GenerateRoomVertex(currentRoomVertex, newVertex);

                // Remap the face to use this vertex
                rect.Vertices[vertIndex] = (ushort)room.Mesh.Vertices.Count;
                room.Mesh.Vertices.Add(newRoomVertex);
            }
        }
    }

    private static void ModifyRectangles(TR2Room room, EMFaceModification mod)
    {
        foreach (int faceIndex in mod.GetIndices())
        {
            TRFace rect = room.Mesh.Rectangles[faceIndex];
            foreach (int vertIndex in mod.VertexChanges.Keys)
            {
                TR2RoomVertex currentRoomVertex = room.Mesh.Vertices[rect.Vertices[vertIndex]];
                TRVertex newVertex = mod.VertexChanges[vertIndex];
                TR2RoomVertex newRoomVertex = GenerateRoomVertex(currentRoomVertex, newVertex);

                // Remap the face to use this vertex
                rect.Vertices[vertIndex] = (ushort)room.Mesh.Vertices.Count;
                room.Mesh.Vertices.Add(newRoomVertex);
            }
        }
    }

    private static void ModifyRectangles(TR3Room room, EMFaceModification mod)
    {
        foreach (int faceIndex in mod.GetIndices())
        {
            TRFace rect = room.Mesh.Rectangles[faceIndex];
            foreach (int vertIndex in mod.VertexChanges.Keys)
            {
                TR3RoomVertex currentRoomVertex = room.Mesh.Vertices[rect.Vertices[vertIndex]];
                TRVertex newVertex = mod.VertexChanges[vertIndex];
                TR3RoomVertex newRoomVertex = GenerateRoomVertex(currentRoomVertex, newVertex);

                // Remap the face to use this vertex
                rect.Vertices[vertIndex] = (ushort)room.Mesh.Vertices.Count;
                room.Mesh.Vertices.Add(newRoomVertex);
            }
        }
    }

    private static void ModifyTriangles(TR1Room room, EMFaceModification mod)
    {
        foreach (int faceIndex in mod.GetIndices())
        {
            TRFace tri = room.Mesh.Triangles[faceIndex];
            foreach (int vertIndex in mod.VertexChanges.Keys)
            {
                TR1RoomVertex currentRoomVertex = room.Mesh.Vertices[tri.Vertices[vertIndex]];
                TRVertex newVertex = mod.VertexChanges[vertIndex];
                TR1RoomVertex newRoomVertex = GenerateRoomVertex(currentRoomVertex, newVertex);

                // Remap the face to use this vertex
                tri.Vertices[vertIndex] = (ushort)room.Mesh.Vertices.Count;
                room.Mesh.Vertices.Add(newRoomVertex);
            }
        }
    }

    private static void ModifyTriangles(TR2Room room, EMFaceModification mod)
    {
        foreach (int faceIndex in mod.GetIndices())
        {
            TRFace tri = room.Mesh.Triangles[faceIndex];
            foreach (int vertIndex in mod.VertexChanges.Keys)
            {
                TR2RoomVertex currentRoomVertex = room.Mesh.Vertices[tri.Vertices[vertIndex]];
                TRVertex newVertex = mod.VertexChanges[vertIndex];
                TR2RoomVertex newRoomVertex = GenerateRoomVertex(currentRoomVertex, newVertex);

                // Remap the face to use this vertex
                tri.Vertices[vertIndex] = (ushort)room.Mesh.Vertices.Count;
                room.Mesh.Vertices.Add(newRoomVertex);
            }
        }
    }

    private static void ModifyTriangles(TR3Room room, EMFaceModification mod)
    {
        foreach (int faceIndex in mod.GetIndices())
        {
            TRFace tri = room.Mesh.Triangles[faceIndex];
            foreach (int vertIndex in mod.VertexChanges.Keys)
            {
                TR3RoomVertex currentRoomVertex = room.Mesh.Vertices[tri.Vertices[vertIndex]];
                TRVertex newVertex = mod.VertexChanges[vertIndex];
                TR3RoomVertex newRoomVertex = GenerateRoomVertex(currentRoomVertex, newVertex);

                // Remap the face to use this vertex
                tri.Vertices[vertIndex] = (ushort)room.Mesh.Vertices.Count;
                room.Mesh.Vertices.Add(newRoomVertex);
            }
        }
    }

    private static TR1RoomVertex GenerateRoomVertex(TR1RoomVertex currentRoomVertex, TRVertex newVertex)
    {
        // We create a new vertex because we can't guarantee nothing else is using it.
        // Note the vertex values in the mod are added to the existing values, so we
        // don't have to define those we aren't changing.
        return new()
        {
            Lighting = currentRoomVertex.Lighting,
            Vertex = new()
            {
                X = (short)(currentRoomVertex.Vertex.X + newVertex.X),
                Y = (short)(currentRoomVertex.Vertex.Y + newVertex.Y),
                Z = (short)(currentRoomVertex.Vertex.Z + newVertex.Z)
            }
        };
    }

    private static TR2RoomVertex GenerateRoomVertex(TR2RoomVertex currentRoomVertex, TRVertex newVertex)
    {
        return new()
        {
            Attributes = currentRoomVertex.Attributes,
            Lighting = currentRoomVertex.Lighting,
            Lighting2 = currentRoomVertex.Lighting2,
            Vertex = new()
            {
                X = (short)(currentRoomVertex.Vertex.X + newVertex.X),
                Y = (short)(currentRoomVertex.Vertex.Y + newVertex.Y),
                Z = (short)(currentRoomVertex.Vertex.Z + newVertex.Z)
            }
        };
    }

    private static TR3RoomVertex GenerateRoomVertex(TR3RoomVertex currentRoomVertex, TRVertex newVertex)
    {
        return new()
        {
            Attributes = currentRoomVertex.Attributes,
            Lighting = currentRoomVertex.Lighting,
            Colour = currentRoomVertex.Colour,
            Vertex = new()
            {
                X = (short)(currentRoomVertex.Vertex.X + newVertex.X),
                Y = (short)(currentRoomVertex.Vertex.Y + newVertex.Y),
                Z = (short)(currentRoomVertex.Vertex.Z + newVertex.Z)
            }
        };
    }

    private static void RotateRectangles(List<TRFace> rectangles, EMFaceRotation rot)
    {
        foreach (int rectIndex in rot.FaceIndices)
        {
            RotateVertices(rectangles[rectIndex].Vertices, rot);
        }
    }

    private static void RotateTriangles(List<TRFace> triangles, EMFaceRotation rot)
    {
        foreach (int triIndex in rot.FaceIndices)
        {
            RotateVertices(triangles[triIndex].Vertices, rot);
        }
    }

    public static void RotateVertices(List<ushort> originalVertices, EMFaceRotation rot)
    {
        List<ushort> remappedVertices = new();
        for (int i = 0; i < originalVertices.Count; i++)
        {
            if (rot.VertexRemap.ContainsKey(i))
            {
                remappedVertices.Add(originalVertices[rot.VertexRemap[i]]);
            }
            else
            {
                remappedVertices.Add(originalVertices[i]);
            }
        }

        originalVertices.Clear();
        originalVertices.AddRange(remappedVertices);
    }
}

public class EMFaceModification
{
    public int RoomNumber { get; set; }
    public EMTextureFaceType FaceType { get; set; }
    public int FaceIndex { get; set; }
    public int[] FaceIndices { get; set; }
    public Dictionary<int, TRVertex> VertexChanges { get; set; }

    public int[] GetIndices()
    {
        return FaceIndices ?? new int[] { FaceIndex };
    }
}

public class EMFaceRotation
{
    public int RoomNumber { get; set; }
    public EMTextureFaceType FaceType { get; set; }
    public int[] FaceIndices { get; set; }
    public Dictionary<int, int> VertexRemap { get; set; }
}
