using System;
using System.Collections.Generic;
using Assets.ScriptableObjects;
using Assets.Installer;
using Assets.Scripts.World;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

public class WorldSettingsWindow : EditorWindow
{
    private const float PREVIEW_HEIGHT = 180f;
    private const float PREVIEW_MARGIN = 12f;
    private WorldGridConfig _config;
    private Vector2 _scroll;
    private int _previewSeed;
    private System.Random _seedSource;
    private int _sceneHandle;

    [MenuItem("Wasteland/World Settings")]
    public static void Open() => GetWindow<WorldSettingsWindow>("World Settings");

    private void OnEnable()
    {
        _seedSource = new System.Random();
        _previewSeed = _seedSource.Next();
        LoadSceneConfiguration();
    }

    private void OnGUI()
    {
        Scene activeScene = SceneManager.GetActiveScene();
        if (_sceneHandle != activeScene.handle) LoadSceneConfiguration();

        GameplaySceneInstaller installer = FindSceneInstaller(activeScene, out bool multipleInstallers);
        EditorGUI.BeginDisabledGroup(EditorApplication.isPlayingOrWillChangePlaymode);
        EditorGUI.BeginChangeCheck();
        WorldGridConfig selectedConfig = (WorldGridConfig)EditorGUILayout.ObjectField("World configuration", _config, typeof(WorldGridConfig), false);
        if (EditorGUI.EndChangeCheck()) SetSceneConfiguration(installer, multipleInstallers, selectedConfig);
        if (GUILayout.Button("Create configuration"))
        {
            string path = EditorUtility.SaveFilePanelInProject("Create world configuration", "WorldGridConfig", "asset", "Choose a location");
            if (string.IsNullOrEmpty(path) == false)
            {
                WorldGridConfig newConfig = CreateInstance<WorldGridConfig>();
                AssetDatabase.CreateAsset(newConfig, path);
                AssetDatabase.SaveAssets();
                SetSceneConfiguration(installer, multipleInstallers, newConfig);
            }
        }
        EditorGUI.EndDisabledGroup();
        if (installer == null || multipleInstallers)
            EditorGUILayout.HelpBox(multipleInstallers ? "The active scene has multiple gameplay installers. Select a scene with exactly one installer to assign the configuration." : "The active scene has no gameplay installer. Open the gameplay scene to assign the configuration.", MessageType.Warning);
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
        Camera camera = Camera.main;
        Tilemap tilemap = installer == null ? null : new SerializedObject(installer).FindProperty("_groundTilemap").objectReferenceValue as Tilemap;
        if (camera == null || tilemap == null)
            EditorGUILayout.HelpBox("Camera width cannot be checked: the scene camera or ground Tilemap is unavailable. It will be checked at startup.", MessageType.Info);
        else if (WorldCameraWidthMonitor.IsTooWide(camera, tilemap, _config.Width))
            EditorGUILayout.HelpBox("World is narrower than the visible camera area, or camera is not orthographic.", MessageType.Error);
        if (GUILayout.Button("New variant")) _previewSeed = unchecked(_previewSeed + 1);
        if (layout.IsValid)
        {
            EnvironmentLayout environment = EnvironmentLayout.Generate(_config, layout, new System.Random(_previewSeed));
            DrawPreview(_config, layout, environment);
        }
        else
        {
            EditorGUILayout.HelpBox("Fix the errors above to display the preview.", MessageType.Warning);
        }
        EditorGUILayout.EndScrollView();
    }

    private void LoadSceneConfiguration()
    {
        Scene scene = SceneManager.GetActiveScene();
        _sceneHandle = scene.handle;
        GameplaySceneInstaller installer = FindSceneInstaller(scene, out bool multipleInstallers);
        _config = installer == null || multipleInstallers ? null :
            new SerializedObject(installer).FindProperty("_worldGridConfig").objectReferenceValue as WorldGridConfig;
    }

    private void SetSceneConfiguration(GameplaySceneInstaller installer, bool multipleInstallers, WorldGridConfig config)
    {
        _config = config;
        if (installer == null || multipleInstallers) return;

        SerializedObject serializedInstaller = new SerializedObject(installer);
        SerializedProperty worldConfig = serializedInstaller.FindProperty("_worldGridConfig");
        if (worldConfig.objectReferenceValue == config) return;
        worldConfig.objectReferenceValue = config;
        serializedInstaller.ApplyModifiedProperties();
    }

