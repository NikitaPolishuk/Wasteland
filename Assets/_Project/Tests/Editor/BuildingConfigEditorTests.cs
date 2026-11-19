using System;
using System.IO;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

public class BuildingConfigEditorTests
{
    private const string WALL_CONFIG = "Assets/_Project/Scripts/ScriptableObjects/BuildingConfig/WallBuilding.asset";

    private string _path;
    private UnityEngine.Object _config;
    private Type _editorType;

    [SetUp]
    public void CopyWallConfiguration()
    {
        _editorType = Type.GetType("BuildingConfigEditor, Assembly-CSharp-Editor", true);
        _path = AssetDatabase.GenerateUniqueAssetPath("Assets/BuildingConfigEditorTest.asset");
        Assert.That(AssetDatabase.CopyAsset(WALL_CONFIG, _path), Is.True);
        _config = AssetDatabase.LoadMainAssetAtPath(_path);
    }

    [TearDown]
    public void RemoveTemporaryConfiguration()
    {
        if (_config != null) Undo.ClearUndo(_config);
        if (_path != null) AssetDatabase.DeleteAsset(_path);
    }

    [Test]
    public void AreaEditSavesImmediatelyAndUndoRestoresSavedValue()
    {
        Assert.That(Edit("TrySetArea", 7), Is.Null);
        Assert.That(ReadArea(), Is.EqualTo(7));
        Assert.That(File.ReadAllText(_path), Does.Contain("Area: 7"));

        Undo.PerformUndo();

        Assert.That(ReadArea(), Is.EqualTo(5));
        Assert.That(File.ReadAllText(_path), Does.Contain("Area: 5"));
    }

    [Test]
    public void InvalidChangesDoNotModifyACompletedConfiguration()
    {
        PrepareWallHealth(100, 120);
        string originalFile = File.ReadAllText(_path);

        Assert.That(Edit("TrySetArea", 0), Does.Contain("Area must be positive"));
        Assert.That(Edit("TrySetBuildingPrefab", ReadAppearance(0)), Does.Contain("BaseBuilding"));
        Assert.That(Edit("TrySetLevel", 1, ReadAppearance(1), 0, -1), Does.Contain("maximum health").And.Contain("upgrade cost"));
        Assert.That(Edit("TrySetLevel", 1, null, 120, 5), Does.Contain("appearance prefab is required"));
        Assert.That(File.ReadAllText(_path), Is.EqualTo(originalFile));
        Assert.That(ReadArea(), Is.EqualTo(5));
        Assert.That(ReadLevel(1).FindPropertyRelative("MaxHp").intValue, Is.EqualTo(120));
    }

    [Test]
    public void LegacyHealthErrorsAllowIncrementalRepairAndLevelZeroKeepsItsCost()
    {
        PrepareWallHealth(0, 0);
        Assert.That(Edit("TrySetLevel", 0, ReadAppearance(0), -1, 99), Does.Contain("maximum health"));
        Assert.That(Edit("TrySetLevel", 0, ReadAppearance(0), 90, 99), Is.Null);

        Assert.That(ReadLevel(0).FindPropertyRelative("MaxHp").intValue, Is.EqualTo(90));
        Assert.That(ReadLevel(0).FindPropertyRelative("UpgradeCost").intValue, Is.Zero);
        Assert.That(ReadLevel(1).FindPropertyRelative("MaxHp").intValue, Is.Zero);
        Assert.That(File.ReadAllText(_path), Does.Contain("MaxHp: 90"));

        Assert.That(Edit("TrySetLevel", 1, ReadAppearance(0), 120, 0), Is.Null);
        Assert.That(ReadAppearance(1), Is.EqualTo(ReadAppearance(0)));
        Assert.That(ReadLevel(1).FindPropertyRelative("UpgradeCost").intValue, Is.Zero);
    }

    [Test]
    public void RenamingKeepsExistingIdentityAndRejectsCaseInsensitiveDuplicates()
    {
        SerializedObject original = new SerializedObject(_config);
        int type = original.FindProperty("BuildingType").intValue;
        string guid = AssetDatabase.AssetPathToGUID(_path);

        Assert.That(Edit("TrySetDisplayName", "cAmP"), Does.Contain("already in use"));
        Assert.That(Edit("TrySetDisplayName", "  Stone Wall  "), Is.Null);

        SerializedObject updated = new SerializedObject(_config);
        Assert.That(updated.FindProperty("BuildingType").intValue, Is.EqualTo(type));
        Assert.That(updated.FindProperty("_displayName").stringValue, Is.EqualTo("Stone Wall"));
        Assert.That(AssetDatabase.AssetPathToGUID(_path), Is.EqualTo(guid));
        Assert.That(File.ReadAllText(_path), Does.Contain("Stone Wall"));
    }

    [Test]
    public void AddingAFreeLevelSavesItsSettingsAndSupportsUndoAndRedo()
    {
        PrepareWallHealth(100, 120);
        GameObject appearance = ReadAppearance(1);

        Assert.That(Edit("TryAddLevel", appearance, 150, 0), Is.Null);

        Assert.That(ReadLevels().arraySize, Is.EqualTo(3));
        Assert.That(ReadLevel(2).FindPropertyRelative("Level").intValue, Is.EqualTo(2));
        Assert.That(ReadAppearance(2), Is.EqualTo(appearance));
        Assert.That(ReadLevel(2).FindPropertyRelative("MaxHp").intValue, Is.EqualTo(150));
        Assert.That(ReadLevel(2).FindPropertyRelative("UpgradeCost").intValue, Is.Zero);
        Assert.That(File.ReadAllText(_path), Does.Contain("- Level: 2").And.Contain("MaxHp: 150"));

        Undo.PerformUndo();

        Assert.That(ReadLevels().arraySize, Is.EqualTo(2));
        Assert.That(File.ReadAllText(_path), Does.Not.Contain("MaxHp: 150"));

        Undo.PerformRedo();

        Assert.That(ReadLevels().arraySize, Is.EqualTo(3));
        Assert.That(File.ReadAllText(_path), Does.Contain("MaxHp: 150"));
    }

