using System.Collections.Generic;
using System.Linq;
using Assets.ScriptableObjects;
using Assets.ScriptableObjects.BuildingConfig;
using Assets.Scripts.Enum;
using UnityEngine;
using Zenject;

namespace Assets.Scripts.Factories
{
    public class EnvironmentFactory : IEnvironmentFactory
    {
        private readonly DiContainer _container;
        private readonly Dictionary<EnvironmentType, EnvironmentConfig> _configsDict;

        public EnvironmentFactory(DiContainer container, EnvironmentConfig[] configs)
        {
            _container = container;
            _configsDict = configs.ToDictionary(x => x.EnvironmentType, x => x);
        }

        public GameObject Create(EnvironmentType type, Vector2 position)
        {
            if (_configsDict.TryGetValue(type, out var config))
            {
                var environment = _container.InstantiatePrefab(config.EnvironmentPrefab, position, Quaternion.identity, null);
                return environment;
            }

            Debug.LogWarning($"environment {type} not found!");
            return null;
        }
        
        public GameObject Create(EnvironmentType type, Vector2 position, Vector3 scale)
        {
            var environment = Create(type, position);
            if (environment != null) environment.transform.localScale = scale;
            return environment;
        }
    }
}