﻿using System.Collections.Generic;
using System.Linq;
using TREnvironmentEditor.Helpers;
using TRLevelReader.Model;
using TRLevelReader.Model.Enums;

namespace TREnvironmentEditor.Model.Types
{
    public class EMAdjustEntityPositionFunction : BaseEMFunction
    {
        public TR2Entities EntityType { get; set; }
        public Dictionary<int, Dictionary<short, EMLocation>> RoomMap { get; set; }

        public override void ApplyToLevel(TR2Level level)
        {
            // Example use case is rotating wall blades, which need various different angles across the levels after mirroring.
            // X, Y, Z in the target relocation will be relative to the current location; the angle will be the new angle.

            List<TR2Entity> entities = level.Entities.ToList().FindAll(e => (TR2Entities)e.TypeID == EntityType);
            foreach (int roomNumber in RoomMap.Keys)
            {
                foreach (short currentAngle in RoomMap[roomNumber].Keys)
                {
                    EMLocation relocation = RoomMap[roomNumber][currentAngle];
                    List<TR2Entity> matchingEntities = entities.FindAll(e => e.Room == roomNumber && e.Angle == currentAngle);
                    foreach (TR2Entity match in matchingEntities)
                    {
                        match.X += relocation.X;
                        match.Y += relocation.Y;
                        match.Z += relocation.Z;
                        match.Angle = relocation.Angle;
                    }
                }
            }
        }
    }
}