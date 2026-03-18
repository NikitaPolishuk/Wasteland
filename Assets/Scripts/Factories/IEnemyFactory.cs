using Assets.Scripts.Enemy;
using UnityEngine;

namespace Assets.Scripts.Factories
{
    public interface IEnemyFactory
    {
        BaseEnemy Create(EnemyType type, Vector2 position);
    }
}
