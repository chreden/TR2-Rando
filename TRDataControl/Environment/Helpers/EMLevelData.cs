﻿using TRLevelControl.Model;

namespace TRDataControl.Environment;

public class EMLevelData
{
    public int NumCameras { get; set; }
    public int NumEntities { get; set; }
    public int NumRooms { get; set; }

    /// <summary>
    /// Negative values will imply a backwards search against NumCameras.
    /// e.g. camera = -2, NumCameras = 14 => Result = 12
    /// </summary>
    public short ConvertCamera(int camera)
    {
        return Convert(camera, NumCameras);
    }

    /// <summary>
    /// Negative values will imply a backwards search against NumEntities.
    /// e.g. entity = -2, NumEntities = 14 => Result = 12
    /// </summary>
    public short ConvertEntity(int entity)
    {
        return Convert(entity, NumEntities);
    }

    /// <summary>
    /// Negative values will imply a backwards search against NumRoows.
    /// e.g. room = -2, NumRooms = 14 => Result = 12
    /// </summary>
    public short ConvertRoom(int room)
    {
        return Convert(room, NumRooms);
    }

    public EMLocation ConvertLocation(EMLocation location)
    {
        return location.Room >= 0 ? location : new()
        {
            X = location.X,
            Y = location.Y,
            Z = location.Z,
            Room = ConvertRoom(location.Room),
            Angle = location.Angle
        };
    }

    public static short Convert(int itemIndex, long numItems)
    {
        return (short)(itemIndex < 0 ? numItems + itemIndex : itemIndex);
    }

    public static EMLevelData GetData(TR1Level level)
    {
        return new()
        {
            NumCameras = level.Cameras.Count,
            NumEntities = level.Entities.Count,
            NumRooms = level.Rooms.Count
        };
    }

    public static EMLevelData GetData(TR2Level level)
    {
        return new()
        {
            NumCameras = level.Cameras.Count,
            NumEntities = level.Entities.Count,
            NumRooms = level.Rooms.Count
        };
    }

    public static EMLevelData GetData(TR3Level level)
    {
        return new()
        {
            NumCameras = level.Cameras.Count,
            NumEntities = level.Entities.Count,
            NumRooms = level.Rooms.Count
        };
    }
}
