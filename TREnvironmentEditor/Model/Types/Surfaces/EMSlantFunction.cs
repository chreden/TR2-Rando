﻿using System.Collections.Generic;
using System.Linq;
using TREnvironmentEditor.Helpers;
using TRFDControl;
using TRFDControl.FDEntryTypes;
using TRFDControl.Utilities;
using TRLevelReader.Model;

namespace TREnvironmentEditor.Model.Types
{
    public class EMSlantFunction : EMClickFunction
    {
        public FDSlantEntryType SlantType { get; set; }
        public sbyte? XSlant { get; set; }
        public sbyte? ZSlant { get; set; }

        public override void ApplyToLevel(TRLevel level)
        {
            // Apply click changes first
            base.ApplyToLevel(level);

            FDControl floorData = new FDControl();
            floorData.ParseFromLevel(level);

            foreach (EMLocation location in _locations)
            {
                TRRoomSector sector = FDUtilities.GetRoomSector(location.X, location.Y, location.Z, location.Room, level, floorData);
                CreateSlantEntry(sector, floorData);
            }

            floorData.WriteToLevel(level);
        }

        public override void ApplyToLevel(TR2Level level)
        {
            base.ApplyToLevel(level);

            FDControl floorData = new FDControl();
            floorData.ParseFromLevel(level);

            foreach (EMLocation location in _locations)
            {
                TRRoomSector sector = FDUtilities.GetRoomSector(location.X, location.Y, location.Z, location.Room, level, floorData);
                CreateSlantEntry(sector, floorData);
            }

            floorData.WriteToLevel(level);
        }

        public override void ApplyToLevel(TR3Level level)
        {
            base.ApplyToLevel(level);

            FDControl floorData = new FDControl();
            floorData.ParseFromLevel(level);

            foreach (EMLocation location in _locations)
            {
                TRRoomSector sector = FDUtilities.GetRoomSector(location.X, location.Y, location.Z, location.Room, level, floorData);
                CreateSlantEntry(sector, floorData);
            }

            floorData.WriteToLevel(level);
        }

        private void CreateSlantEntry(TRRoomSector sector, FDControl floorData)
        {
            if (sector.FDIndex == 0)
            {
                floorData.CreateFloorData(sector);
            }

            FDSlantEntry newSlant = new FDSlantEntry
            {
                Setup = new FDSetup(SlantType == FDSlantEntryType.FloorSlant ? FDFunctions.FloorSlant : FDFunctions.CeilingSlant),
                Type = SlantType
            };
            if (XSlant.HasValue)
            {
                newSlant.XSlant = XSlant.Value;
            }
            if (ZSlant.HasValue)
            {
                newSlant.ZSlant = ZSlant.Value;
            }

            List<FDEntry> entries = floorData.Entries[sector.FDIndex];

            // Only one slant of each type is supported, and floor must come before ceiling and both before anything else.
            // For ease, remove any existing slants, then re-add/replace as needed.
            FDEntry floorSlant = entries.Find(e => e is FDSlantEntry slant && slant.Type == FDSlantEntryType.FloorSlant);
            FDEntry ceilingSlant = entries.Find(e => e is FDSlantEntry slant && slant.Type == FDSlantEntryType.CeilingSlant);

            if (floorSlant != null)
            {
                entries.Remove(floorSlant);
            }
            if (ceilingSlant != null)
            {
                entries.Remove(ceilingSlant);
            }

            if (SlantType == FDSlantEntryType.FloorSlant)
            {
                floorSlant = newSlant;
            }
            else
            {
                ceilingSlant = newSlant;
            }

            if (ceilingSlant != null)
            {
                entries.Insert(0, ceilingSlant);
            }
            if (floorSlant != null)
            {
                entries.Insert(0, floorSlant);
            }
        }

        protected override int GetEntityYShift(int clicks)
        {
            List<sbyte> corners = new List<sbyte> { 0, 0, 0, 0 };
            if (XSlant.HasValue && XSlant > 0)
            {
                corners[0] += XSlant.Value;
                corners[1] += XSlant.Value;
            }
            else if (XSlant.HasValue && XSlant < 0)
            {
                corners[2] -= XSlant.Value;
                corners[3] -= XSlant.Value;
            }

            if (ZSlant.HasValue && ZSlant > 0)
            {
                corners[0] += ZSlant.Value;
                corners[2] += ZSlant.Value;
            }
            else if (ZSlant.HasValue && ZSlant < 0)
            {
                corners[1] -= ZSlant.Value;
                corners[3] -= ZSlant.Value;
            }

            // Half-way down the slope
            return (clicks * 256) + (corners.Max() - corners.Min()) * 256 / 2;
        }
    }
}