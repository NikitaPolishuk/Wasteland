using System;
using System.Collections.Generic;
using System.Linq;
using Assets.ScriptableObjects.BuildingConfig;
using Assets.Scripts.Buildings;
using Assets.Scripts.Enum;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

[InitializeOnLoad]
public static class BuildingConfigEditor
{
    private const string RESOURCES_FOLDER = "Assets/Resources";
    private static readonly HashSet<BuildingConfig> _editedConfigs = new HashSet<BuildingConfig>();

    static BuildingConfigEditor()
    {
        Undo.undoRedoPerformed += SaveAfterUndoRedo;
    }

    public static BuildingConfig[] GetConfigs()
    {
        return AssetDatabase.FindAssets("t:BuildingConfig", new[] { "Assets" })
            .Select(AssetDatabase.GUIDToAssetPath)
            .OrderBy(path => path, StringComparer.Ordinal)
            .Select(AssetDatabase.LoadAssetAtPath<BuildingConfig>)
            .Where(config => config != null)
            .OrderBy(config => config.BuildingType)
            .ToArray();
    }

    public static string[] GetErrors(BuildingConfig config) => GetErrors(config, GetConfigs());

    public static BuildingType[] GetTypes()
    {
        return System.Enum.GetValues(typeof(BuildingType)).Cast<BuildingType>().Distinct().ToArray();
    }

    public static bool TryCreateConfig(BuildingType type, out BuildingConfig config, out string error)
    {
        config = null;
        if (EditorApplication.isPlayingOrWillChangePlaymode)
        {
            error = "Building settings can be changed only outside Play Mode.";
            return false;
        }

        if (System.Enum.IsDefined(typeof(BuildingType), type) == false)
        {
            error = "Select a building type defined in code.";
            return false;
        }

        BuildingConfig[] configs = GetConfigs();
        if (configs.Any(existing => existing.BuildingType == type))
        {
            error = $"Building type {type} already has a configuration.";
            return false;
        }

        if (AssetDatabase.IsValidFolder(RESOURCES_FOLDER) == false)
            AssetDatabase.CreateFolder("Assets", "Resources");

        string folder = $"{RESOURCES_FOLDER}/{BuildingCatalog.RESOURCE_PATH}";
        if (AssetDatabase.IsValidFolder(folder) == false)
            AssetDatabase.CreateFolder(RESOURCES_FOLDER, BuildingCatalog.RESOURCE_PATH);

        BuildingConfig draft = ScriptableObject.CreateInstance<BuildingConfig>();
        draft.BuildingType = type;
        Type dataType = BuildingTypeDataEditor.GetDataType(type);
        if (dataType != null) draft.SetTypeData((BuildingTypeData)Activator.CreateInstance(dataType));
        draft.Levels = new[] { new BuildingLevelConfig() };
        for (int suffix = 2; HasDuplicateName(draft, configs); suffix++)
            draft.DisplayName = $"{type} {suffix}";

        SerializedObject serializedDraft = new SerializedObject(draft);
        serializedDraft.FindProperty("_isEditorManaged").boolValue = true;
        serializedDraft.ApplyModifiedPropertiesWithoutUndo();
        string path = AssetDatabase.GenerateUniqueAssetPath($"{folder}/{type}.asset");
        AssetDatabase.CreateAsset(draft, path);
        AssetDatabase.SaveAssetIfDirty(draft);
        config = draft;
        error = null;
        return true;
    }

    public static bool TrySetDisplayName(BuildingConfig config, string displayName, out string error)
    {
        if (string.IsNullOrWhiteSpace(displayName))
        {
            error = "Display name is required.";
            return false;
        }

        return TryApply(config, "Rename building type", candidate => candidate.DisplayName = displayName.Trim(), out error);
    }

    public static bool TrySetArea(BuildingConfig config, int area, out string error)
    {
        return TryApply(config, "Change building area", candidate => candidate.Area = area, out error);
    }

    public static bool TrySetBuildingPrefab(BuildingConfig config, GameObject prefab, out string error)
    {
        return TryApply(config, "Change main building prefab", candidate => candidate.BuildingPrefab = prefab, out error);
    }

    public static bool TrySetLevel(BuildingConfig config, int index, GameObject appearancePrefab, int maxHp, int upgradeCost, out string error)
    {
        if (config == null || config.Levels == null || index < 0 || index >= config.Levels.Length || config.Levels[index] == null)
        {
            error = "Select an existing level.";
            return false;
        }

        return TryApply(config, "Change building level", candidate =>
        {
            BuildingLevelConfig level = candidate.Levels[index];
            level.AppearancePrefab = appearancePrefab;
            level.MaxHp = maxHp;
            if (level.Level > 0) level.UpgradeCost = upgradeCost;
        }, out error);
    }

    public static bool TryAddLevel(BuildingConfig config, GameObject appearancePrefab, int maxHp, int upgradeCost, out string error)
    {
        int nextLevel = config?.Levels?.Length ?? 0;
        if (appearancePrefab == null || IsPrefabAsset(appearancePrefab) == false)
        {
            error = "Choose an appearance prefab from the Project.";
            return false;
        }

        if (maxHp <= 0)
        {
            error = "Maximum health must be positive.";
            return false;
        }

        if (nextLevel > 0 && upgradeCost < 0)
        {
            error = "Upgrade cost cannot be negative.";
            return false;
        }

        return TryApply(config, "Add building level", candidate =>
        {
            BuildingLevelConfig[] levels = candidate.Levels ?? Array.Empty<BuildingLevelConfig>();
            Array.Resize(ref levels, nextLevel + 1);
            levels[nextLevel] = new BuildingLevelConfig
            {
                Level = nextLevel,
                AppearancePrefab = appearancePrefab,
                MaxHp = maxHp,
                UpgradeCost = nextLevel == 0 ? 0 : upgradeCost
            };
            candidate.Levels = levels;
        }, out error);
    }

