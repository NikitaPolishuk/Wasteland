using System;
using System.IO;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

public class BuildingTypeDataTests
{
    private const string WALL_CONFIG = "Assets/_Project/Scripts/ScriptableObjects/BuildingConfig/WallBuilding.asset";
    private string _path;
    private ScriptableObject _config;
    private Type _example;
    private Type _editor;

    [SetUp]
    public void SetUp()
    {
        _path = AssetDatabase.GenerateUniqueAssetPath("Assets/BuildingTypeDataTest.asset");
        Assert.That(AssetDatabase.CopyAsset(WALL_CONFIG, _path), Is.True);
        _config = AssetDatabase.LoadAssetAtPath<ScriptableObject>(_path);
        SerializedObject serialized = new SerializedObject(_config);
        serialized.FindProperty("BuildingType").intValue = 777;
        serialized.ApplyModifiedPropertiesWithoutUndo();
        _example = Type.GetType("BuildingTypeDataTestExample, Assembly-CSharp-Editor", true);
        _editor = Type.GetType("BuildingTypeDataEditor, Assembly-CSharp-Editor", true);
    }

    [TearDown]
    public void TearDown()
    {
        Undo.ClearUndo(_config);
        AssetDatabase.DeleteAsset(_path);
    }

    [Test]
    public void TypeSettingsAreValidatedSavedAndUndoableWithoutChangingCommonSettings()
    {
        Assert.That(Save(4), Is.Null);
        Undo.IncrementCurrentGroup();
        Assert.That(File.ReadAllText(_path), Does.Contain("Radius: 4"));
        Assert.That(Save(0), Does.Contain("Radius must be positive"));
        Assert.That(Save(9), Is.Null);
        Assert.That(ReadRadius(), Is.EqualTo(9));
        Assert.That(new SerializedObject(_config).FindProperty("Area").intValue, Is.EqualTo(5));

        Undo.PerformUndo();
        Assert.That(ReadRadius(), Is.EqualTo(4));
        Assert.That(File.ReadAllText(_path), Does.Contain("Radius: 4"));
        Undo.PerformRedo();
        Assert.That(ReadRadius(), Is.EqualTo(9));
    }

    [Test]
    public void DraftAllowsIncrementalTypeSettingsAndShowsValidationUntilRepaired()
    {
        SerializedObject serialized = new SerializedObject(_config);
        serialized.FindProperty("_isEditorManaged").boolValue = true;
        serialized.FindProperty("Area").intValue = 0;
        serialized.ApplyModifiedPropertiesWithoutUndo();
        AssetDatabase.SaveAssetIfDirty(_config);

        Assert.That(Save(0), Is.Null);
        Assert.That(ReadRadius(), Is.Zero);
        string[] errors = (string[])_config.GetType().GetMethod("GetValidationErrors").Invoke(_config, null);
        Assert.That(errors, Does.Contain("Radius must be positive."));
        Assert.That((bool)_config.GetType().GetProperty("IsAvailableInGame").GetValue(_config), Is.False);
        Undo.IncrementCurrentGroup();

        Assert.That(Save(3), Is.Null);
        errors = (string[])_config.GetType().GetMethod("GetValidationErrors").Invoke(_config, null);
        Assert.That(errors, Does.Not.Contain("Radius must be positive."));
        Assert.That(errors, Has.Some.Contains("Area must be positive"));
        Undo.PerformUndo();
        Assert.That(ReadRadius(), Is.Zero);
    }

    [Test]
    public void BuildingBehaviorReadsTheSavedTypeSettings()
    {
        Assert.That(Save(6), Is.Null);
        SerializedObject config = new SerializedObject(_config);
        SerializedProperty levels = config.FindProperty("Levels");
        for (int index = 0; index < levels.arraySize; index++)
            levels.GetArrayElementAtIndex(index).FindPropertyRelative("MaxHp").intValue = 100;
        config.ApplyModifiedPropertiesWithoutUndo();
        GameObject prefab = (GameObject)config.FindProperty("BuildingPrefab").objectReferenceValue;
        GameObject instance = UnityEngine.Object.Instantiate(prefab);
        try
        {
            Type baseType = Type.GetType("Assets.Scripts.Buildings.BaseBuilding, Assembly-CSharp", true);
            Component original = instance.GetComponent(baseType);
            Component example = instance.AddComponent(Type.GetType("BuildingTypeDataTestBuilding, Assembly-CSharp-Editor", true));
            SerializedObject source = new SerializedObject(original);
            SerializedObject target = new SerializedObject(example);
            target.FindProperty("_buildPrice").objectReferenceValue = source.FindProperty("_buildPrice").objectReferenceValue;
            target.FindProperty("_healthSystem").objectReferenceValue = source.FindProperty("_healthSystem").objectReferenceValue;
            target.ApplyModifiedPropertiesWithoutUndo();
            baseType.GetMethod("Initialize").Invoke(example, new object[] { _config });
            Assert.That(example.GetType().GetProperty("Radius").GetValue(example), Is.EqualTo(6));
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(instance);
        }
    }

    private string Save(int radius)
    {
        object data = Activator.CreateInstance(_example);
        _example.GetField("Radius").SetValue(data, radius);
        object[] args = { _config, data, null };
        bool saved = (bool)_editor.GetMethod("TrySetData").Invoke(null, args);
        return saved ? null : (string)args[2];
    }

    private int ReadRadius()
    {
        SerializedProperty data = new SerializedObject(_config).FindProperty("_typeData");
        return data.FindPropertyRelative("Radius").intValue;
    }
}
