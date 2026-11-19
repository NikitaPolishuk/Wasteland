using Assets.ScriptableObjects;
using Assets.Scripts.Enum;
using Assets.Scripts.Interfaces;
using System;
using UnityEngine;
using Zenject;

namespace Assets.Scripts.Character
{
    public class CharacterInputHandler : IDisposable, IFixedTickable
    {
        private IInput _inputReader;
        private IMovable _movable;
        private PlayerConfig _playerConfig;

        private Vector2 _inputDirection;

        [Inject]
        public CharacterInputHandler(IInput input, IMovable movable)
        {
            _inputReader = input;
            _movable = movable;

            _inputReader.OnMove += OnMoveHandler;
        }

        private void OnMoveHandler(Vector2 move)
        {
            _inputDirection = move;
        }

        public void FixedTick()
        {
            _movable.MoveController.Move(_inputDirection, _movable.Speed);
        }

        public void Dispose()
        {
            _inputReader.OnMove -= OnMoveHandler;
        }
    }
}
