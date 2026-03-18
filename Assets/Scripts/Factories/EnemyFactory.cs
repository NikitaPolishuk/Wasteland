using System.Collections.Generic;
using System.Linq;
using Assets.ScriptableObjects;
using Assets.Scripts.Enemy;
using UnityEngine;
using Zenject;

namespace Assets.Scripts.Factories
{
    public class EnemyFactory : IEnemyFactory
    {
        private readonly DiContainer _container;
        private readonly EnemyConfig[] _enemyConfigs;
        private readonly Dictionary<EnemyType, BaseEnemy> _enemiesDict;

        public EnemyFactory(DiContainer container, EnemyConfig[] enemyConfigs)
        {
            _container = container;
            _enemyConfigs = enemyConfigs;
            _enemiesDict = enemyConfigs.ToDictionary(x => x.EnemyType, x => x.EnemyPrefab);
        }

        public BaseEnemy Create(EnemyType type, Vector2 position)
        {
            if (_enemiesDict.TryGetValue(type, out var prefab))
            {
                return _container.InstantiatePrefabForComponent<BaseEnemy>(prefab, position, Quaternion.identity, null);
            }

            Debug.LogWarning($"Enemy type {type} not found!");
            return null;
        }
    }
}
