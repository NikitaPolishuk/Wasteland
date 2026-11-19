using System;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using Zenject;

public class BuildingHealthTests
{
    private const string WALL_CONFIG_PATH = "Assets/_Project/Scripts/ScriptableObjects/BuildingConfig/WallBuilding.asset";
    private static readonly Type _buildingType = GameType("Assets.Scripts.Buildings.BaseBuilding");
    private static readonly Type _configType = GameType("Assets.ScriptableObjects.BuildingConfig.BuildingConfig");
    private static readonly Type _resourcesType = GameType("Assets.Scripts.Interfaces.IResourceService");
    private static readonly object _gold = Enum.Parse(GameType("Assets.Scripts.Enum.ResourceType"), "Gold");

    private ScriptableObject _config;
    private ScriptableObject _playerConfig;
    private GameObject _instance;
    private GameObject _player;
    private DiContainer _container;
    private IDisposable _input;
    private IDisposable _interactables;

    [SetUp]
    public void SetUpGameplayServices()
    {
        _container = new DiContainer();
        Type playerConfigType = GameType("Assets.ScriptableObjects.PlayerConfig");
        _playerConfig = ScriptableObject.CreateInstance(playerConfigType);
        _container.Bind(playerConfigType).FromInstance(_playerConfig);
        _container.Bind(GameType("Assets.Scripts.Wallet.ResourceWallet")).AsSingle();
        _container.Bind(_resourcesType).To(GameType("Assets.Scripts.Wallet.PlayerResourceService")).AsSingle();

        Type playerType = GameType("Assets.Scripts.Character.Player");
        _player = new GameObject("Test player");
        _container.Bind(playerType).FromInstance(_player.AddComponent(playerType));
        Type inputType = GameType("Assets.Scripts.Interfaces.IInput");
        _container.Bind(inputType).To(GameType("Assets.Scripts.Input.KeyboardInputController")).AsSingle();
        _input = (IDisposable)_container.Resolve(inputType);
        Type interactablesType = GameType("Assets.Scripts.Interfaces.IInteractableService");
        _container.Bind(interactablesType).To(GameType("Assets.Scripts.Character.InteractableService")).AsSingle();
        _interactables = (IDisposable)_container.Resolve(interactablesType);
    }

    [TearDown]
    public void TearDownGameplayServices()
    {
        UnityEngine.Object.DestroyImmediate(_instance);
        UnityEngine.Object.DestroyImmediate(_config);
        UnityEngine.Object.DestroyImmediate(_player);
        UnityEngine.Object.DestroyImmediate(_playerConfig);
        _interactables?.Dispose();
        _input?.Dispose();
    }

    [Test]
    public void SuccessfulUpgradeFullyRestoresDamagedBuildingAtTheNewMaximum()
    {
        Component building = CreateWall((100, 0), (175, 5));
        object resources = _container.Resolve(_resourcesType);
        int startingGold = GetGold(resources);
        AssertState(building, level: 0, currentHp: 100, maxHp: 100);

        TakeDamage(building, 35);
        AssertState(building, level: 0, currentHp: 65, maxHp: 100);

        Interact(building);

        AssertState(building, level: 1, currentHp: 175, maxHp: 175);
        Assert.That(GetGold(resources), Is.EqualTo(startingGold - 5));
    }

    [Test]
    public void UnaffordableNextUpgradePreservesDamageAndCanBeRetried()
    {
        Component building = CreateWall((100, 0), (175, 5), (140, 8));
        object resources = _container.Resolve(_resourcesType);
        int startingGold = GetGold(resources);
        bool spent = (bool)_resourcesType.GetMethod("TrySpend").Invoke(resources, new[] { _gold, (object)(startingGold - 6) });
        Assert.That(spent, Is.True);

        Interact(building);
        AssertState(building, level: 1, currentHp: 175, maxHp: 175);
        Assert.That(GetGold(resources), Is.EqualTo(1));
        TakeDamage(building, 50);

        Interact(building);

        AssertState(building, level: 1, currentHp: 125, maxHp: 175);
        Assert.That(GetGold(resources), Is.EqualTo(1));

        _resourcesType.GetMethod("Add").Invoke(resources, new[] { _gold, (object)7 });
        Interact(building);

        AssertState(building, level: 2, currentHp: 140, maxHp: 140);
        Assert.That(GetGold(resources), Is.Zero);
    }

    [Test]
    public void BuildingWithOnlyLevelZeroCannotBeUpgradedOrCharged()
    {
        Component building = CreateWall((100, 0));
        object resources = _container.Resolve(_resourcesType);
        int startingGold = GetGold(resources);
        TakeDamage(building, 35);

        Interact(building);

        Assert.That(_buildingType.GetProperty("CanInteract").GetValue(building), Is.False);
        AssertState(building, level: 0, currentHp: 65, maxHp: 100);
        Assert.That(GetGold(resources), Is.EqualTo(startingGold));
    }

    private Component CreateWall(params (int maxHp, int cost)[] levelSettings)
    {
        ScriptableObject wall = AssetDatabase.LoadAssetAtPath<ScriptableObject>(WALL_CONFIG_PATH);
        _config = UnityEngine.Object.Instantiate(wall);
        SerializedObject serializedConfig = new SerializedObject(_config);
        SerializedProperty levels = serializedConfig.FindProperty("Levels");
        levels.arraySize = levelSettings.Length;

        for (int index = 0; index < levelSettings.Length; index++)
        {
            SerializedProperty level = levels.GetArrayElementAtIndex(index);
            level.FindPropertyRelative("Level").intValue = index;
            level.FindPropertyRelative("MaxHp").intValue = levelSettings[index].maxHp;
            level.FindPropertyRelative("UpgradeCost").intValue = levelSettings[index].cost;
        }

        serializedConfig.ApplyModifiedPropertiesWithoutUndo();
        Type factoryType = GameType("Assets.Scripts.Factories.BuildingFactory");
        Array configs = Array.CreateInstance(_configType, 1);
        configs.SetValue(_config, 0);
        object factory = Activator.CreateInstance(factoryType, _container, configs);
        _instance = (GameObject)factoryType.GetMethod("Create", new[] { _configType, typeof(Vector2) })
            .Invoke(factory, new object[] { _config, Vector2.zero });
        return _instance.GetComponent(_buildingType);
    }

    private static void Interact(Component building)
    {
        _buildingType.GetMethod("Interactable").Invoke(building, null);
    }

    private static void TakeDamage(Component building, int amount)
    {
        _buildingType.GetMethod("TakeDamage").Invoke(building, new object[] { amount });
    }

    private static void AssertState(Component building, int level, int currentHp, int maxHp)
    {
        Assert.That(_buildingType.GetProperty("Level").GetValue(building), Is.EqualTo(level), "Building level");
        Assert.That(_buildingType.GetProperty("CurrentHp").GetValue(building), Is.EqualTo(currentHp), "Current HP");
        Assert.That(_buildingType.GetProperty("MaxHp").GetValue(building), Is.EqualTo(maxHp), "Maximum HP");
    }

    private static int GetGold(object resources)
    {
        return (int)_resourcesType.GetMethod("GetAmount").Invoke(resources, new[] { _gold });
    }

    private static Type GameType(string name)
    {
        return Type.GetType(name + ", Assembly-CSharp", true);
    }
}
