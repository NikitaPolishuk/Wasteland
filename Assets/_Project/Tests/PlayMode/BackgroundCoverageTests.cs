using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class BackgroundCoverageTests
{
    private GameObject _cameraObject;
    private GameObject _background;
    private Texture2D _texture;
    private Sprite _sprite;
    private Camera _camera;

    [UnitySetUp]
    public IEnumerator SetUp()
    {
        _cameraObject = new GameObject("Background test camera");
        _camera = _cameraObject.AddComponent<Camera>();
        _camera.orthographic = true;
        _camera.orthographicSize = 1;
        _camera.aspect = 2;
        _texture = new Texture2D(2, 2);
        _sprite = Sprite.Create(_texture, new Rect(0, 0, 2, 2), new Vector2(0.5f, 0.5f), 1);
        _background = new GameObject("Background test layer");
        for (int index = -1; index <= 1; index++)
        {
            GameObject tile = new GameObject("Tile " + index);
            tile.transform.SetParent(_background.transform);
            tile.transform.localPosition = new Vector3(index * 2, 0, 0);
            tile.AddComponent<SpriteRenderer>().sprite = _sprite;
        }
        Type controllerType = Type.GetType("Systems.BackgroundController, Assembly-CSharp", true);
        Component controller = _background.AddComponent(controllerType);
        controllerType.GetField("_cam").SetValue(controller, _cameraObject);
        controllerType.GetField("ParallaxEffect").SetValue(controller, 0.5f);
        yield return null;
        yield return new WaitForFixedUpdate();
    }

    [UnityTearDown]
    public IEnumerator TearDown()
    {
        UnityEngine.Object.Destroy(_background);
        UnityEngine.Object.Destroy(_cameraObject);
        UnityEngine.Object.Destroy(_sprite);
        UnityEngine.Object.Destroy(_texture);
        yield return null;
    }

    [UnityTest]
    public IEnumerator LayerCoversCameraDuringMovementAndResize()
    {
        float[] positions = { 2f, -2f, 100f, -100f, 0f };
        foreach (float position in positions)
        {
            _cameraObject.transform.position = new Vector3(position, 0, -10);
            yield return new WaitForFixedUpdate();
            yield return null;
            AssertCoverage();
        }

        _camera.orthographicSize = 3;
        yield return null;
        AssertCoverage();
    }

    [UnityTest]
    public IEnumerator SiblingGroupsRepeatWithoutOverlappingOrCopyingControllers()
    {
        UnityEngine.Object.Destroy(_background);
        yield return null;
        _background = new GameObject("Repeated sibling groups");
        Type controllerType = Type.GetType("Systems.BackgroundController, Assembly-CSharp", true);
        for (int index = -1; index <= 1; index++)
        {
            GameObject group = new GameObject("Group " + index);
            group.transform.SetParent(_background.transform);
            group.transform.localPosition = new Vector3(index * 2, 0, 0);
            group.AddComponent<SpriteRenderer>().sprite = _sprite;
            Component controller = group.AddComponent(controllerType);
            controllerType.GetField("_cam").SetValue(controller, _cameraObject);
            controllerType.GetField("ParallaxEffect").SetValue(controller, 0.5f);
        }

        yield return null;
        _cameraObject.transform.position = new Vector3(100, 0, -10);
        yield return new WaitForFixedUpdate();
        yield return null;
        AssertCoverage();
        Assert.That(_background.GetComponentsInChildren(controllerType).Length, Is.EqualTo(3));
        int countAtCenter = 0;
        foreach (SpriteRenderer renderer in _background.GetComponentsInChildren<SpriteRenderer>())
            if (renderer.bounds.min.x < 100 && renderer.bounds.max.x > 100) countAtCenter++;
        Assert.That(countAtCenter, Is.EqualTo(1), "Repeated groups should not overlap.");
    }

    private void AssertCoverage()
    {
        float halfWidth = _camera.orthographicSize * _camera.aspect;
        float coveredUntil = _camera.transform.position.x - halfWidth;
        float right = _camera.transform.position.x + halfWidth;
        List<Bounds> bounds = new List<Bounds>();
        foreach (SpriteRenderer renderer in _background.GetComponentsInChildren<SpriteRenderer>()) bounds.Add(renderer.bounds);
        bounds.Sort((first, second) => first.min.x.CompareTo(second.min.x));
        foreach (Bounds item in bounds)
        {
            if (item.max.x < coveredUntil) continue;
            Assert.That(item.min.x, Is.LessThanOrEqualTo(coveredUntil + 0.001f), "Background gap inside the camera view.");
            coveredUntil = Mathf.Max(coveredUntil, item.max.x);
            if (coveredUntil >= right) return;
        }
        Assert.Fail("Background does not reach the camera edge.");
    }
}
