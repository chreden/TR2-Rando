﻿using Newtonsoft.Json;
using TRGE.Core;
using TRLevelControl.Model;
using TRRandomizerCore.Helpers;
using TRRandomizerCore.Levels;

namespace TRRandomizerCore.Randomizers;

public class TR2RSecretRandomizer : BaseTR2RRandomizer
{
    private readonly Dictionary<string, List<Location>> _unarmedLocations;

    public ItemFactory<TR2Entity> ItemFactory { get; set; }

    public TR2RSecretRandomizer()
    {
        _unarmedLocations = JsonConvert.DeserializeObject<Dictionary<string, List<Location>>>(ReadResource(@"TR2\Locations\unarmed_locations.json"));
    }

    public override void Randomize(int seed)
    {
        _generator = new(seed);
        TR2SecretAllocator allocator = new()
        {
            Generator = _generator,
            Settings = Settings,
            ItemFactory = ItemFactory,
        };

        foreach (TRRScriptedLevel lvl in Levels)
        {
            LoadLevelInstance(lvl);
            if (Settings.DevelopmentMode)
            {
                allocator.PlaceAllSecrets(_levelInstance.Name, _levelInstance.Data);
            }
            else
            {
                List<Location> pickedLocations = allocator.RandomizeSecrets(_levelInstance.Name, _levelInstance.Data);
                AddDamageControl(_levelInstance, pickedLocations);
            }

            SaveLevelInstance();
            if (!TriggerProgress())
            {
                break;
            }
        }
    }

    private void AddDamageControl(TR2RCombinedLevel level, List<Location> locations)
    {
        if (locations.Any(l => l.RequiresDamage) && _unarmedLocations.ContainsKey(level.Name)
            && ItemFactory.CanCreateItem(level.Name, level.Data.Entities, Settings.DevelopmentMode))
        {
            List<Location> pool = _unarmedLocations[level.Name];
            Location location = pool[_generator.Next(0, pool.Count)];

            TR2Entity medi = ItemFactory.CreateItem(level.Name, level.Data.Entities, location, Settings.DevelopmentMode);
            medi.TypeID = TR2Type.LargeMed_S_P;
        }
    }
}
