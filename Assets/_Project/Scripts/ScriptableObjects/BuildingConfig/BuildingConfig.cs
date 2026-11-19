using System;
using System.Collections.Generic;
using Assets.Scripts.Buildings;
using Assets.Scripts.Enum;
using UnityEngine;

namespace Assets.ScriptableObjects.BuildingConfig
{
    public class BuildingConfig : ScriptableObject
    {
        public BuildingType BuildingType;
        public GameObject BuildingPrefab;
        public BuildingLevelConfig[] Levels;
        public int Area;

        [SerializeReference] private BuildingTypeData _typeData;
        [SerializeField] private string _displayName;
        [SerializeField, HideInInspector] private bool _isEditorManaged;

        public bool IsEditorManaged => _isEditorManaged;
        public bool IsAvailableInGame => IsEditorManaged == false || GetValidationErrors().Length == 0;

        public string DisplayName
        {
            get => string.IsNullOrWhiteSpace(_displayName) ? BuildingType.ToString() : _displayName;
            set => _displayName = value;
        }

        public T GetTypeData<T>() where T : BuildingTypeData => _typeData as T;

        public void SetTypeData(BuildingTypeData data) => _typeData = data;

        public virtual string[] GetValidationErrors()
        {
            List<string> errors = new List<string>();
            Type dataType = BuildingTypeDataDefinitions.GetDataType(BuildingType);
            if (dataType != null && _typeData == null) errors.Add("Type settings are required.");
            else if (_typeData != null && _typeData.GetType() != dataType) errors.Add("Saved type settings do not match the building type.");
            if (_typeData != null) errors.AddRange(_typeData.GetValidationErrors());
            if (System.Enum.IsDefined(typeof(BuildingType), BuildingType) == false)
                errors.Add($"Unknown building type: {BuildingType}.");

            if (Area <= 0) errors.Add($"Area must be positive (current value: {Area}).");

            if (BuildingPrefab == null)
            {
                errors.Add("Main prefab is required.");
            }
            else if (BuildingPrefab.TryGetComponent(out BaseBuilding building) == false)
            {
                errors.Add("Main prefab must have a BaseBuilding component on its root.");
            }
            else
            {
                if (BuildingPrefab.transform.parent != null)
                    errors.Add("Main prefab must reference the root of a prefab.");
                if (building.HasBuildPrice == false)
                    errors.Add("Assign a BuildPrice reference on the main prefab's building component.");
                if (building.HasHealthSystem == false)
                    errors.Add("Assign a HealthSystem reference on the main prefab's building component.");
            }

            if (Levels == null || Levels.Length == 0)
            {
                errors.Add("Level 0 is required.");
                return errors.ToArray();
            }

            HashSet<int> levelNumbers = new HashSet<int>();
            for (int i = 0; i < Levels.Length; i++)
            {
                BuildingLevelConfig level = Levels[i];
                if (level == null)
                {
                    errors.Add($"Level entry {i} is missing.");
                    continue;
                }

                string label = $"Level {level.Level}";
                if (level.Level < 0 || level.Level >= Levels.Length || levelNumbers.Add(level.Level) == false)
                    errors.Add($"{label}: use consecutive level numbers starting at 0, with no duplicates.");
                if (level.AppearancePrefab == null) errors.Add($"{label}: appearance prefab is required.");
                else if (level.AppearancePrefab.transform.parent != null)
                    errors.Add($"{label}: appearance must reference the root of a prefab.");
                if (level.MaxHp <= 0) errors.Add($"{label}: maximum health must be positive (current value: {level.MaxHp}).");
                if (level.Level > 0 && level.UpgradeCost < 0)
                    errors.Add($"{label}: upgrade cost cannot be negative (current value: {level.UpgradeCost}).");
            }

            return errors.ToArray();
        }
    }

    [Serializable]
    public class BuildingLevelConfig
    {
        public int Level;
        public GameObject AppearancePrefab;
        public int UpgradeCost;
        public int MaxHp;
    }
}
