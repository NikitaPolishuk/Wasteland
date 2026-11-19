using Assets.Scripts.Enum;
using UnityEngine;

namespace Assets.ScriptableObjects
{
    [CreateAssetMenu(fileName = "EnvironmentConfig", menuName = "Configs/EnvironmentConfig")]
    public class EnvironmentConfig : ScriptableObject
    {
        public EnvironmentType EnvironmentType;
        public GameObject EnvironmentPrefab;
        public int Area;
    }
}