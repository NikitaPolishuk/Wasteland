using Assets.ScriptableObjects;
using Assets.ScriptableObjects.BuildingConfig;
using Assets.Scripts.Buildings;
using Assets.Scripts.Character;
using Assets.Scripts.Enemy;
using Assets.Scripts.Factories;
using Assets.Scripts.Interfaces;
using Assets.Scripts.Wallet;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Tilemaps;
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
        [SerializeField] private EnemyConfig[] _enemyConfigs;
        [SerializeField] private Transform _enemySpawnPoint;

        [Header("Buildings")]
        [SerializeField] private BuildingConfig[] _buildingConfigs;
        [SerializeField] private Tilemap _groundTilemap;

        public override void InstallBindings()
        {
            BindWallet();
            BindPlayer();
            BindEnemySystem();
            BindBuildingSystem();
            Container.Bind<IInteractableService>().To<InteractableService>().AsSingle().NonLazy();
        }

        private void BindPlayer()
        {
            Container.Bind<PlayerConfig>().FromInstance(_playerConfig);
            Player character = Container.InstantiatePrefabForComponent<Player>(_characterController, _playerSpawnpPoint.position, Quaternion.identity, null);
            Container.BindInterfacesAndSelfTo<Player>().FromInstance(character).AsSingle();
            Container.BindInterfacesAndSelfTo<CharacterInputHandler>().AsSingle().WithArguments(character).NonLazy();
            _cinemachineCamera.Follow = character.transform;
        }

        private void BindWallet()
        {
            Container.Bind<ResourceWallet>().AsSingle();
            Container.Bind<IResourceService>().To<PlayerResourceService>().AsSingle().NonLazy();
        }

        private void BindEnemySystem()
        {
            Container.Bind<IEnemyFactory>().To<EnemyFactory>().AsSingle().WithArguments(_enemyConfigs);
            Container.BindInterfacesAndSelfTo<EnemyController>().AsSingle().WithArguments(_enemySpawnPoint).NonLazy();
        }

        private void BindBuildingSystem()
        {
            Container.Bind<IBuildingFactory>().To<BuildingFactory>().AsSingle().WithArguments(_buildingConfigs);
            Container.Bind<Tilemap>().WithId("Ground").FromInstance(_groundTilemap);
            Container.Bind<BuildingManager>().AsSingle();
        }
    }
}
