using System;
using Assets.Scripts.Factories;
using UnityEngine;
using Zenject;

namespace Assets.Scripts.Enemy
{
    public enum EnemyType
    {
        Melee,
    }
    
    public class EnemyController : ITickable
    {
        private readonly IEnemyFactory _enemyFactory;
        private readonly Transform _spawnPoint;
        private float _timer;

        [Inject]
        public EnemyController(IEnemyFactory enemyFactory, Transform spawnPoint)
        {
            _enemyFactory = enemyFactory;
            _spawnPoint = spawnPoint;
        }

        public BaseEnemy Spawn(EnemyType type, Vector2 position)
        {
            return _enemyFactory.Create(type, position);
        }

        public void Tick()
        {
            _timer += Time.deltaTime;

            if (_timer >= 5f)
            {
                _timer = 0f;
            }
        }
    }
}
