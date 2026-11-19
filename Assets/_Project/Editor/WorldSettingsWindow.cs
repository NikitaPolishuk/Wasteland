using Assets.ScriptableObjects;
using Assets.Scripts.World;
using UnityEditor;
using UnityEngine;

public class WorldSettingsWindow : EditorWindow
{
    private WorldGridConfig _config;
    private Vector2 _scroll;

    [MenuItem("Wasteland/World Settings")]
    public static void Open() => GetWindow<WorldSettingsWindow>("World Settings");

    private void OnGUI()
    {
        _config = (WorldGridConfig)EditorGUILayout.ObjectField("World configuration", _config, typeof(WorldGridConfig), false);
        if (GUILayout.Button("Create configuration"))
        {
            string path = EditorUtility.SaveFilePanelInProject("Create world configuration", "WorldGridConfig", "asset", "Choose a location");
            if (string.IsNullOrEmpty(path) == false)
            {
                _config = CreateInstance<WorldGridConfig>();
                AssetDatabase.CreateAsset(_config, path);
                AssetDatabase.SaveAssets();
            }
        }
        if (_config == null) return;
        _scroll = EditorGUILayout.BeginScrollView(_scroll);
        SerializedObject data = new SerializedObject(_config);
        data.Update();
        SerializedProperty property = data.GetIterator();
        bool enterChildren = true;
        while (property.NextVisible(enterChildren))
        {
            enterChildren = false;
            if (property.name == "m_Script") continue;
            EditorGUILayout.PropertyField(property, true);
        }
        if (data.ApplyModifiedProperties()) EditorUtility.SetDirty(_config);
        ShowWidths("Left", _config.LeftBuildings);
        ShowWidths("Right", _config.RightBuildings);
        if (_config.Camp != null) EditorGUILayout.LabelField("Camp width", _config.Camp.Area.ToString());
        WorldLayout layout = WorldLayout.Calculate(_config);
        foreach (string error in layout.Errors) EditorGUILayout.HelpBox(error, MessageType.Error);
        EditorGUILayout.HelpBox("Camera width is checked when the scene starts.", MessageType.Info);
        EditorGUILayout.EndScrollView();
    }

    private static void ShowWidths(string side, System.Collections.Generic.List<WorldBuildingEntry> entries)
    {
        if (entries == null) return;
        for (int i = 0; i < entries.Count; i++)
            if (entries[i] != null && entries[i].Config != null)
                EditorGUILayout.LabelField($"{side} building {i + 1} width", entries[i].Config.Area.ToString());
    }
}
