using Assets.ScriptableObjects;
using Assets.Scripts.Character;
using Assets.Scripts.Constants;
using Assets.Scripts.Interfaces;
using Assets.Scripts.Wallet;
using Unity.Cinemachine;
using UnityEngine;
using Zenject;

namespace Assets.Installer
{
    public class GameplaySceneInstaller : MonoInstaller
    {
        [Header("Camera")]
        [SerializeField] private CinemachineCamera _cinemachineCamera;

        [Header("Player")]
        [SerializeField] private Player _characterController;
        [SerializeField] private Transform _playerSpawnpPoint;
        [SerializeField] private PlayerConfig _playerConfig;

        public override void InstallBindings()
        {
            BindWallet();
            BindPlayer();
        }

        private void BindPlayer()
        {
            Container.Bind<PlayerConfig>().FromInstance(_playerConfig);
            Player character = Container.InstantiatePrefabForComponent<Player>(_characterController, _playerSpawnpPoint.position, Quaternion.identity, null);
            Container.BindInterfacesAndSelfTo<Player>().FromInstance(character).AsSingle();
            Container.Bind<MoveHandler>().AsSingle().WithArguments(character.MoveController, character.Animator).NonLazy();
            _cinemachineCamera.Follow = character.transform;
        }

        private void BindWallet()
        {
            Container.Bind<ResourceWallet>().AsSingle();
            Container.Bind<PlayerResourceService>().AsSingle().NonLazy();
        }
    }
}