    public static bool TryRemoveLevel(BuildingConfig config, int index, out string error)
    {
        if (config == null || config.Levels == null || index < 0 || index >= config.Levels.Length || config.Levels[index] == null)
        {
            error = "Select an existing level.";
            return false;
        }

        int removedLevel = config.Levels[index].Level;
        if (removedLevel <= 0)
        {
            error = "Level 0 is required and cannot be removed.";
            return false;
        }

        return TryApply(config, "Remove building level", candidate =>
        {
            candidate.Levels = candidate.Levels.Where((level, position) => position != index).ToArray();
            foreach (BuildingLevelConfig level in candidate.Levels)
            {
                if (level != null && level.Level > removedLevel)
                    level.Level--;
            }
        }, out error, candidateError => GetErrorBeforeRemoval(candidateError, config.Levels, index));
    }

    internal static bool TryApply(BuildingConfig config, string undoName, Action<BuildingConfig> edit, out string error,
        Func<string, string> getPreviousError = null)
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
        {
            error = "Building settings can be changed only outside Play Mode.";
            return false;
        }

        if (config == null || AssetDatabase.Contains(config) == false)
        {
            error = "Select a saved building configuration.";
            return false;
        }

        BuildingConfig candidate = Object.Instantiate(config);
        try
        {
            edit(candidate);
            BuildingConfig[] configs = GetConfigs();
            string[] existingErrors = GetErrors(config, configs);
            if (candidate.DisplayName != config.DisplayName && HasDuplicateName(candidate, configs))
            {
                error = GetDuplicateNameError(candidate);
                return false;
            }

            if (config.IsEditorManaged == false || existingErrors.Length == 0)
            {
                string[] newErrors = GetErrors(candidate, configs)
                    .Where(candidateError => existingErrors.Contains(getPreviousError?.Invoke(candidateError) ?? candidateError) == false)
                    .Distinct()
                    .ToArray();
                if (newErrors.Length > 0)
                {
                    error = string.Join("\n", newErrors);
                    return false;
                }
            }

            Undo.RegisterCompleteObjectUndo(config, undoName);
            EditorUtility.CopySerializedManagedFieldsOnly(candidate, config);
            EditorUtility.SetDirty(config);
            AssetDatabase.SaveAssetIfDirty(config);
            _editedConfigs.Add(config);
            error = null;
            return true;
        }
        finally
        {
            Object.DestroyImmediate(candidate);
        }
    }

    internal static string[] GetErrors(BuildingConfig config, BuildingConfig[] configs)
    {
        List<string> errors = new List<string>(config.GetValidationErrors());
        if (HasDuplicateName(config, configs)) errors.Add(GetDuplicateNameError(config));
        if (config.IsEditorManaged && configs.Count(other => other.BuildingType == config.BuildingType) > 1)
            errors.Add($"Building type {config.BuildingType} has multiple configurations.");
        if (config.BuildingPrefab != null && IsPrefabAsset(config.BuildingPrefab) == false)
            errors.Add("Main prefab must be a prefab asset from the Project.");

        if (config.Levels != null)
        {
            foreach (BuildingLevelConfig level in config.Levels)
            {
                if (level?.AppearancePrefab != null && IsPrefabAsset(level.AppearancePrefab) == false)
                    errors.Add($"Level {level.Level}: appearance must be a prefab asset from the Project.");
            }
        }

        return errors.ToArray();
    }

    private static bool HasDuplicateName(BuildingConfig config, BuildingConfig[] configs)
    {
        return configs.Any(other => other.BuildingType != config.BuildingType && BuildingCatalog.HasSameDisplayName(config, other));
    }

    private static string GetDuplicateNameError(BuildingConfig config)
    {
        return $"Display name '{config.DisplayName}' is already in use by another building type.";
    }

    private static bool IsPrefabAsset(GameObject prefab)
    {
        return PrefabUtility.IsPartOfPrefabAsset(prefab) && prefab.transform.parent == null;
    }

    private static string GetErrorBeforeRemoval(string error, BuildingLevelConfig[] levels, int removedIndex)
    {
        int removedLevel = levels[removedIndex].Level;
        for (int position = 0; position < levels.Length; position++)
        {
            BuildingLevelConfig level = levels[position];
            if (level == null && position > removedIndex && error == $"Level entry {position - 1} is missing.")
                return $"Level entry {position} is missing.";

            if (level == null || level.Level <= removedLevel)
                continue;

            string currentLabel = $"Level {level.Level - 1}:";
            if (error.StartsWith(currentLabel, StringComparison.Ordinal))
                return $"Level {level.Level}:" + error.Substring(currentLabel.Length);
        }

        return error;
    }

    private static void SaveAfterUndoRedo()
    {
        _editedConfigs.RemoveWhere(config => config == null);
        foreach (BuildingConfig config in _editedConfigs)
            AssetDatabase.SaveAssetIfDirty(config);
    }
}
