﻿using System.Collections.Generic;
using System.Linq;
using TREnvironmentEditor.Helpers;
using TRFDControl;
using TRFDControl.FDEntryTypes;
using TRFDControl.Utilities;
using TRLevelReader.Model;

namespace TREnvironmentEditor.Model.Types
{
    public class EMConvertTriggerFunction : BaseEMFunction
    {
        public EMLocation Location { get; set; }
        public FDTrigType? TrigType { get; set; }
        public bool? OneShot { get; set; }

        public override void ApplyToLevel(TR2Level level)
        {
            FDControl control = new FDControl();
            control.ParseFromLevel(level);

            TRRoomSector sector = FDUtilities.GetRoomSector(Location.X, Location.Y, Location.Z, (short)ConvertItemNumber(Location.Room, level.NumRooms), level, control);
            ConvertTrigger(sector, control);

            control.WriteToLevel(level);
        }

        public override void ApplyToLevel(TR3Level level)
        {
            FDControl control = new FDControl();
            control.ParseFromLevel(level);

            TRRoomSector sector = FDUtilities.GetRoomSector(Location.X, Location.Y, Location.Z, (short)ConvertItemNumber(Location.Room, level.NumRooms), level, control);
            ConvertTrigger(sector, control);

            control.WriteToLevel(level);
        }

        private void ConvertTrigger(TRRoomSector sector, FDControl floorData)
        {
            if (sector.FDIndex != 0)
            {
                IEnumerable<FDTriggerEntry> triggers = floorData.Entries[sector.FDIndex].FindAll(e => e is FDTriggerEntry).Cast<FDTriggerEntry>();
                foreach (FDTriggerEntry trigger in triggers)
                {
                    if (TrigType.HasValue)
                    {
                        trigger.TrigType = TrigType.Value;
                    }
                    if (OneShot.HasValue)
                    {
                        trigger.TrigSetup.OneShot = OneShot.Value;
                    }
                }
            }
        }
    }
}