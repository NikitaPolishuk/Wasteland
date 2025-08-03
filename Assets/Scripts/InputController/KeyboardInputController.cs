using Assets.Input;
using Assets.Scripts.Interfaces;
using System;
using UnityEngine;
using UnityEngine.InputSystem;
using Zenject;

namespace Assets.Scripts.Input
{
    public class KeyboardInputController : IInput, IDisposable
    {
        private readonly KeyBoardInputActions _inputActions;
        private Vector2 _move;

        public Action<Vector2> OnMove { get; set; }

        [Inject]
        public KeyboardInputController()
        {
            _inputActions = new KeyBoardInputActions();
            _inputActions.Move.Enable();
            _inputActions.Move.Move.performed += OnMovePerformed;
            _inputActions.Move.Move.canceled += OnMoveCanceled;
        }

        private void OnMovePerformed(InputAction.CallbackContext context)
        {
            _move = context.ReadValue<Vector2>();
            OnMove?.Invoke(_move);
        }

        private void OnMoveCanceled(InputAction.CallbackContext context)
        {
            _move = Vector2.zero;
            OnMove?.Invoke(_move);
        }

        public void Dispose()
        {
            _inputActions.Move.Move.performed -= OnMovePerformed;
            _inputActions.Move.Move.canceled -= OnMoveCanceled;
        }
    }
}
