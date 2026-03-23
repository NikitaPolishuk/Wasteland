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

                if (type == EnvironmentType.Tree1 || type == EnvironmentType.Tree2 ) environment.transform.localScale *= Random.Range(1f, 1.7f);
                if (type == EnvironmentType.Grass1 || type == EnvironmentType.Grass2 ) environment.transform.localScale *= Random.Range(1.2f, 1.6f);
                
                return environment;
            }

            Debug.LogWarning($"environment {type} not found!");
            return null;
        }
    }
}