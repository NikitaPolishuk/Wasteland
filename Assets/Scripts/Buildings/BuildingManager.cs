using Assets.Scripts.Enum;
using Assets.Scripts.Factories;
using UnityEngine;
using UnityEngine.Tilemaps;
using Zenject;

namespace Assets.Scripts.Buildings
{
    public class BuildingManager
    {
        private readonly IBuildingFactory _buildingFactory;
        private readonly Tilemap _groundTilemap;

        [Inject]
        public BuildingManager(IBuildingFactory buildingFactory, [Inject(Id = "Ground")] Tilemap groundTilemap)
        {
            _buildingFactory = buildingFactory;
            _groundTilemap = groundTilemap;
            /*PlaceBuilding(BuildingType.Camp, Vector3.one);*/
        }

        public Vector3 GetTilePosition(Vector3 worldPosition)
        {
            Vector3Int cellPos = _groundTilemap.WorldToCell(worldPosition);
            return _groundTilemap.GetCellCenterWorld(cellPos);
        }

        public bool IsTileValid(Vector3 worldPosition)
        {
            Vector3Int cellPos = _groundTilemap.WorldToCell(worldPosition);
            return _groundTilemap.HasTile(cellPos);
        }

        public void PlaceBuilding(BuildingType type, Vector3 worldPosition)
        {
            if (!IsTileValid(worldPosition))
            {
                Debug.LogWarning("Cannot place building: no tile at position");
                return;
            }

            Vector3 tilePosition = GetTilePosition(worldPosition);
            _buildingFactory.Create(type, tilePosition);
        }
    }
}
