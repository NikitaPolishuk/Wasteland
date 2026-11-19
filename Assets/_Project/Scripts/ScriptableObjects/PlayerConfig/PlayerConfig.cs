using UnityEngine;

namespace Assets.ScriptableObjects
{
    [CreateAssetMenu(fileName = "PlayerConfig", menuName = "Configs/PlayerConfig")]
    public class PlayerConfig : ScriptableObject
    {
        [field: SerializeField, Range(0, 10)] public float Speed { get; private set; }
        [field: SerializeField, Min(0)] public int StartingGold { get; private set; } = 30;
    }
}
