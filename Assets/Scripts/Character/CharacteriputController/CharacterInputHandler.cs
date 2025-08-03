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
        private IMoveController _movable;
        private PlayerConfig _playerConfig;

        private Vector2 _inputDirection;

        [Inject]
        public CharacterInputHandler(IInput input, IMoveController movable, PlayerConfig playerConfig)
        {
            _inputReader = input;
            _movable = movable;
            _playerConfig = playerConfig;

            _inputReader.OnMove += OnMoveHandler;
        }

        private void OnMoveHandler(Vector2 move)
        {
            _inputDirection = move;
        }

        public void FixedTick()
        {
            _movable.Move(_inputDirection, _playerConfig.Speed);
        }

        public void Dispose()
        {
            _inputReader.OnMove -= OnMoveHandler;
        }
    }
}
