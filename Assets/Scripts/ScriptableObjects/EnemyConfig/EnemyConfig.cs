using Assets.Scripts.Enemy;
using UnityEngine;

namespace Assets.ScriptableObjects
{
    [CreateAssetMenu(fileName = "EnemyConfig", menuName = "Configs/EnemyConfig")]
    public class EnemyConfig : ScriptableObject
    {
        public EnemyType EnemyType;
        public BaseEnemy EnemyPrefab;
        [field: SerializeField, Range(0, 10)] public float Speed { get; private set; }
    }
}
