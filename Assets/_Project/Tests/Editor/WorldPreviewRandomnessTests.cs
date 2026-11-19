using System;
using System.Collections;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

public class WorldPreviewRandomnessTests
{
    [Test]
    public void RecalculatingWithSameRandomStatePreservesPreview()
    {
        Type configType = Type.GetType("Assets.ScriptableObjects.WorldGridConfig, Assembly-CSharp", true);
        Type worldType = Type.GetType("Assets.Scripts.World.WorldLayout, Assembly-CSharp", true);
        Type environmentType = Type.GetType("Assets.Scripts.World.EnvironmentLayout, Assembly-CSharp", true);
        UnityEngine.Object asset = AssetDatabase.LoadAssetAtPath("Assets/_Project/Scripts/ScriptableObjects/WorldConfig/ExampleWorldGeneration.asset", configType);
        ScriptableObject config = UnityEngine.Object.Instantiate(asset) as ScriptableObject;
        try
        {
            object world = worldType.GetMethod("Calculate").Invoke(null, new object[] { config, false });
            Assert.That(worldType.GetProperty("IsValid").GetValue(world), Is.True);
            object first = environmentType.GetMethod("Generate").Invoke(null, new object[] { config, world, new System.Random(123) });
            object second = environmentType.GetMethod("Generate").Invoke(null, new object[] { config, world, new System.Random(123) });
            AssertSameItems(environmentType, first, second, "Segments", new[] { "Start", "End", "IsForest" });
            AssertSameItems(environmentType, first, second, "Placements", new[] { "Start", "End", "Config", "Scale" });
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(config);
        }
    }

    private static void AssertSameItems(Type type, object first, object second, string collection, string[] fields)
    {
        IList firstItems = (IList)type.GetField(collection).GetValue(first);
        IList secondItems = (IList)type.GetField(collection).GetValue(second);
        Assert.That(secondItems.Count, Is.EqualTo(firstItems.Count));
        for (int i = 0; i < firstItems.Count; i++)
            foreach (string field in fields)
                Assert.That(secondItems[i].GetType().GetField(field).GetValue(secondItems[i]), Is.EqualTo(firstItems[i].GetType().GetField(field).GetValue(firstItems[i])));
    }
}
