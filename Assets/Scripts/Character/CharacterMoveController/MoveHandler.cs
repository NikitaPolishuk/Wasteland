using Assets.ScriptableObjects;
using Assets.Scripts.Enum;
using Assets.Scripts.Interfaces;
using System;
using UnityEngine;
using Zenject;

namespace Assets.Scripts.Character
{
    public class MoveHandler : IDisposable
    {
        private IInput _inputReader;
        private IMoveController _movable;
        private PlayerConfig _playerConfig;

        [Inject]
        public MoveHandler(IInput input, IMoveController movable, IAnimator animator,  PlayerConfig playerConfig)
        {
            _inputReader = input;
            _movable = movable;
            _playerConfig = playerConfig;

            _inputReader.OnMove += OnMoveHandler;
        }

        private void OnMoveHandler(Vector2 move)
        {
            _movable.Move(move, _playerConfig.Speed);
        }

        public void Dispose()
        {
            _inputReader.OnMove -= OnMoveHandler;
        }
    }
}
