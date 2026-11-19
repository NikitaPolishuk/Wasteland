using System;
using NUnit.Framework;

public class BuildingUpgradeTests
{
    [Test]
    public void PaysOneLevelAtATimeAndKeepsBalanceWhenUnaffordable()
    {
        Type upgradeType = Type.GetType("Assets.Scripts.Buildings.BuildingUpgrade, Assembly-CSharp", true);
        int balance = 5;
        Func<int, bool> spend = cost => { if (cost > balance || cost < 0) return false; balance -= cost; return true; };
        object upgrade = Activator.CreateInstance(upgradeType, new object[] { new[] { 0, 5, 3 }, spend });
        Assert.That((int)upgradeType.GetProperty("NextCost").GetValue(upgrade), Is.EqualTo(5));
        Assert.That((bool)upgradeType.GetMethod("TryUpgrade").Invoke(upgrade, null), Is.True);
        Assert.That((int)upgradeType.GetProperty("Level").GetValue(upgrade), Is.EqualTo(1));
        Assert.That(balance, Is.EqualTo(0));
        Assert.That((bool)upgradeType.GetMethod("TryUpgrade").Invoke(upgrade, null), Is.False);
        Assert.That((int)upgradeType.GetProperty("Level").GetValue(upgrade), Is.EqualTo(1));
        Assert.That((int)upgradeType.GetProperty("NextCost").GetValue(upgrade), Is.EqualTo(3));
    }

    [Test]
    public void FreeUpgradeWorksAndMaximumNeverCharges()
    {
        Type upgradeType = Type.GetType("Assets.Scripts.Buildings.BuildingUpgrade, Assembly-CSharp", true);
        int calls = 0;
        Func<int, bool> spend = cost => { calls++; return cost == 0; };
        object upgrade = Activator.CreateInstance(upgradeType, new object[] { new[] { 0, 0 }, spend });
        Assert.That((int)upgradeType.GetProperty("NextCost").GetValue(upgrade), Is.EqualTo(0));
        Assert.That((bool)upgradeType.GetMethod("TryUpgrade").Invoke(upgrade, null), Is.True);
        Assert.That((bool)upgradeType.GetProperty("CanUpgrade").GetValue(upgrade), Is.False);
        Assert.That((bool)upgradeType.GetMethod("TryUpgrade").Invoke(upgrade, null), Is.False);
        Assert.That(calls, Is.EqualTo(1));
    }
}
