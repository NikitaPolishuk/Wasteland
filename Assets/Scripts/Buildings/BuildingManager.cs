using Assets.ScriptableObjects.BuildingConfig;
using Assets.Scripts.Enum;
using Assets.Scripts.Factories;
using Assets.Scripts.World;
using UnityEngine;
using UnityEngine.Tilemaps;
using Zenject;

namespace Assets.Scripts.Buildings
{
    public class BuildingManager : PlaceManager<BuildingConfig, BuildingType>
    {
        private readonly IBuildingFactory _buildingFactory;

        [Inject]
        public BuildingManager(IBuildingFactory buildingFactory, [Inject(Id = "Ground")] Tilemap groundTilemap, BuildingConfig[] configs)
            : base(groundTilemap, configs)
        {
            _buildingFactory = buildingFactory;
        }

        protected override BuildingType GetTypeKey(BuildingConfig config) => config.BuildingType;

        public override GameObject Place(BuildingType type, Vector3 worldPosition)
        {
            if (!IsTileValid(worldPosition))
            {
                Debug.LogWarning("Cannot place building: no tile at position");
                return null;
            }

            Vector3 topPosition = GetTopPosition(worldPosition);
            return _buildingFactory.Create(type, topPosition);
        }

        public override BuildingConfig GetConfig(BuildingType type) => ConfigsDict[type];
    }
}
