﻿using Newtonsoft.Json;
using TRLevelControl;
using TRLevelControl.Model;

namespace TRDataControl.Environment;

public class EMImportRoomFunction : BaseEMRoomImportFunction, ITextureModifier
{
    public byte RoomNumber { get; set; }
    public EMLocation NewLocation { get; set; }
    public EMLocation LinkedLocation { get; set; }
    public ushort RectangleTexture { get; set; }
    public ushort TriangleTexture { get; set; }
    public bool PreservePortals { get; set; }
    public bool PreserveBoxes { get; set; }

    public EMImportRoomFunction()
    {
        RectangleTexture = TriangleTexture = ushort.MaxValue;
    }

    public override void ApplyToLevel(TR1Level level)
    {
        // Not yet implemented, plan is to rework this class to read level files instead of JSON
        throw new NotImplementedException();
    }

    public override void ApplyToLevel(TR2Level level)
    {
        Dictionary<byte, EMRoomDefinition<TR2Room>> roomResource = JsonConvert.DeserializeObject<Dictionary<byte, EMRoomDefinition<TR2Room>>>(ReadRoomResource("TR2"), _jsonSettings);
        if (!roomResource.ContainsKey(RoomNumber))
        {
            throw new Exception(string.Format("Missing room {0} in room definition data for {1}.", RoomNumber, LevelID));
        }

        EMRoomDefinition<TR2Room> roomDef = roomResource[RoomNumber];

        int xdiff = NewLocation.X - roomDef.Room.Info.X;
        int ydiff = NewLocation.Y - roomDef.Room.Info.YBottom;
        int zdiff = NewLocation.Z - roomDef.Room.Info.Z;

        TR2Room newRoom = new()
        {
            AlternateRoom = -1,
            AmbientIntensity = roomDef.Room.AmbientIntensity,
            AmbientIntensity2 = roomDef.Room.AmbientIntensity2,
            Flags = roomDef.Room.Flags,
            Info = new()
            {
                X = NewLocation.X,
                YBottom = NewLocation.Y,
                YTop = NewLocation.Y + (roomDef.Room.Info.YTop - roomDef.Room.Info.YBottom),
                Z = NewLocation.Z
            },
            Lights = new(),
            LightMode = roomDef.Room.LightMode,
            NumXSectors = roomDef.Room.NumXSectors,
            NumZSectors = roomDef.Room.NumZSectors,
            Portals = new(),
            Mesh = new()
            {
                Rectangles = new(),
                Triangles = new(),
                Vertices = new(),
                Sprites = new(),
            },
            Sectors = new(),
            StaticMeshes = new()
        };

        if (PreservePortals)
        {
            for (int i = 0; i < roomDef.Room.Portals.Count; i++)
            {
                newRoom.Portals.Add(new()
                {
                    AdjoiningRoom = roomDef.Room.Portals[i].AdjoiningRoom,
                    Normal = roomDef.Room.Portals[i].Normal,
                    Vertices = roomDef.Room.Portals[i].Vertices
                });
            }
        }

        // Lights
        for (int i = 0; i < roomDef.Room.Lights.Count; i++)
        {
            newRoom.Lights.Add(new()
            {
                Fade1 = roomDef.Room.Lights[i].Fade1,
                Fade2 = roomDef.Room.Lights[i].Fade2,
                Intensity1 = roomDef.Room.Lights[i].Intensity1,
                Intensity2 = roomDef.Room.Lights[i].Intensity2,
                X = roomDef.Room.Lights[i].X + xdiff,
                Y = roomDef.Room.Lights[i].Y + ydiff,
                Z = roomDef.Room.Lights[i].Z + zdiff
            });
        }

        // Faces
        for (int i = 0; i < roomDef.Room.Mesh.Rectangles.Count; i++)
        {
            newRoom.Mesh.Rectangles.Add(new()
            {
                Texture = RectangleTexture == ushort.MaxValue ? roomDef.Room.Mesh.Rectangles[i].Texture : RectangleTexture,
                Vertices = new(roomDef.Room.Mesh.Rectangles[i].Vertices)
            });
        }

        for (int i = 0; i < roomDef.Room.Mesh.Triangles.Count; i++)
        {
            newRoom.Mesh.Triangles.Add(new()
            {
                Type = TRFaceType.Triangle,
                Texture = TriangleTexture == ushort.MaxValue ? roomDef.Room.Mesh.Triangles[i].Texture : TriangleTexture,
                Vertices = new(roomDef.Room.Mesh.Triangles[i].Vertices)
            });
        }

        // Vertices
        for (int i = 0; i < roomDef.Room.Mesh.Vertices.Count; i++)
        {
            newRoom.Mesh.Vertices.Add(new()
            {
                Attributes = roomDef.Room.Mesh.Vertices[i].Attributes,
                Lighting = roomDef.Room.Mesh.Vertices[i].Lighting,
                Lighting2 = roomDef.Room.Mesh.Vertices[i].Lighting2,
                Vertex = new()
                {
                    X = roomDef.Room.Mesh.Vertices[i].Vertex.X, // Room coords for X and Z
                    Y = (short)(roomDef.Room.Mesh.Vertices[i].Vertex.Y + ydiff),
                    Z = roomDef.Room.Mesh.Vertices[i].Vertex.Z
                }
            });
        }

        // Sprites
        for (int i = 0; i < roomDef.Room.Mesh.Sprites.Count; i++)
        {
            newRoom.Mesh.Sprites.Add(new()
            {
                ID = roomDef.Room.Mesh.Sprites[i].ID,
                Vertex = roomDef.Room.Mesh.Sprites[i].Vertex
            });
        }

        // Static Meshes
        for (int i = 0; i < roomDef.Room.StaticMeshes.Count; i++)
        {
            newRoom.StaticMeshes.Add(new()
            {
                Intensity1 = roomDef.Room.StaticMeshes[i].Intensity1,
                Intensity2 = roomDef.Room.StaticMeshes[i].Intensity2,
                ID = roomDef.Room.StaticMeshes[i].ID,
                Angle = roomDef.Room.StaticMeshes[i].Angle,
                X = roomDef.Room.StaticMeshes[i].X + xdiff,
                Y = roomDef.Room.StaticMeshes[i].Y + ydiff,
                Z = roomDef.Room.StaticMeshes[i].Z + zdiff
            });
        }

        // Boxes, zones and sectors
        EMLevelData data = GetData(level);

        ushort newBoxIndex = ushort.MaxValue;
        // Duplicate the zone for the new box and link the current box to the new room
        if (!PreserveBoxes)
        {
            TRRoomSector linkedSector = level.GetRoomSector(data.ConvertLocation(LinkedLocation));
            newBoxIndex = (ushort)level.Boxes.Count;
            int linkedBoxIndex = linkedSector.BoxIndex;

            TRBox linkedBox = level.Boxes[linkedBoxIndex];
            linkedBox.Overlaps.Add(newBoxIndex);

            // Make a new box for the new room
            uint xmin = (uint)(newRoom.Info.X + TRConsts.Step4);
            uint zmin = (uint)(newRoom.Info.Z + TRConsts.Step4);
            uint xmax = (uint)(xmin + (newRoom.NumXSectors - 2) * TRConsts.Step4);
            uint zmax = (uint)(zmin + (newRoom.NumZSectors - 2) * TRConsts.Step4);
            TRBox box = new()
            {
                XMin = xmin,
                ZMin = zmin,
                XMax = xmax,
                ZMax = zmax,
                TrueFloor = (short)newRoom.Info.YBottom,
                Zone = linkedBox.Zone.Clone(),
            };            
            level.Boxes.Add(box);

            // Link the box to the room we're joining to
            box.Overlaps.Add((ushort)linkedBoxIndex);
        }

        for (int i = 0; i < roomDef.Room.Sectors.Count; i++)
        {
            int sectorYDiff = 0;
            ushort sectorBoxIndex = roomDef.Room.Sectors[i].BoxIndex;
            // Only change the sector if it's not impenetrable and we don't want to preserve the existing zoning
            if (roomDef.Room.Sectors[i].Ceiling != TRConsts.WallClicks || roomDef.Room.Sectors[i].Floor != TRConsts.WallClicks)
            {
                sectorYDiff = ydiff / TRConsts.Step1;
                if (!PreserveBoxes)
                {
                    sectorBoxIndex = newBoxIndex;
                }
            }

            newRoom.Sectors.Add(new()
            {
                BoxIndex = sectorBoxIndex,
                Ceiling = (sbyte)(roomDef.Room.Sectors[i].Ceiling + sectorYDiff),
                FDIndex = 0, // Initialise to no FD
                Floor = (sbyte)(roomDef.Room.Sectors[i].Floor + sectorYDiff),
                RoomAbove = PreservePortals ? roomDef.Room.Sectors[i].RoomAbove : (byte)TRConsts.NoRoom,
                RoomBelow = PreservePortals ? roomDef.Room.Sectors[i].RoomBelow : (byte)TRConsts.NoRoom
            });

            // Duplicate the FD too for everything except triggers. Track any portals
            // so they can be blocked off.
            ushort fdIndex = roomDef.Room.Sectors[i].FDIndex;
            if (roomDef.FloorData.ContainsKey(fdIndex))
            {
                List<FDEntry> entries = roomDef.FloorData[fdIndex];
                List<FDEntry> newEntries = new();
                foreach (FDEntry entry in entries)
                {
                    switch (entry)
                    {
                        case FDPortalEntry:
                            // This portal will no longer be valid in the new room's position,
                            // so block off the wall
                            newRoom.Sectors[i].Floor = newRoom.Sectors[i].Ceiling = TRConsts.WallClicks;
                            break;
                        case FDTriggerEntry:
                            break;
                        default:
                            newEntries.Add(entry.Clone());
                            break;
                    }
                }

                if (newEntries.Count > 0)
                {
                    level.FloorData.CreateFloorData(newRoom.Sectors[i]);
                    level.FloorData[newRoom.Sectors[i].FDIndex].AddRange(newEntries);
                }
            }
        }

        level.Rooms.Add(newRoom);
    }

    public override void ApplyToLevel(TR3Level level)
    {
        throw new NotImplementedException();
    }

    public void RemapTextures(Dictionary<ushort, ushort> indexMap)
    {
        if (indexMap.ContainsKey(RectangleTexture))
        {
            RectangleTexture = indexMap[RectangleTexture];
        }
        if (indexMap.ContainsKey(TriangleTexture))
        {
            TriangleTexture = indexMap[TriangleTexture];
        }
    }
}
