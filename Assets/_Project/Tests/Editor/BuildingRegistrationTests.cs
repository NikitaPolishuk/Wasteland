using System;
using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using Zenject;
using Object = UnityEngine.Object;

public class BuildingRegistrationTests
{
    private const string CONFIG_FOLDER = "Assets/_Project/Scripts/ScriptableObjects/BuildingConfig";
    private const string WALL_CONFIG = CONFIG_FOLDER + "/WallBuilding.asset";
    private const string CAMP_CONFIG = CONFIG_FOLDER + "/CampBuilding.asset";

    [Test]
    public void UnconfiguredTypeCanSaveDraftAndRegisterWithoutSceneAssignment()
    {
        Type editor = Type.GetType("BuildingConfigEditor, Assembly-CSharp-Editor", true);
        Object originalWall = AssetDatabase.LoadMainAssetAtPath(WALL_CONFIG);
        SerializedObject template = new SerializedObject(originalWall);
        GameObject prefab = (GameObject)template.FindProperty("BuildingPrefab").objectReferenceValue;
        GameObject appearance = (GameObject)template.FindProperty("Levels").GetArrayElementAtIndex(0)
            .FindPropertyRelative("AppearancePrefab").objectReferenceValue;
        object type = Enum.Parse(Type.GetType("Assets.Scripts.Enum.BuildingType, Assembly-CSharp", true), "Wall");
        string holdingFolder = Path.Combine("Temp", Guid.NewGuid().ToString());
        Directory.CreateDirectory(holdingFolder);
        string heldAsset = Path.Combine(holdingFolder, "WallBuilding.asset");
        Object config = null;
        string path = null;
        string[] resourceFolders = AssetDatabase.GetSubFolders("Assets/Resources");

        try
        {
            File.Move(WALL_CONFIG, heldAsset);
            File.Move(WALL_CONFIG + ".meta", heldAsset + ".meta");
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);

            object[] create = { type, null, null };
            Assert.That((bool)editor.GetMethod("TryCreateConfig").Invoke(null, create), Is.True, () => (string)create[2]);
            config = (Object)create[1];
            path = AssetDatabase.GetAssetPath(config);
            Assert.That(File.Exists(path), Is.True);
            Assert.That(AssetDatabase.AssetPathToGUID(path), Is.Not.Empty);
            Assert.That((Array)editor.GetMethod("GetTypes").Invoke(null, null), Has.Member(type));
            Assert.That((string[])editor.GetMethod("GetErrors").Invoke(null, new object[] { config }),
                Has.Member("Main prefab is required."));
            Assert.That(RegisteredConfigs(config.GetType()), Has.No.Member(config));
            AssertDraftCannotSpawn(config, type);

            Assert.That(Edit(editor, "TrySetArea", config, 4), Is.Null);
            Assert.That(File.ReadAllText(path), Does.Contain("Area: 4"));
            Assert.That(Edit(editor, "TrySetBuildingPrefab", config, prefab), Is.Null);
            Assert.That(RegisteredConfigs(config.GetType()), Has.No.Member(config));
            Assert.That(Edit(editor, "TrySetLevel", config, 0, appearance, 20, 0), Is.Null);

            Object camp = AssetDatabase.LoadMainAssetAtPath(CAMP_CONFIG);
            Assert.That(RegisteredConfigs(config.GetType(), camp), Is.EquivalentTo(new[] { camp, config }));
            Assert.That(Edit(editor, "TrySetArea", config, 0), Is.Not.Null);
            Assert.That(Edit(editor, "TrySetDisplayName", config, "cAmP"), Is.Not.Null);
            string guid = AssetDatabase.AssetPathToGUID(path);
            Assert.That(Edit(editor, "TrySetDisplayName", config, "Defensive wall"), Is.Null);
            Assert.That(AssetDatabase.AssetPathToGUID(path), Is.EqualTo(guid));
            Assert.That(config.GetType().GetProperty("DisplayName").GetValue(config), Is.EqualTo("Defensive wall"));
            Object[] registered = RegisteredConfigs(config.GetType(), camp);
            Assert.That(registered, Is.EquivalentTo(new[] { camp, config }));
            AssertCanSpawnTwoInstances(registered, config, type);
        }
        finally
        {
            try
            {
                if (config != null) Undo.ClearUndo(config);
                if (string.IsNullOrEmpty(path) == false)
                {
                    AssetDatabase.DeleteAsset(path);
                    string configFolder = Path.GetDirectoryName(path).Replace('\\', '/');
                    if (resourceFolders.Contains(configFolder) == false && Directory.Exists(configFolder) &&
                        Directory.EnumerateFileSystemEntries(configFolder).Any() == false)
                    {
                        AssetDatabase.DeleteAsset(configFolder);
                    }
                }
            }
            finally
            {
                if (File.Exists(heldAsset)) File.Move(heldAsset, WALL_CONFIG);
                if (File.Exists(heldAsset + ".meta")) File.Move(heldAsset + ".meta", WALL_CONFIG + ".meta");
                Directory.Delete(holdingFolder);
                AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
            }
        }
    }

    [Test]
    public void SceneBuiltinsKeepTheirExistingRegistrations()
    {
        Object camp = AssetDatabase.LoadMainAssetAtPath(CAMP_CONFIG);
        Object wall = AssetDatabase.LoadMainAssetAtPath(WALL_CONFIG);
        Assert.That(RegisteredConfigs(camp.GetType(), camp, wall), Is.EquivalentTo(new[] { camp, wall }));
    }

    private static Object[] RegisteredConfigs(Type configType, params Object[] configs)
    {
        Type catalog = Type.GetType("Assets.Scripts.Buildings.BuildingCatalog, Assembly-CSharp", true);
        Array sceneConfigs = Array.CreateInstance(configType, configs.Length);
        Array.Copy(configs, sceneConfigs, configs.Length);
        Array registered = (Array)catalog.GetMethod("Load").Invoke(null, new object[] { sceneConfigs });
        return registered.Cast<Object>().ToArray();
    }

    private static void AssertDraftCannotSpawn(Object config, object buildingType)
    {
        Type factoryType = Type.GetType("Assets.Scripts.Factories.BuildingFactory, Assembly-CSharp", true);
        Type configType = config.GetType();
        Array configs = Array.CreateInstance(configType, 1);
        configs.SetValue(config, 0);
        object factory = Activator.CreateInstance(factoryType, null, configs);
        Assert.That(factoryType.GetMethod("Create", new[] { configType, typeof(Vector2) })
            .Invoke(factory, new object[] { config, Vector2.zero }), Is.Null);
        Assert.That(factoryType.GetMethod("Create", new[] { buildingType.GetType(), typeof(Vector2) })
            .Invoke(factory, new[] { buildingType, (object)Vector2.zero }), Is.Null);
    }

    private static void AssertCanSpawnTwoInstances(Object[] registered, Object config, object buildingType)
    {
        DiContainer container = new DiContainer();
        container.Bind(Type.GetType("Assets.Scripts.Interfaces.IInteractableService, Assembly-CSharp", true)).FromInstance(null);
        container.Bind(Type.GetType("Assets.Scripts.Interfaces.IResourceService, Assembly-CSharp", true)).FromInstance(null);
        Type factoryType = Type.GetType("Assets.Scripts.Factories.BuildingFactory, Assembly-CSharp", true);
        Type configType = config.GetType();
        Array configs = Array.CreateInstance(configType, registered.Length);
        Array.Copy(registered, configs, registered.Length);
        object factory = Activator.CreateInstance(factoryType, container, configs);
        GameObject first = null;
        GameObject second = null;

        try
        {
            first = (GameObject)factoryType.GetMethod("Create", new[] { buildingType.GetType(), typeof(Vector2) })
                .Invoke(factory, new[] { buildingType, (object)Vector2.zero });
            second = (GameObject)factoryType.GetMethod("Create", new[] { configType, typeof(Vector2) })
                .Invoke(factory, new object[] { config, Vector2.right });
            Assert.That(first, Is.Not.Null);
            Assert.That(second, Is.Not.Null.And.Not.SameAs(first));
            Assert.That(first.transform.position, Is.EqualTo(Vector3.zero));
            Assert.That(second.transform.position, Is.EqualTo(Vector3.right));
        }
        finally
        {
            Object.DestroyImmediate(second);
            Object.DestroyImmediate(first);
        }
    }

    private static string Edit(Type editor, string method, Object config, params object[] arguments)
    {
        object[] parameters = new object[arguments.Length + 2];
        parameters[0] = config;
        Array.Copy(arguments, 0, parameters, 1, arguments.Length);
        bool saved = (bool)editor.GetMethod(method).Invoke(null, parameters);
        return saved ? null : (string)parameters[parameters.Length - 1];
    }
}
