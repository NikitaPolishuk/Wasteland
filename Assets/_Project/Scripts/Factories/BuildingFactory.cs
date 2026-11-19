using System.Collections.Generic;
using Assets.ScriptableObjects.BuildingConfig;
using Assets.Scripts.Buildings;
using Assets.Scripts.Enum;
using UnityEngine;
using Zenject;

namespace Assets.Scripts.Factories
{
    public class BuildingFactory : IBuildingFactory
    {
        private readonly DiContainer _container;
        private readonly Dictionary<BuildingType, BuildingConfig> _configsDict;

        public BuildingFactory(DiContainer container, BuildingConfig[] configs)
        {
            _container = container;
            _configsDict = new Dictionary<BuildingType, BuildingConfig>();
            foreach (BuildingConfig config in BuildingCatalog.GetAvailableConfigs(configs))
                _configsDict.Add(config.BuildingType, config);
        }

        public GameObject Create(BuildingConfig config, Vector2 position)
        {
            if (config == null) return null;

            if (config.IsEditorManaged &&
                (_configsDict.TryGetValue(config.BuildingType, out BuildingConfig registered) == false || registered != config ||
                 config.IsAvailableInGame == false))
            {
                Debug.LogWarning($"Building type {config.BuildingType} is not available for placement.");
                return null;
            }

            GameObject building = _container.InstantiatePrefab(config.BuildingPrefab, position, Quaternion.identity, null);
            BaseBuilding component = building.GetComponent<BaseBuilding>();
            component?.Initialize(config);
            return building;
        }

        public GameObject Create(BuildingType type, Vector2 position)
        {
            if (_configsDict.TryGetValue(type, out BuildingConfig config))
            {
                return Create(config, position);
            }

            Debug.LogWarning($"Building type {type} not found!");
            return null;
        }
    }
}
