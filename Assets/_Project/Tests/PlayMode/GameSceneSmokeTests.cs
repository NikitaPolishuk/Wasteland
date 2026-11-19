#if UNITY_EDITOR
using System;
using System.Collections;
using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

public class GameSceneSmokeTests
{
    [UnityTest]
    public IEnumerator LoadsGameSceneAndInstallsPlayer()
    {
        yield return EditorSceneManager.LoadSceneAsyncInPlayMode("Assets/_Project/Scenes/GameScene.unity", new LoadSceneParameters(LoadSceneMode.Single));
        yield return null;
        yield return null;
        Type player = Type.GetType("Assets.Scripts.Character.Player, Assembly-CSharp", true);
        Assert.That(UnityEngine.Object.FindFirstObjectByType(player), Is.Not.Null);
        UnityEngine.Tilemaps.Tilemap[] tilemaps = UnityEngine.Object.FindObjectsByType<UnityEngine.Tilemaps.Tilemap>(FindObjectsSortMode.None);
        Assert.That(Array.Exists(tilemaps, tilemap => tilemap.GetUsedTilesCount() > 0), Is.True, "World ground should be generated.");
    }
}
#endif
