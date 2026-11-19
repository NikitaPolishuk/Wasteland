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

    [Serializable]
    public class EnvironmentVariant
    {
        public EnvironmentConfig Config;
        public float Weight = 1f;
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
        [Header("Environment")]
        public int ForestMinLength = 12;
        public int ForestMaxLength = 30;
        public int OpenMinLength = 12;
        public int OpenMaxLength = 30;
        public float TreeScaleMin = 0.8f;
        public float TreeScaleMax = 1.2f;
        public float RockScaleMin = 1f;
        public float RockScaleMax = 1f;
        public float GrassScaleMin = 1f;
        public float GrassScaleMax = 1f;
        [Range(0, 1)] public float TreeDensity = 0.2f;
        [Range(0, 1)] public float ForestRockDensity = 0.05f;
        [Range(0, 1)] public float OpenRockDensity = 0.1f;
        [Range(0, 1)] public float ForestGrassDensity = 0.2f;
        [Range(0, 1)] public float OpenGrassDensity = 0.2f;
        public List<EnvironmentVariant> Trees = new List<EnvironmentVariant>();
        public List<EnvironmentVariant> Rocks = new List<EnvironmentVariant>();
        public List<EnvironmentVariant> Grass = new List<EnvironmentVariant>();
        [Header("Player")]
        public int PlayerCell;
    }
}
