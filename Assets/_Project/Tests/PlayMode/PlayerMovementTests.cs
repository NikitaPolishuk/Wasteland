using System;
using System.Collections;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class PlayerMovementTests
{
    [UnityTest]
    public IEnumerator DiagonalInputKeepsFullHorizontalSpeed()
    {
        GameObject player = new GameObject("Player");
        player.AddComponent<SpriteRenderer>();
        Rigidbody2D body = player.AddComponent<Rigidbody2D>();
        body.gravityScale = 0;
        Type controllerType = Type.GetType("Assets.Scripts.MoveControllers.RigedbodyMoveController, Assembly-CSharp");
        Component controller = player.AddComponent(controllerType);
        controllerType.GetMethod("Move").Invoke(controller, new object[] { new Vector2(0.7071068f, 0.7071068f), 4f });
        yield return new WaitForFixedUpdate();
        yield return new WaitForFixedUpdate();
        Assert.That(body.linearVelocity.x, Is.EqualTo(4).Within(0.001f));
        Assert.That(body.linearVelocity.y, Is.EqualTo(0).Within(0.001f));
        UnityEngine.Object.Destroy(player);
    }
    [UnityTest]
    public IEnumerator ColliderStaysInsideWorldAtRightEdge()
    {
        GameObject player = new GameObject("Player");
        player.AddComponent<SpriteRenderer>();
        Rigidbody2D body = player.AddComponent<Rigidbody2D>();
        body.gravityScale = 0;
        BoxCollider2D collider = player.AddComponent<BoxCollider2D>();
        Type controllerType = Type.GetType("Assets.Scripts.MoveControllers.RigedbodyMoveController, Assembly-CSharp");
        Component controller = player.AddComponent(controllerType);
        MethodInfo configure = controllerType.GetMethod("ConfigureBounds");
        Assert.That(configure, Is.Not.Null);
        configure.Invoke(controller, new object[] { -1f, 1f });
        controllerType.GetMethod("Move").Invoke(controller, new object[] { Vector2.right, 4f });
        yield return new WaitForSeconds(1f);
        Assert.That(collider.bounds.max.x, Is.LessThanOrEqualTo(1.001f));
        Assert.That(body.linearVelocity.x, Is.EqualTo(0).Within(0.001f));
        UnityEngine.Object.Destroy(player);
    }    [UnityTest]
    public IEnumerator AnimationFollowsActualVelocityAndStopsAtRest()
    {
        GameObject player = new GameObject("Player");
        Rigidbody2D body = player.AddComponent<Rigidbody2D>();
        body.gravityScale = 0;
        player.AddComponent<Animator>();
        Type animationType = Type.GetType("Assets.Scripts.Character.PlayerAnimationController, Assembly-CSharp");
        Component animation = player.AddComponent(animationType);
        PropertyInfo state = animationType.GetProperty("State");
        body.linearVelocity = Vector2.right * 4;
        yield return new WaitForFixedUpdate();
        yield return new WaitForFixedUpdate();
        Assert.That(state.GetValue(animation).ToString(), Is.EqualTo("Walk"));
        body.linearVelocity = Vector2.zero;
        yield return new WaitForFixedUpdate();
        yield return new WaitForFixedUpdate();
        Assert.That(state.GetValue(animation).ToString(), Is.EqualTo("Idle"));
        UnityEngine.Object.Destroy(player);
    }    [UnityTest]
    public IEnumerator LeftBoundaryAndStoppedInputStopTheBody()
    {
        GameObject player = new GameObject("Player");
        player.AddComponent<SpriteRenderer>();
        Rigidbody2D body = player.AddComponent<Rigidbody2D>();
        body.gravityScale = 0;
        BoxCollider2D collider = player.AddComponent<BoxCollider2D>();
        Type controllerType = Type.GetType("Assets.Scripts.MoveControllers.RigedbodyMoveController, Assembly-CSharp");
        Component controller = player.AddComponent(controllerType);
        MethodInfo move = controllerType.GetMethod("Move");
        controllerType.GetMethod("ConfigureBounds").Invoke(controller, new object[] { -1f, 1f });
        move.Invoke(controller, new object[] { Vector2.left, 4f });
        yield return new WaitForSeconds(1f);
        Assert.That(collider.bounds.min.x, Is.GreaterThanOrEqualTo(-1.001f));
        move.Invoke(controller, new object[] { Vector2.up, 4f });
        yield return new WaitForFixedUpdate();
        yield return new WaitForFixedUpdate();
        Assert.That(body.linearVelocity.x, Is.EqualTo(0).Within(0.001f));
        UnityEngine.Object.Destroy(player);
    }}
