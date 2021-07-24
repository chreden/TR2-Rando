﻿using System.Collections.Generic;
using System.Linq;
using TRFDControl.FDEntryTypes;
using TRLevelReader.Model;

namespace TRFDControl.Utilities
{
    public static class FDUtilities
    {
        public static List<FDTriggerEntry> GetEntityTriggers(FDControl control, int entityIndex)
        {
            List<FDTriggerEntry> entries = new List<FDTriggerEntry>();

            foreach (List<FDEntry> entryList in control.Entries.Values)
            {
                foreach (FDEntry entry in entryList)
                {
                    if (entry is FDTriggerEntry triggerEntry)
                    {
                        int itemIndex = triggerEntry.TrigActionList.FindIndex
                        (
                            i =>
                                i.TrigAction == FDTrigAction.Object && i.Parameter == entityIndex
                        );
                        if (itemIndex != -1)
                        {
                            entries.Add(triggerEntry);
                        }
                    }
                }
            }

            return entries;
        }

        public static List<FDActionListItem> GetActionListItems(FDControl control, FDTrigAction trigAction, int sectorIndex = -1)
        {
            List<FDActionListItem> items = new List<FDActionListItem>();

            List<List<FDEntry>> entrySearch;
            if (sectorIndex == -1)
            {
                entrySearch = control.Entries.Values.ToList();
            }
            else
            {
                entrySearch = new List<List<FDEntry>>
                {
                    control.Entries[sectorIndex]
                };
            }

            foreach (List<FDEntry> entryList in entrySearch)
            {
                foreach (FDEntry entry in entryList)
                {
                    if (entry is FDTriggerEntry triggerEntry)
                    {
                        foreach (FDActionListItem item in triggerEntry.TrigActionList)
                        {
                            if (item.TrigAction == trigAction)
                            {
                                items.Add(item);
                            }
                        }
                    }
                }
            }

            return items;
        }

        public static readonly short NO_ROOM = 0xff;
        public static readonly short WALL_SHIFT = 10;

        // See Control.c line 516
        public static TRRoomSector GetRoomSector(int x, int y, int z, short roomNumber, TR2Level level, FDControl floorData)
        {
            int xFloor, yFloor;
            TR2Room room = level.Rooms[roomNumber];
            TRRoomSector sector;
            short data;

            do
            {
                // Clip position to edge of room (that way doorways are detected)
                xFloor = (z - room.Info.Z) >> WALL_SHIFT;
                yFloor = (x - room.Info.X) >> WALL_SHIFT;

                // Ensure we don't test the corner of a room's floor data, as this can cause problems when 4 rooms join at a point
                if (xFloor <= 0)
                {
                    xFloor = 0;
                    if (yFloor < 1)
                    {
                        yFloor = 1;
                    }
                    else if (yFloor > room.NumXSectors - 2)
                    {
                        yFloor = room.NumXSectors - 2;
                    }
                }
                else if (xFloor >= room.NumZSectors - 1)
                {
                    xFloor = room.NumZSectors - 1;
                    if (yFloor < 1)
                    {
                        yFloor = 1;
                    }
                    else if (yFloor > room.NumXSectors - 2)
                    {
                        yFloor = room.NumXSectors - 2;
                    }
                }
                else if (yFloor < 0)
                {
                    yFloor = 0;
                }
                else if (yFloor >= room.NumXSectors)
                {
                    yFloor = room.NumXSectors - 1;
                }

                // If doorway, go through and retest, else move onto next stage
                sector = room.SectorList[xFloor + yFloor * room.NumZSectors];
                data = GetDoor(sector, floorData);
                if (data != NO_ROOM && data != 0 & data < level.Rooms.Length - 1)
                {
                    room = level.Rooms[data];
                }
            }
            while (data != NO_ROOM);

            // If below floor and pit, go down
            if (y >= (sector.Floor << 8))
            {
                do
                {
                    if (sector.RoomBelow == NO_ROOM)
                    {
                        return sector;
                    }

                    room = level.Rooms[sector.RoomBelow];
                    sector = room.SectorList[((z - room.Info.Z) >> WALL_SHIFT) + ((x - room.Info.X) >> WALL_SHIFT) * room.NumZSectors];
                }
                while (y >= (sector.Floor << 8));
            }
            else if (y < (sector.Ceiling << 8))
            {
                // If above ceiling and skylight, go up
                do
                {
                    if (sector.RoomAbove == NO_ROOM)
                    {
                        return sector;
                    }

                    room = level.Rooms[sector.RoomAbove];
                    sector = room.SectorList[((z - room.Info.Z) >> WALL_SHIFT) + ((x - room.Info.X) >> WALL_SHIFT) * room.NumZSectors];
                }
                while (y < (sector.RoomAbove << 8));
            }

            return sector;
        }

        // See Control.c line 1344
        public static short GetDoor(TRRoomSector sector, FDControl floorData)
        {
            if (sector.FDIndex == 0)
            {
                return NO_ROOM;
            }

            List<FDEntry> entries = floorData.Entries[sector.FDIndex];
            foreach (FDEntry entry in entries)
            {
                if (entry is FDPortalEntry portal)
                {
                    return (short)portal.Room;
                }
            }

            return NO_ROOM;
        }
    }
}