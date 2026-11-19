using System;
using NUnit.Framework;

public class BuildingSelectionTests
{
    [Test]
    public void SelectsNearestAvailableAndKeepsCurrentOnTie()
    {
        Assert.That(Select(new[] { 2f, 1f, 1f }, new[] { true, true, true }, -1), Is.EqualTo(1));
        Assert.That(Select(new[] { 2f, 1f, 1f }, new[] { true, true, true }, 2), Is.EqualTo(2));
        Assert.That(Select(new[] { 2f, 1f, 1f }, new[] { true, false, false }, 2), Is.EqualTo(0));
        Assert.That(Select(new[] { 2f }, new[] { false }, 0), Is.EqualTo(-1));
        Assert.That(Select(new float[0], new bool[0], -1), Is.EqualTo(-1));
    }

    private static int Select(float[] distances, bool[] available, int current)
    {
        Type selector = Type.GetType("Assets.Scripts.Character.BuildingSelection, Assembly-CSharp", true);
        return (int)selector.GetMethod("Select").Invoke(null, new object[] { distances, available, current });
    }
}
