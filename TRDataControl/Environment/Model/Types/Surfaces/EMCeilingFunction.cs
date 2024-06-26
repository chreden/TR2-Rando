﻿using TRLevelControl;
using TRLevelControl.Model;

namespace TRDataControl.Environment;

public class EMCeilingFunction : BaseEMFunction
{
    public Dictionary<int, sbyte> CeilingHeights { get; set; }
    public bool AmendVertices { get; set; }

    public override void ApplyToLevel(TR1Level level)
    {
        EMLevelData data = GetData(level);
        foreach (int roomNumber in CeilingHeights.Keys)
        {
            TR1Room room = level.Rooms[data.ConvertRoom(roomNumber)];
            int oldYTop = room.Info.YTop;
            int min = int.MaxValue;
            foreach (TRRoomSector sector in room.Sectors)
            {
                if (!sector.IsWall)
                {
                    sector.Ceiling = CeilingHeights[roomNumber];
                    min = Math.Min(min, sector.Ceiling);
                }
            }
            room.Info.YTop = min * TRConsts.Step1;

            if (AmendVertices)
            {
                foreach (TR1RoomVertex vertex in room.Mesh.Vertices)
                {
                    if (vertex.Vertex.Y == oldYTop)
                    {
                        vertex.Vertex.Y = (short)room.Info.YTop;
                    }
                }
            }
        }
    }

    public override void ApplyToLevel(TR2Level level)
    {
        EMLevelData data = GetData(level);
        foreach (int roomNumber in CeilingHeights.Keys)
        {
            TR2Room room = level.Rooms[data.ConvertRoom(roomNumber)];
            int oldYTop = room.Info.YTop;
            int min = int.MaxValue;
            foreach (TRRoomSector sector in room.Sectors)
            {
                if (!sector.IsWall)
                {
                    sector.Ceiling = CeilingHeights[roomNumber];
                    min = Math.Min(min, sector.Ceiling);
                }
            }
            room.Info.YTop = min * TRConsts.Step1;

            if (AmendVertices)
            {
                foreach (TR2RoomVertex vertex in room.Mesh.Vertices)
                {
                    if (vertex.Vertex.Y == oldYTop)
                    {
                        vertex.Vertex.Y = (short)room.Info.YTop;
                    }
                }
            }
        }
    }

    public override void ApplyToLevel(TR3Level level)
    {
        EMLevelData data = GetData(level);
        foreach (int roomNumber in CeilingHeights.Keys)
        {
            TR3Room room = level.Rooms[data.ConvertRoom(roomNumber)];
            int oldYTop = room.Info.YTop;
            int min = int.MaxValue;
            foreach (TRRoomSector sector in room.Sectors)
            {
                if (!sector.IsWall)
                {
                    sector.Ceiling = CeilingHeights[roomNumber];
                    min = Math.Min(min, sector.Ceiling);
                }
            }
            room.Info.YTop = min * TRConsts.Step1;

            if (AmendVertices)
            {
                foreach (TR3RoomVertex vertex in room.Mesh.Vertices)
                {
                    if (vertex.Vertex.Y == oldYTop)
                    {
                        vertex.Vertex.Y = (short)room.Info.YTop;
                    }
                }
            }
        }
    }
}
