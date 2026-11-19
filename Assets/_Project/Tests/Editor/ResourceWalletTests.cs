using System;
using NUnit.Framework;

public class ResourceWalletTests
{
    [Test]
    public void SpendingIsAtomicAndAcceptsZero()
    {
        Type walletType = Type.GetType("Assets.Scripts.Wallet.ResourceWallet, Assembly-CSharp", true);
        Type goldType = Type.GetType("Assets.Scripts.Wallet.GoldResource, Assembly-CSharp", true);
        Type resourceType = Type.GetType("Assets.Scripts.Enum.ResourceType, Assembly-CSharp", true);
        object gold = Enum.Parse(resourceType, "Gold");
        object wallet = Activator.CreateInstance(walletType);
        walletType.GetMethod("RegisterResource").Invoke(wallet, new[] { Activator.CreateInstance(goldType) });
        walletType.GetMethod("Add").Invoke(wallet, new[] { gold, (object)5 });
        Assert.That(walletType.GetMethod("TrySpend").Invoke(wallet, new[] { gold, (object)6 }), Is.False);
        Assert.That(walletType.GetMethod("GetAmount").Invoke(wallet, new[] { gold }), Is.EqualTo(5));
        Assert.That(walletType.GetMethod("TrySpend").Invoke(wallet, new[] { gold, (object)0 }), Is.True);
        Assert.That(walletType.GetMethod("TrySpend").Invoke(wallet, new[] { gold, (object)5 }), Is.True);
        Assert.That(walletType.GetMethod("GetAmount").Invoke(wallet, new[] { gold }), Is.EqualTo(0));
        Assert.That(walletType.GetMethod("TrySpend").Invoke(wallet, new[] { gold, (object)(-1) }), Is.False);
    }
}
