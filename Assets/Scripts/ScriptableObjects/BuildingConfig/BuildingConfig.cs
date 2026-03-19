using Assets.Scripts.Enum;
using UnityEngine;

namespace Assets.ScriptableObjects.BuildingConfig
{
    [CreateAssetMenu(fileName = "BuildingConfig", menuName = "Configs/BuildingConfig")]
    public class BuildingConfig : ScriptableObject
    {
        public BuildingType BuildingType;
        public GameObject BuildingPrefab;
        public BuildingLevelConfig[] Levels;
        public int Area;
    }

    [System.Serializable]
    public class BuildingLevelConfig
    {
        public int Level;
        public GameObject AppearancePrefab;
        public int UpgradeCost;
        public int MaxHp;
    }
}
