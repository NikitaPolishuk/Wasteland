using System;
using System.Collections.Generic;
using System.Linq;
using Assets.ScriptableObjects.BuildingConfig;
using Assets.Scripts.Enum;
using UnityEngine;

namespace Assets.Scripts.Buildings
{
    public static class BuildingCatalog
    {
        public const string RESOURCE_PATH = "WastelandBuildingConfigs";

        public static BuildingConfig[] Load(BuildingConfig[] sceneConfigs)
        {
            BuildingConfig[] resources = Resources.LoadAll<BuildingConfig>(RESOURCE_PATH);
            return GetAvailableConfigs(sceneConfigs.Concat(resources.Where(config => config.IsEditorManaged)));
        }

        public static BuildingConfig[] GetAvailableConfigs(IEnumerable<BuildingConfig> configs)
        {
            BuildingConfig[] candidates = configs.Where(config => config != null).Distinct().ToArray();
            Dictionary<BuildingType, BuildingConfig> available = new Dictionary<BuildingType, BuildingConfig>();

            foreach (BuildingConfig config in candidates)
            {
                if (config.IsEditorManaged == false) available.TryAdd(config.BuildingType, config);
            }

            foreach (BuildingConfig config in candidates)
            {
                if (config.IsEditorManaged == false || available.ContainsKey(config.BuildingType)) continue;
                if (config.IsAvailableInGame == false) continue;
                if (candidates.Any(other => other != config && other.BuildingType == config.BuildingType)) continue;
                if (candidates.Any(other => other.BuildingType != config.BuildingType && HasSameDisplayName(config, other))) continue;

                available.Add(config.BuildingType, config);
            }

            return available.Values.ToArray();
        }

        public static bool HasSameDisplayName(BuildingConfig first, BuildingConfig second)
        {
            return string.Equals(first.DisplayName.Trim(), second.DisplayName.Trim(), StringComparison.OrdinalIgnoreCase);
        }
    }
}