    [Test]
    public void IncompleteNewLevelLeavesTheReadyConfigurationUnchanged()
    {
        PrepareWallHealth(100, 120);
        string originalFile = File.ReadAllText(_path);
        GameObject appearance = ReadAppearance(1);

        Assert.That(Edit("TryAddLevel", null, 150, 0), Does.Contain("appearance prefab"));
        Assert.That(Edit("TryAddLevel", appearance, 0, 0), Does.Contain("health"));
        Assert.That(Edit("TryAddLevel", appearance, 150, -1), Does.Contain("cost"));

        Assert.That(ReadLevels().arraySize, Is.EqualTo(2));
        Assert.That(File.ReadAllText(_path), Is.EqualTo(originalFile));
    }

    [Test]
    public void RemovingAnIntermediateLevelRenumbersAndPreservesLaterSettings()
    {
        PrepareWallHealth(100, 120);
        GameObject appearance = ReadAppearance(0);
        Assert.That(Edit("TryAddLevel", appearance, 150, 15), Is.Null);
        Undo.IncrementCurrentGroup();

        Assert.That(Edit("TryRemoveLevel", 1), Is.Null);

        Assert.That(ReadLevels().arraySize, Is.EqualTo(2));
        Assert.That(ReadLevel(1).FindPropertyRelative("Level").intValue, Is.EqualTo(1));
        Assert.That(ReadAppearance(1), Is.EqualTo(appearance));
        Assert.That(ReadLevel(1).FindPropertyRelative("MaxHp").intValue, Is.EqualTo(150));
        Assert.That(ReadLevel(1).FindPropertyRelative("UpgradeCost").intValue, Is.EqualTo(15));
        Assert.That(File.ReadAllText(_path), Does.Not.Contain("- Level: 2"));

        Undo.PerformUndo();

        Assert.That(ReadLevels().arraySize, Is.EqualTo(3));
        Assert.That(ReadLevel(1).FindPropertyRelative("MaxHp").intValue, Is.EqualTo(120));
        Assert.That(File.ReadAllText(_path), Does.Contain("- Level: 2"));
    }

    [Test]
    public void RemovingALevelCanMoveALegacyHealthErrorWithoutBlockingTheEdit()
    {
        Assert.That(Edit("TryAddLevel", ReadAppearance(0), 150, 15), Is.Null);
        SerializedProperty lastLevel = ReadLevel(2);
        lastLevel.FindPropertyRelative("MaxHp").intValue = 0;
        lastLevel.serializedObject.ApplyModifiedPropertiesWithoutUndo();
        AssetDatabase.SaveAssetIfDirty(_config);
        Assert.That(Edit("TrySetLevel", 1, ReadAppearance(1), 120, 5), Is.Null);
        Undo.IncrementCurrentGroup();

        Assert.That(Edit("TryRemoveLevel", 1), Is.Null);

        Assert.That(ReadLevels().arraySize, Is.EqualTo(2));
        Assert.That(ReadLevel(1).FindPropertyRelative("Level").intValue, Is.EqualTo(1));
        Assert.That(ReadLevel(1).FindPropertyRelative("MaxHp").intValue, Is.Zero);
        Assert.That(Edit("TrySetLevel", 1, ReadAppearance(1), 150, 15), Is.Null);
    }

    [Test]
    public void OnlyLevelZeroCanRemainAndItCannotBeRemoved()
    {
        PrepareWallHealth(100, 120);
        Assert.That(Edit("TryRemoveLevel", 1), Is.Null);
        string singleLevelFile = File.ReadAllText(_path);

        Assert.That(Edit("TryRemoveLevel", 0), Does.Contain("Level 0 is required"));

        Assert.That(ReadLevels().arraySize, Is.EqualTo(1));
        Assert.That(ReadLevel(0).FindPropertyRelative("Level").intValue, Is.Zero);
        Assert.That(File.ReadAllText(_path), Is.EqualTo(singleLevelFile));
    }

    private void PrepareWallHealth(int firstLevel, int secondLevel)
    {
        SerializedObject data = new SerializedObject(_config);
        SerializedProperty levels = data.FindProperty("Levels");
        levels.GetArrayElementAtIndex(0).FindPropertyRelative("MaxHp").intValue = firstLevel;
        levels.GetArrayElementAtIndex(1).FindPropertyRelative("MaxHp").intValue = secondLevel;
        data.ApplyModifiedPropertiesWithoutUndo();
        AssetDatabase.SaveAssetIfDirty(_config);
    }

    private GameObject ReadAppearance(int index) => (GameObject)ReadLevel(index).FindPropertyRelative("AppearancePrefab").objectReferenceValue;

    private SerializedProperty ReadLevel(int index) => ReadLevels().GetArrayElementAtIndex(index);

    private SerializedProperty ReadLevels() => new SerializedObject(_config).FindProperty("Levels");

    private string Edit(string method, params object[] arguments)
    {
        object[] parameters = new object[arguments.Length + 2];
        parameters[0] = _config;
        Array.Copy(arguments, 0, parameters, 1, arguments.Length);
        bool saved = (bool)_editorType.GetMethod(method).Invoke(null, parameters);
        return saved ? null : (string)parameters[parameters.Length - 1];
    }

    private int ReadArea() => new SerializedObject(_config).FindProperty("Area").intValue;
}
