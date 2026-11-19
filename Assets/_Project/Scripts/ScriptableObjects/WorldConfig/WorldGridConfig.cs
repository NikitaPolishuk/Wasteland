using System;
using System.Collections.Generic;
using BuildingAsset = Assets.ScriptableObjects.BuildingConfig.BuildingConfig;
using Assets.Scripts.Enum;
using UnityEngine;

namespace Assets.ScriptableObjects
{
    [Serializable]
    public class WorldBuildingEntry
    {
        public BuildingAsset Config;
        public int Gap;
    }

    [Serializable]
    public class WorldBuildingRequirement
    {
        public BuildingType Type;
        public int Minimum;
    }

    [CreateAssetMenu(fileName = "WorldGridConfig", menuName = "Configs/WorldGridConfig")]
    public class WorldGridConfig : ScriptableObject
    {
        [Header("Tiles")] public RuleTile GroundRuleTile;
        [Header("World Settings")]
        public int Width = 50;
        public int GroundHeight = 4;
        public int GroundY = 0;
        [Header("Starting buildings")]
        public BuildingAsset Camp;
        public int CampOffset;
        public List<WorldBuildingRequirement> Requirements = new List<WorldBuildingRequirement>();
        public List<WorldBuildingEntry> LeftBuildings = new List<WorldBuildingEntry>();
        public List<WorldBuildingEntry> RightBuildings = new List<WorldBuildingEntry>();
        [Header("Player")]
        public int PlayerCell;
    }
}
