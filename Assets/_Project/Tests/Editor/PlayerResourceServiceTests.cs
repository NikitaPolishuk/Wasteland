using System;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

public class PlayerResourceServiceTests
{
    [Test]
    public void StartsWithGoldFromPlayerConfig()
    {
        ScriptableObject config = ScriptableObject.CreateInstance(FindType("Assets.ScriptableObjects.PlayerConfig"));
        try
        {
            SerializedObject serializedConfig = new SerializedObject(config);
            serializedConfig.FindProperty("<StartingGold>k__BackingField").intValue = 17;
            serializedConfig.ApplyModifiedPropertiesWithoutUndo();
            object service = CreateService(config);
            Assert.That(GetGold(service), Is.EqualTo(17));
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(config);
        }
    }

    [Test]
    public void CreditAndSuccessfulDebitUpdateBalance()
    {
        ScriptableObject config = ScriptableObject.CreateInstance(FindType("Assets.ScriptableObjects.PlayerConfig"));
        try
        {
            object service = CreateService(config);
            object gold = System.Enum.Parse(FindType("Assets.Scripts.Enum.ResourceType"), "Gold");
            service.GetType().GetMethod("Add").Invoke(service, new[] { gold, (object)8 });
            Assert.That(GetGold(service), Is.EqualTo(38));
            bool spent = (bool)service.GetType().GetMethod("TrySpend").Invoke(service, new[] { gold, (object)10 });
            Assert.That(spent, Is.True);
            Assert.That(GetGold(service), Is.EqualTo(28));
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(config);
        }
    }

    [Test]
    public void InsufficientGoldDoesNotChangeBalance()
    {
        ScriptableObject config = ScriptableObject.CreateInstance(FindType("Assets.ScriptableObjects.PlayerConfig"));
        try
        {
            object service = CreateService(config);
            object gold = System.Enum.Parse(FindType("Assets.Scripts.Enum.ResourceType"), "Gold");
            bool spent = (bool)service.GetType().GetMethod("TrySpend").Invoke(service, new[] { gold, (object)31 });
            Assert.That(spent, Is.False);
            Assert.That(GetGold(service), Is.EqualTo(30));
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(config);
        }
    }
    private static Type FindType(string name)
    {
        return Type.GetType(name + ", Assembly-CSharp", true);
    }

    private static object CreateService(ScriptableObject config)
    {
        object wallet = Activator.CreateInstance(FindType("Assets.Scripts.Wallet.ResourceWallet"));
        return Activator.CreateInstance(FindType("Assets.Scripts.Wallet.PlayerResourceService"), wallet, config);
    }

    private static int GetGold(object service)
    {
        object gold = System.Enum.Parse(FindType("Assets.Scripts.Enum.ResourceType"), "Gold");
        return (int)service.GetType().GetMethod("GetAmount").Invoke(service, new[] { gold });
    }
}
