using System;
using System.Linq;
using Assets.ScriptableObjects.BuildingConfig;
using Assets.Scripts.Enum;
using UnityEditor;
using UnityEngine;

public sealed class BuildingSettingsWindow : EditorWindow
{
    private const float PREVIEW_HEIGHT = 180f;
    private const int MINIMUM_HEALTH = 1;

    [SerializeField] private BuildingType _selectedType;
    [SerializeField] private int _levelIndex;

    private BuildingConfig _selected;
    private BuildingType[] _types;
    private BuildingConfig[] _configs;
    private Vector2 _scroll;
    private string _editError;
    private BuildingConfig _addingLevelTo;
    private BuildingLevelConfig _newLevel;

    [MenuItem("Wasteland/Buildings")]
    public static void Open() => GetWindow<BuildingSettingsWindow>("Buildings");

    private void OnEnable()
    {
        LoadConfigs();
        EditorApplication.projectChanged += LoadConfigs;
        Undo.undoRedoPerformed += OnUndoRedo;
    }

    private void OnDisable()
    {
        EditorApplication.projectChanged -= LoadConfigs;
        Undo.undoRedoPerformed -= OnUndoRedo;
    }

    private void OnGUI()
    {
        if (_types.Length == 0)
        {
            EditorGUILayout.HelpBox("No building types are defined in code.", MessageType.Info);
            return;
        }

        int selectedIndex = Array.IndexOf(_types, _selectedType);
        string[] labels = _types.Select(type => FindConfig(type)?.DisplayName ?? type.ToString()).ToArray();
        int newIndex = EditorGUILayout.Popup("Building type", Mathf.Max(0, selectedIndex), labels);
        if (newIndex != selectedIndex)
        {
            _selectedType = _types[newIndex];
            _levelIndex = 0;
            _editError = null;
        }

        _selected = FindConfig(_selectedType);
        bool isPlaying = EditorApplication.isPlayingOrWillChangePlaymode;
        if (isPlaying) EditorGUILayout.HelpBox("Building settings are read-only during Play Mode.", MessageType.Info);

        if (_selected == null)
        {
            DrawMissingConfig(isPlaying);
            return;
        }

        _scroll = EditorGUILayout.BeginScrollView(_scroll);
        using (new EditorGUI.DisabledScope(isPlaying))
        {
            DrawBuildingSettings();
            DrawTypeSettings();
        }

        DrawLevels(isPlaying);
        if (string.IsNullOrEmpty(_editError) == false) EditorGUILayout.HelpBox(_editError, MessageType.Error);
        string[] errors = BuildingConfigEditor.GetErrors(_selected, _configs);
        if (_selected.IsEditorManaged)
            EditorGUILayout.LabelField("Status", errors.Length == 0 ? "Available in game" : "Draft");
        foreach (string error in errors)
            EditorGUILayout.HelpBox(error, MessageType.Error);

        EditorGUILayout.EndScrollView();
    }

    private void LoadConfigs()
    {
        _types = BuildingConfigEditor.GetTypes();
        _configs = BuildingConfigEditor.GetConfigs();
        Repaint();
    }

    private BuildingConfig FindConfig(BuildingType type)
    {
        return _configs.FirstOrDefault(config => config.BuildingType == type && config.IsEditorManaged == false)
            ?? _configs.FirstOrDefault(config => config.BuildingType == type);
    }

    private void DrawMissingConfig(bool isPlaying)
    {
        EditorGUILayout.HelpBox("This building type has no configuration.", MessageType.Info);
        using (new EditorGUI.DisabledScope(isPlaying))
        {
            if (GUILayout.Button("Create configuration") &&
                BuildingConfigEditor.TryCreateConfig(_selectedType, out _selected, out _editError))
            {
                _levelIndex = 0;
                LoadConfigs();
            }
        }

        if (string.IsNullOrEmpty(_editError) == false) EditorGUILayout.HelpBox(_editError, MessageType.Error);
    }

    private void OnUndoRedo()
    {
        _editError = null;
        LoadConfigs();
    }

    private void DrawBuildingSettings()
    {
        string displayName = EditorGUILayout.DelayedTextField("Display name", _selected.DisplayName);
        if (displayName != _selected.DisplayName)
            BuildingConfigEditor.TrySetDisplayName(_selected, displayName, out _editError);

        GameObject prefab = (GameObject)EditorGUILayout.ObjectField("Main prefab", _selected.BuildingPrefab, typeof(GameObject), false);
        if (prefab != _selected.BuildingPrefab)
            BuildingConfigEditor.TrySetBuildingPrefab(_selected, prefab, out _editError);

        int area = EditorGUILayout.DelayedIntField("Area (cells)", _selected.Area);
        if (area != _selected.Area)
            BuildingConfigEditor.TrySetArea(_selected, area, out _editError);
    }

    private void DrawTypeSettings()
    {
        Type dataType;
        try
        {
            dataType = BuildingTypeDataEditor.GetDataType(_selected.BuildingType);
        }
        catch (InvalidOperationException exception)
        {
            EditorGUILayout.HelpBox(exception.Message, MessageType.Error);
            return;
        }

        if (dataType == null) return;

        BuildingConfig candidate = Instantiate(_selected);
        try
        {
            SerializedObject serialized = new SerializedObject(candidate);
            SerializedProperty data = serialized.FindProperty("_typeData");
            bool isMissing = data.managedReferenceValue == null;
            if (isMissing)
                data.managedReferenceValue = Activator.CreateInstance(dataType);
            else if (data.managedReferenceValue.GetType() != dataType)
            {
                EditorGUILayout.HelpBox("Saved type data does not match the definition for this building.", MessageType.Error);
                return;
            }

            EditorGUILayout.LabelField("Type settings", EditorStyles.boldLabel);
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(data, true);
            bool changed = EditorGUI.EndChangeCheck();
            if (isMissing && GUILayout.Button("Save type settings")) changed = true;
            if (changed)
            {
                serialized.ApplyModifiedPropertiesWithoutUndo();
                BuildingTypeDataEditor.TrySetData(_selected, (BuildingTypeData)data.managedReferenceValue, out _editError);
            }
        }
        finally
        {
            DestroyImmediate(candidate);
        }
    }

