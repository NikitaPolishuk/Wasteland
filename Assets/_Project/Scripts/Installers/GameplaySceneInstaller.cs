using Assets.ScriptableObjects;
using Assets.ScriptableObjects.BuildingConfig;
using Assets.Scripts.Buildings;
using Assets.Scripts.Character;
using Assets.Scripts.Enemy;
using Assets.Scripts.Factories;
using Assets.Scripts.Interfaces;
using Assets.Scripts.MoveControllers;
using Assets.Scripts.Wallet;
using Assets.Scripts.World;
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
        
        [Header("World")]
        [SerializeField] private WorldGridConfig _worldGridConfig;
        [SerializeField] private EnvironmentConfig[] _environmentConfigs;

        private WorldLayout _layout;
        private Vector3 _spawnPosition;

        public override void InstallBindings()
        {
            ValidateWorld();
            Application.targetFrameRate = 60;
            Container.Bind<WorldLayout>().FromInstance(_layout);
            Container.Bind<WorldGridConfigBounds>().FromInstance(new WorldGridConfigBounds { Width = _worldGridConfig.Width });
            Container.BindInterfacesTo<WorldCameraWidthMonitor>().AsSingle().WithArguments(Camera.main, _groundTilemap).NonLazy();
            BindWallet();
            BindPlayer();
            BindEnemySystem();
            WorldGenerate();
            BindBuildingSystem();
            Container.BindInterfacesAndSelfTo<InteractableService>().AsSingle().NonLazy();
        }

        private void ValidateWorld()
        {
            _layout = WorldLayout.Calculate(_worldGridConfig);
            if (_groundTilemap == null) _layout.Errors.Add("Ground Tilemap is missing.");
            if (_characterController == null) _layout.Errors.Add("Player prefab is missing.");
            Camera camera = Camera.main;
            if (camera == null) _layout.Errors.Add("Main camera is missing.");
            if (_groundTilemap != null && camera != null && _worldGridConfig != null && WorldCameraWidthMonitor.IsTooWide(camera, _groundTilemap, _worldGridConfig.Width))
                _layout.Errors.Add("World is narrower than the visible camera area, or camera is not orthographic.");
            if (_groundTilemap != null && _characterController != null && _worldGridConfig != null && _worldGridConfig.Width > 0)
            {
                BoxCollider2D collider = _characterController.GetComponent<BoxCollider2D>();
                if (collider == null) _layout.Errors.Add("Player BoxCollider2D is missing.");
                else
                {
                    int cell = _layout.LeftCell + _worldGridConfig.PlayerCell;
                    Vector3 bottom = _groundTilemap.CellToWorld(new Vector3Int(cell, _worldGridConfig.GroundY, 0));
                    bottom += _groundTilemap.transform.TransformVector(new Vector3(_groundTilemap.cellSize.x / 2f, _groundTilemap.cellSize.y, 0));
                    Vector3 scale = _characterController.transform.localScale;
                    _spawnPosition = bottom - new Vector3(collider.offset.x * scale.x, (collider.offset.y - collider.size.y / 2f) * scale.y, 0);
                    float left = _groundTilemap.CellToWorld(new Vector3Int(_layout.LeftCell, 0, 0)).x;
                    float right = _groundTilemap.CellToWorld(new Vector3Int(_layout.LeftCell + _worldGridConfig.Width, 0, 0)).x;
                    float half = Mathf.Abs(collider.size.x * scale.x) / 2f;
                    if (bottom.x - half < left || bottom.x + half > right) _layout.Errors.Add("Player collider exceeds world bounds.");
                    _layout.ValidatePlayerCollider(_groundTilemap, _worldGridConfig.PlayerCell, half * 2f);
                }
            }
            if (_layout.IsValid == false) throw new System.InvalidOperationException("World generation failed:\n" + string.Join("\n", _layout.Errors));
        }
        private void BindPlayer()
        {
            Container.Bind<PlayerConfig>().FromInstance(_playerConfig);
            Player character = Container.InstantiatePrefabForComponent<Player>(_characterController, _spawnPosition, Quaternion.identity, null);
            int halfWidth = _worldGridConfig.Width / 2;
            float leftBound = _groundTilemap.CellToWorld(new Vector3Int(-halfWidth, 0, 0)).x;
            float rightBound = _groundTilemap.CellToWorld(new Vector3Int(_layout.LeftCell + _worldGridConfig.Width, 0, 0)).x;
            character.GetComponent<RigedbodyMoveController>().ConfigureBounds(leftBound, rightBound);
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
            Container.Bind<BuildingManager>().AsSingle().WithArguments(_buildingConfigs).NonLazy();
        }
        
        private void WorldGenerate()
        {
            Container.Bind<IEnvironmentFactory>().To<EnvironmentFactory>().AsSingle().WithArguments(_environmentConfigs);
            Container.Bind<EnvironmentManager>().AsSingle().WithArguments(_environmentConfigs).NonLazy();
            Container.Bind<WorldGridConfig>().FromInstance(_worldGridConfig);
            Container.Bind<WorldGenerateController>().AsSingle().NonLazy();
        }
    }
}