    private static GameplaySceneInstaller FindSceneInstaller(Scene scene, out bool multipleInstallers)
    {
        multipleInstallers = false;
        if (scene.IsValid() == false || scene.isLoaded == false) return null;

        GameplaySceneInstaller found = null;
        foreach (GameObject root in scene.GetRootGameObjects())
        {
            foreach (GameplaySceneInstaller installer in root.GetComponentsInChildren<GameplaySceneInstaller>(true))
            {
                if (found != null)
                {
                    multipleInstallers = true;
                    return null;
                }
                found = installer;
            }
        }
        return found;
    }

    private static void DrawPreview(WorldGridConfig config, WorldLayout layout, EnvironmentLayout environment)
    {
        EditorGUILayout.LabelField("World preview", EditorStyles.boldLabel);
        Rect area = GUILayoutUtility.GetRect(0, PREVIEW_HEIGHT, GUILayout.ExpandWidth(true));
        EditorGUI.DrawRect(area, new Color(0.13f, 0.13f, 0.13f));
        float left = area.x + PREVIEW_MARGIN;
        float width = Mathf.Max(1f, area.width - PREVIEW_MARGIN * 2);
        float groundY = area.y + 110f;
        float scale = width / config.Width;
        EditorGUI.DrawRect(new Rect(left, groundY, width, 3f), new Color(0.65f, 0.49f, 0.3f));
        foreach (EnvironmentSegment segment in environment.Segments)
        {
            Rect segmentRect = new Rect(left + segment.Start * scale, groundY + 5f, (segment.End - segment.Start) * scale, 20f);
            EditorGUI.DrawRect(segmentRect, segment.IsForest ? new Color(0.2f, 0.45f, 0.25f) : new Color(0.55f, 0.53f, 0.32f));
            GUI.Label(segmentRect, segment.IsForest ? "Forest" : "Open", EditorStyles.miniLabel);
        }
        foreach (EnvironmentPlacement placement in environment.Placements)
        {
            float x = left + placement.Start * scale;
            float markerWidth = Mathf.Max(2f, (placement.End - placement.Start) * scale);
            EditorGUI.DrawRect(new Rect(x, groundY - 8f, markerWidth, 6f), new Color(0.75f, 0.75f, 0.65f));
        }
        List<WorldBuildingPlacement> buildings = new List<WorldBuildingPlacement>(layout.Buildings);
        buildings.Sort((a, b) => a.Start.CompareTo(b.Start));
        for (int i = 0; i < buildings.Count; i++)
        {
            WorldBuildingPlacement building = buildings[i];
            Rect buildingRect = new Rect(left + building.Start * scale, groundY - 38f, Mathf.Max(1f, (building.End - building.Start) * scale), 30f);
            EditorGUI.DrawRect(buildingRect, building.Config == config.Camp ? new Color(0.7f, 0.4f, 0.2f) : new Color(0.4f, 0.5f, 0.7f));
            GUI.Label(new Rect(buildingRect.x, buildingRect.y - 18f, Mathf.Max(70f, buildingRect.width), 18f), $"{building.Config.name} ({building.End - building.Start})", EditorStyles.miniLabel);
            if (i > 0)
            {
                int gap = building.Start - buildings[i - 1].End;
                float gapX = left + buildings[i - 1].End * scale;
                GUI.Label(new Rect(gapX, groundY + 28f, Mathf.Max(35f, gap * scale), 18f), $"Gap {gap}", EditorStyles.miniLabel);
            }
        }
        float playerX = left + (config.PlayerCell + 0.5f) * scale;
        EditorGUI.DrawRect(new Rect(playerX - 1f, groundY - 25f, 3f, 25f), Color.yellow);
        GUI.Label(new Rect(playerX, groundY - 55f, 80f, 18f), "Player", EditorStyles.miniLabel);
        EditorGUI.DrawRect(new Rect(left, groundY - 3f, 1f, 36f), Color.white);
        EditorGUI.DrawRect(new Rect(left + width - 1f, groundY - 3f, 1f, 36f), Color.white);
        GUI.Label(new Rect(left, groundY + 48f, width, 18f), $"World: {config.Width} cells | Decor: pale marks", EditorStyles.miniLabel);
    }

    private static void ShowWidths(string side, List<WorldBuildingEntry> entries)
    {
        if (entries == null) return;
        for (int i = 0; i < entries.Count; i++)
            if (entries[i] != null && entries[i].Config != null)
                EditorGUILayout.LabelField($"{side} building {i + 1} width", entries[i].Config.Area.ToString());
    }
}
