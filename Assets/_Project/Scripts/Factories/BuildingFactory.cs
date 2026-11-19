using System.Collections.Generic;
using System.Linq;
using Assets.ScriptableObjects.BuildingConfig;
using Assets.Scripts.Enum;
using UnityEngine;
using Zenject;

namespace Assets.Scripts.Factories
{
    public class BuildingFactory : IBuildingFactory
    {
        private readonly DiContainer _container;
        private readonly BuildingConfig[] _configs;
        private readonly Dictionary<BuildingType, BuildingConfig> _configsDict;

        public BuildingFactory(DiContainer container, BuildingConfig[] configs)
        {
            _container = container;
            _configs = configs;
            _configsDict = configs.ToDictionary(x => x.BuildingType, x => x);
        }

        public GameObject Create(BuildingConfig config, Vector2 position)
        {
            GameObject building = _container.InstantiatePrefab(config.BuildingPrefab, position, Quaternion.identity, null);
            Buildings.BaseBuilding component = building.GetComponent<Buildings.BaseBuilding>();
            component?.Initialize(config);
            return building;
        }

        public GameObject Create(BuildingType type, Vector2 position)
        {
            if (_configsDict.TryGetValue(type, out var config))
            {
                return Create(config, position);
            }

            Debug.LogWarning($"Building type {type} not found!");
            return null;
        }
    }
}
