using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

public class EnvironmentLayoutTests
{
    private Type _worldType;
    private Type _worldLayoutType;
    private Type _environmentType;
    private ScriptableObject _world;

    [SetUp]
    public void SetUp()
    {
        _worldType = Type.GetType("Assets.ScriptableObjects.WorldGridConfig, Assembly-CSharp", true);
        _worldLayoutType = Type.GetType("Assets.Scripts.World.WorldLayout, Assembly-CSharp", true);
        _environmentType = Type.GetType("Assets.Scripts.World.EnvironmentLayout, Assembly-CSharp", true);
        UnityEngine.Object asset = AssetDatabase.LoadAssetAtPath("Assets/_Project/Scripts/ScriptableObjects/WorldConfig/WorldGridConfig.asset", _worldType);
        _world = UnityEngine.Object.Instantiate(asset) as ScriptableObject;
    }

    [TearDown]
    public void TearDown() => UnityEngine.Object.DestroyImmediate(_world);

    [Test]
    public void InvalidSegmentRangeBlocksGeneration()
    {
        _worldType.GetField("ForestMinLength").SetValue(_world, 0);
        object layout = Calculate();
        IList errors = (IList)_worldLayoutType.GetField("Errors").GetValue(layout);
        Assert.That(string.Join(" ", new List<string>((IEnumerable<string>)errors)), Does.Contain("Forest"));
    }

    [Test]
    public void EmptyVariantsAreAllowedOnlyForZeroDensity()
    {
        ((IList)_worldType.GetField("Trees").GetValue(_world)).Clear();
        object invalid = Calculate();
        Assert.That(_worldLayoutType.GetProperty("IsValid").GetValue(invalid), Is.False);
        _worldType.GetField("TreeDensity").SetValue(_world, 0f);
        object valid = Calculate();
        Assert.That(_worldLayoutType.GetProperty("IsValid").GetValue(valid), Is.True);
    }

    [Test]
    public void RandomLayoutsCoverBothSidesAndRespectOccupancy()
    {
        object world = Calculate();
        Assert.That(_worldLayoutType.GetProperty("IsValid").GetValue(world), Is.True);
        IList buildings = (IList)_worldLayoutType.GetField("Buildings").GetValue(world);
        int left = int.MaxValue, right = 0;
        foreach (object building in buildings)
        {
            left = Math.Min(left, Field<int>(building, "Start"));
            right = Math.Max(right, Field<int>(building, "End"));
        }
        int width = (int)_worldType.GetField("Width").GetValue(_world);
        for (int seed = 0; seed < 50; seed++)
        {
            object result = _environmentType.GetMethod("Generate").Invoke(null, new object[] { _world, world, new System.Random(seed) });
            IList segments = (IList)_environmentType.GetField("Segments").GetValue(result);
            int[] coverage = new int[width];
            foreach (object segment in segments)
                for (int i = Field<int>(segment, "Start"); i < Field<int>(segment, "End"); i++) coverage[i]++;
            for (int i = 0; i < width; i++) Assert.That(coverage[i], Is.EqualTo(i < left || i >= right ? 1 : 0));
            List<object> leftSegments = new List<object>();
            List<object> rightSegments = new List<object>();
            foreach (object segment in segments)
            {
                if (Field<int>(segment, "End") <= left) leftSegments.Add(segment);
                else rightSegments.Add(segment);
            }
            leftSegments.Sort((a, b) => Field<int>(b, "Start").CompareTo(Field<int>(a, "Start")));
            rightSegments.Sort((a, b) => Field<int>(a, "Start").CompareTo(Field<int>(b, "Start")));
            CheckAlternation(leftSegments);
            CheckAlternation(rightSegments);
            IList placements = (IList)_environmentType.GetField("Placements").GetValue(result);
            HashSet<object> trees = VariantConfigs("Trees");
            HashSet<object> rocks = VariantConfigs("Rocks");
            for (int index = 0; index < placements.Count; index++)
            {
                object placement = placements[index];
                int start = Field<int>(placement, "Start"), end = Field<int>(placement, "End");
                Assert.That(start, Is.GreaterThanOrEqualTo(0));
                Assert.That(end, Is.LessThanOrEqualTo(width));
                foreach (object building in buildings)
                    Assert.That(end <= Field<int>(building, "Start") - 1 || start >= Field<int>(building, "End") + 1, Is.True);
                object config = placement.GetType().GetField("Config").GetValue(placement);
                if (trees.Contains(config))
                {
                    bool inForest = false;
                    foreach (object segment in segments)
                        if (Field<bool>(segment, "IsForest") && start >= Field<int>(segment, "Start") && end <= Field<int>(segment, "End")) inForest = true;
                    Assert.That(inForest, Is.True);
                    Assert.That(Field<float>(placement, "Scale"), Is.InRange(0.8f, 1.2f));
                }
                for (int other = 0; other < index; other++)
                {
                    object previous = placements[other];
                    object previousConfig = previous.GetType().GetField("Config").GetValue(previous);
                    bool overlap = start < Field<int>(previous, "End") && end > Field<int>(previous, "Start");
                    if (rocks.Contains(config) || rocks.Contains(previousConfig) || trees.Contains(config) == trees.Contains(previousConfig))
                        Assert.That(overlap, Is.False);
                }
            }
        }
    }

    [Test]
    public void WideBuildingGapCanContainDecor()
    {
        IList entries = (IList)_worldType.GetField("LeftBuildings").GetValue(_world);
        entries[0].GetType().GetField("Gap").SetValue(entries[0], 12);
        _worldType.GetField("OpenRockDensity").SetValue(_world, 1f);
        _worldType.GetField("PlayerCell").SetValue(_world, 100);
        object world = Calculate();
        Assert.That(_worldLayoutType.GetProperty("IsValid").GetValue(world), Is.True);
        IList buildings = (IList)_worldLayoutType.GetField("Buildings").GetValue(world);
        int gapStart = Field<int>(buildings[1], "End");
        int gapEnd = Field<int>(buildings[0], "Start");
        object result = _environmentType.GetMethod("Generate").Invoke(null, new object[] { _world, world, new System.Random(5) });
        IList placements = (IList)_environmentType.GetField("Placements").GetValue(result);
        bool found = false;
        foreach (object placement in placements)
            if (Field<int>(placement, "Start") >= gapStart && Field<int>(placement, "End") <= gapEnd) found = true;
        Assert.That(found, Is.True);
    }

    private static void CheckAlternation(List<object> segments)
    {
        for (int i = 0; i < segments.Count; i++)
            Assert.That(Field<bool>(segments[i], "IsForest"), Is.EqualTo(i % 2 == 0));
    }

    private HashSet<object> VariantConfigs(string name)
    {
        HashSet<object> result = new HashSet<object>();
        foreach (object variant in (IList)_worldType.GetField(name).GetValue(_world))
            result.Add(variant.GetType().GetField("Config").GetValue(variant));
        return result;
    }

    private object Calculate() => _worldLayoutType.GetMethod("Calculate").Invoke(null, new object[] { _world, false });
    private static T Field<T>(object value, string name) => (T)value.GetType().GetField(name).GetValue(value);
}