    private void DrawLevels(bool isPlaying)
    {
        BuildingLevelConfig[] levels = _selected.Levels;
        BuildingLevelConfig level = null;
        if (levels != null && levels.Length > 0)
        {
            string[] labels = levels.Select((entry, position) => entry == null ? $"Missing entry {position}" : $"Level {entry.Level}").ToArray();
            int index = EditorGUILayout.Popup("Level", Mathf.Clamp(_levelIndex, 0, levels.Length - 1), labels);
            if (index != _levelIndex)
            {
                _levelIndex = index;
                _editError = null;
            }

            level = levels[_levelIndex];
        }

        using (new EditorGUI.DisabledScope(isPlaying))
        {
            if (level != null)
            {
                GameObject appearance = (GameObject)EditorGUILayout.ObjectField("Appearance prefab", level.AppearancePrefab, typeof(GameObject), false);
                int maxHp = EditorGUILayout.DelayedIntField("Maximum health", level.MaxHp);
                int cost = level.Level > 0 ? EditorGUILayout.DelayedIntField("Upgrade cost", level.UpgradeCost) : level.UpgradeCost;
                if (appearance != level.AppearancePrefab || maxHp != level.MaxHp || cost != level.UpgradeCost)
                    BuildingConfigEditor.TrySetLevel(_selected, _levelIndex, appearance, maxHp, cost, out _editError);

                DrawAppearancePreview(_selected.Levels[_levelIndex].AppearancePrefab);
            }

            DrawLevelActions(level);
        }
    }

    private void DrawLevelActions(BuildingLevelConfig selectedLevel)
    {
        if (_addingLevelTo != _selected)
            _newLevel = null;

        using (new EditorGUILayout.HorizontalScope())
        {
            if (_newLevel == null && GUILayout.Button("Add level"))
            {
                _addingLevelTo = _selected;
                _newLevel = new BuildingLevelConfig
                {
                    AppearancePrefab = selectedLevel?.AppearancePrefab,
                    MaxHp = Mathf.Max(MINIMUM_HEALTH, selectedLevel?.MaxHp ?? MINIMUM_HEALTH)
                };
                _editError = null;
            }

            using (new EditorGUI.DisabledScope(selectedLevel == null || selectedLevel.Level <= 0))
            {
                if (GUILayout.Button("Remove level") &&
                    EditorUtility.DisplayDialog("Remove building level",
                        $"Remove level {selectedLevel.Level}? Higher levels will be renumbered.", "Remove", "Cancel") &&
                    BuildingConfigEditor.TryRemoveLevel(_selected, _levelIndex, out _editError))
                {
                    _levelIndex = Mathf.Min(_levelIndex, _selected.Levels.Length - 1);
                }
            }
        }

        if (_newLevel == null)
            return;

        int nextLevel = _selected.Levels?.Length ?? 0;
        EditorGUILayout.LabelField($"New level {nextLevel}", EditorStyles.boldLabel);
        _newLevel.AppearancePrefab = (GameObject)EditorGUILayout.ObjectField("Appearance prefab", _newLevel.AppearancePrefab, typeof(GameObject), false);
        _newLevel.MaxHp = EditorGUILayout.IntField("Maximum health", _newLevel.MaxHp);
        if (nextLevel > 0)
            _newLevel.UpgradeCost = EditorGUILayout.IntField("Upgrade cost", _newLevel.UpgradeCost);

        using (new EditorGUILayout.HorizontalScope())
        {
            if (GUILayout.Button("Apply new level") &&
                BuildingConfigEditor.TryAddLevel(_selected, _newLevel.AppearancePrefab, _newLevel.MaxHp, _newLevel.UpgradeCost, out _editError))
            {
                _levelIndex = nextLevel;
                _newLevel = null;
            }

            if (GUILayout.Button("Cancel"))
            {
                _newLevel = null;
                _editError = null;
            }
        }
    }

    private void DrawAppearancePreview(GameObject appearance)
    {
        if (appearance == null) return;

        EditorGUILayout.LabelField("Appearance preview", EditorStyles.boldLabel);
        Texture2D preview = AssetPreview.GetAssetPreview(appearance);
        Sprite sprite = null;
        if (preview == null)
        {
            SpriteRenderer renderer = appearance.GetComponentInChildren<SpriteRenderer>(true);
            if (renderer != null) sprite = renderer.sprite;
            if (sprite != null) preview = AssetPreview.GetAssetPreview(sprite);
        }

        Rect rect = GUILayoutUtility.GetRect(0, PREVIEW_HEIGHT, GUILayout.ExpandWidth(true));
        if (preview != null)
        {
            GUI.DrawTexture(rect, preview, ScaleMode.ScaleToFit);
        }
        else
        {
            bool isLoading = AssetPreview.IsLoadingAssetPreview(appearance.GetInstanceID()) ||
                (sprite != null && AssetPreview.IsLoadingAssetPreview(sprite.GetInstanceID()));
            GUI.Label(rect, isLoading ? "Loading preview…" : "Preview unavailable.", EditorStyles.centeredGreyMiniLabel);
            if (isLoading) Repaint();
        }
    }
}
