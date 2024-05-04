﻿using TRLevelControl.Model;

namespace TRDataControl.Environment;

public class EMVisibilityPortal
{
    public short BaseRoom { get; set; }
    public short AdjoiningRoom { get; set; }
    public TRVertex Normal { get; set; }
    public List<TRVertex> Vertices { get; set; }

    public TRRoomPortal ToPortal(EMLevelData levelData)
    {
        BaseRoom = (short)(BaseRoom < 0 ? levelData.NumRooms + BaseRoom : BaseRoom);
        return new()
        {
            AdjoiningRoom = (ushort)(AdjoiningRoom < 0 ? levelData.NumRooms + AdjoiningRoom : AdjoiningRoom),
            Normal = Normal,
            Vertices = Vertices
        };
    }

    public static EMVisibilityPortal FromPortal(short baseRoom, TRRoomPortal portal)
    {
        return new()
        {
            BaseRoom = baseRoom,
            AdjoiningRoom = (short)portal.AdjoiningRoom,
            Normal = portal.Normal,
            Vertices = portal.Vertices
        };
    }
}
