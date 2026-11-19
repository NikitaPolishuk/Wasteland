using System;
using System.Collections;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

public class WorldLayoutTests
{
    [Test]
    public void PlayerColliderOverlapWithAdjacentBuildingIsReported()
    {
        Type worldType = Type.GetType("Assets.ScriptableObjects.WorldGridConfig, Assembly-CSharp", true);
        Type layoutType = Type.GetType("Assets.Scripts.World.WorldLayout, Assembly-CSharp", true);
        Type buildingType = Type.GetType("Assets.ScriptableObjects.BuildingConfig.BuildingConfig, Assembly-CSharp", true);
        ScriptableObject world = ScriptableObject.CreateInstance(worldType);
        ScriptableObject camp = ScriptableObject.CreateInstance(buildingType);
        GameObject prefab = new GameObject();
        GameObject grid = new GameObject("Grid", typeof(Grid));
        GameObject tilemapObject = new GameObject("Tilemap", typeof(UnityEngine.Tilemaps.Tilemap));
        tilemapObject.transform.SetParent(grid.transform);
        try
        {
            SerializedObject campData = new SerializedObject(camp);
            campData.FindProperty("Area").intValue = 3;
            campData.FindProperty("BuildingPrefab").objectReferenceValue = prefab;
            campData.ApplyModifiedPropertiesWithoutUndo();
            worldType.GetField("Width").SetValue(world, 11);
            worldType.GetField("GroundHeight").SetValue(world, 1);
            worldType.GetField("Camp").SetValue(world, camp);
            worldType.GetField("PlayerCell").SetValue(world, 3);
            object layout = layoutType.GetMethod("Calculate").Invoke(null, new object[] { world, false });
            IList errors = (IList)layoutType.GetField("Errors").GetValue(layout);
            layoutType.GetMethod("ValidatePlayerCollider").Invoke(layout, new object[] { tilemapObject.GetComponent<UnityEngine.Tilemaps.Tilemap>(), 3, 1f });
            Assert.That(errors, Has.None.Contains("Player collider overlaps a building"));
            layoutType.GetMethod("ValidatePlayerCollider").Invoke(layout, new object[] { tilemapObject.GetComponent<UnityEngine.Tilemaps.Tilemap>(), 3, 2f });
            Assert.That(errors, Has.Some.Contains("Player collider overlaps a building"));
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(world);
            UnityEngine.Object.DestroyImmediate(camp);
            UnityEngine.Object.DestroyImmediate(prefab);
            UnityEngine.Object.DestroyImmediate(grid);
        }
    }

    [Test]
    public void ReportsIndependentConfigurationErrorsTogether()
    {
        Type worldType = Type.GetType("Assets.ScriptableObjects.WorldGridConfig, Assembly-CSharp", true);
        Type layoutType = Type.GetType("Assets.Scripts.World.WorldLayout, Assembly-CSharp", true);
        ScriptableObject world = ScriptableObject.CreateInstance(worldType);
        worldType.GetField("Width").SetValue(world, 0);
        worldType.GetField("GroundHeight").SetValue(world, 0);
        object result = layoutType.GetMethod("Calculate").Invoke(null, new object[] { world, true });
        IList errors = (IList)layoutType.GetField("Errors").GetValue(result);
        Assert.That(errors.Count, Is.GreaterThanOrEqualTo(4));
        Assert.That(layoutType.GetProperty("IsValid").GetValue(result), Is.False);
        UnityEngine.Object.DestroyImmediate(world);
    }
    [Test]
    public void OddWorldPlacesBuildingsAtExactGaps()
    {
        Type worldType = Type.GetType("Assets.ScriptableObjects.WorldGridConfig, Assembly-CSharp", true);
        Type buildingType = Type.GetType("Assets.ScriptableObjects.BuildingConfig.BuildingConfig, Assembly-CSharp", true);
        Type entryType = Type.GetType("Assets.ScriptableObjects.WorldBuildingEntry, Assembly-CSharp", true);
        Type layoutType = Type.GetType("Assets.Scripts.World.WorldLayout, Assembly-CSharp", true);
        ScriptableObject world = ScriptableObject.CreateInstance(worldType);
        ScriptableObject camp = ScriptableObject.CreateInstance(buildingType);
        ScriptableObject wall = ScriptableObject.CreateInstance(buildingType);
        GameObject prefab = new GameObject();
        SerializedObject campData = new SerializedObject(camp);
        campData.FindProperty("Area").intValue = 3;
        campData.FindProperty("BuildingPrefab").objectReferenceValue = prefab;
        campData.ApplyModifiedPropertiesWithoutUndo();
        SerializedObject wallData = new SerializedObject(wall);
        wallData.FindProperty("Area").intValue = 2;
        wallData.FindProperty("BuildingType").enumValueIndex = 1;
        wallData.FindProperty("BuildingPrefab").objectReferenceValue = prefab;
        wallData.ApplyModifiedPropertiesWithoutUndo();
        worldType.GetField("Width").SetValue(world, 11);
        worldType.GetField("GroundHeight").SetValue(world, 1);
        worldType.GetField("Camp").SetValue(world, camp);
        object entry = Activator.CreateInstance(entryType);
        entryType.GetField("Config").SetValue(entry, wall);
        entryType.GetField("Gap").SetValue(entry, 1);
        ((IList)worldType.GetField("LeftBuildings").GetValue(world)).Add(entry);
        object result = layoutType.GetMethod("Calculate").Invoke(null, new object[] { world, false });
        IList buildings = (IList)layoutType.GetField("Buildings").GetValue(result);
        Assert.That(layoutType.GetField("LeftCell").GetValue(result), Is.EqualTo(-5));
        Assert.That(buildings[0].GetType().GetField("Start").GetValue(buildings[0]), Is.EqualTo(4));
        Assert.That(buildings[1].GetType().GetField("Start").GetValue(buildings[1]), Is.EqualTo(1));
        UnityEngine.Object.DestroyImmediate(world); UnityEngine.Object.DestroyImmediate(camp); UnityEngine.Object.DestroyImmediate(wall); UnityEngine.Object.DestroyImmediate(prefab);
    }
}
