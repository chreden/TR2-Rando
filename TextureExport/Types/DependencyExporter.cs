﻿using Newtonsoft.Json;
using TRLevelControl.Model;
using TRModelTransporter.Model.Textures;

namespace TextureExport.Types;

public static class DependencyExporter
{
    public static void Export(TR1Level level, string lvl)
    {
        TR1TextureRemapGroup remapGroup = new();
        foreach (TR1Type type in level.Models.Keys)
        {
            remapGroup.CalculateDependencies(level, type);
        }

        foreach (TextureDependency<TR1Type> dependency in remapGroup.Dependencies)
        {
            // We need to ensure Atlantean spawns are accounted for because these are null meshes
            if (dependency.Entities.Contains(TR1Type.FlyingAtlantean))
            {
                dependency.AddEntity(TR1Type.ShootingAtlantean_N);
                dependency.AddEntity(TR1Type.NonShootingAtlantean_N);
                dependency.Entities.Sort();
            }
        }

        remapGroup.Dependencies.Sort(delegate (TextureDependency<TR1Type> d1, TextureDependency<TR1Type> d2)
        {
            if (d1.TileIndex == d2.TileIndex)
            {
                if (d1.Bounds.X == d2.Bounds.X)
                {
                    return d1.Bounds.Y.CompareTo(d2.Bounds.Y);
                }
                return d1.Bounds.X.CompareTo(d2.Bounds.X);
            }
            return d1.TileIndex.CompareTo(d2.TileIndex);
        });

        string dir = @"TR1\Deduplication";
        Directory.CreateDirectory(dir);
        File.WriteAllText(string.Format(@"{0}\{1}-TextureRemap.json", dir, lvl.ToUpper()), JsonConvert.SerializeObject(remapGroup, Formatting.Indented));
    }

    public static void Export(TR2Level level, string lvl)
    {
        TR2TextureRemapGroup remapGroup = new();
        foreach (TR2Type type in level.Models.Keys)
        {
            remapGroup.CalculateDependencies(level, type);
        }

        string dir = @"TR2\Deduplication";
        Directory.CreateDirectory(dir);
        File.WriteAllText(string.Format(@"{0}\{1}-TextureRemap.json", dir, lvl.ToUpper()), JsonConvert.SerializeObject(remapGroup, Formatting.Indented));
    }

    public static void Export(TR3Level level, string lvl)
    {
        TR3TextureRemapGroup remapGroup = new();
        foreach (TR3Type type in level.Models.Keys)
        {
            remapGroup.CalculateDependencies(level, type);
        }

        remapGroup.Dependencies.Sort(delegate (TextureDependency<TR3Type> d1, TextureDependency<TR3Type> d2)
        {
            if (d1.TileIndex == d2.TileIndex)
            {
                if (d1.Bounds.X == d2.Bounds.X)
                {
                    return d1.Bounds.Y.CompareTo(d2.Bounds.Y);
                }
                return d1.Bounds.X.CompareTo(d2.Bounds.X);
            }
            return d1.TileIndex.CompareTo(d2.TileIndex);
        });

        string dir = @"TR3\Deduplication";
        Directory.CreateDirectory(dir);
        File.WriteAllText(string.Format(@"{0}\{1}-TextureRemap.json", dir, lvl.ToUpper()), JsonConvert.SerializeObject(remapGroup, Formatting.Indented));
    }
}
