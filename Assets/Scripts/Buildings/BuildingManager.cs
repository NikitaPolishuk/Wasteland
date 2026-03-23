using System.Collections.Generic;
using System.Linq;
using Assets.ScriptableObjects.BuildingConfig;
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
        private readonly BuildingConfig[] _configs;
        public readonly Dictionary<BuildingType, BuildingConfig> ConfigsDict;
        
        [Inject]
        public BuildingManager(IBuildingFactory buildingFactory, [Inject(Id = "Ground")] Tilemap groundTilemap, BuildingConfig[] configs)
        {
            _buildingFactory = buildingFactory;
            _groundTilemap = groundTilemap;
            ConfigsDict = configs.ToDictionary(x => x.BuildingType, x => x);
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

        public GameObject PlaceBuilding(BuildingType type, Vector3 worldPosition)
        {
            if (!IsTileValid(worldPosition))
            {
                Debug.LogWarning("Cannot place building: no tile at position");
                return null;
            }

            Vector3 tilePosition = GetTilePosition(worldPosition);
            float topY = tilePosition.y + _groundTilemap.cellSize.y/2f;
            Vector3 topPosition = new Vector3(worldPosition.x, topY, 0);
            return _buildingFactory.Create(type, topPosition);
        }
    }
}
