using System.Collections.Generic;
using System.Linq;
using Assets.ScriptableObjects;
using Assets.Scripts.Enum;
using Assets.Scripts.Factories;
using UnityEngine;
using UnityEngine.Tilemaps;
using Zenject;

namespace Assets.Scripts.World
{
    public class EnvironmentManager
    {
        private readonly IEnvironmentFactory _environmentFactory;
        private readonly Tilemap _groundTilemap;
        private readonly EnvironmentConfig[] _configs;
        public readonly Dictionary<EnvironmentType, EnvironmentConfig > ConfigsDict;
        
        [Inject]
        public EnvironmentManager(IEnvironmentFactory environmentFactory, [Inject(Id = "Ground")] Tilemap groundTilemap, EnvironmentConfig[] configs)
        {
            _environmentFactory = environmentFactory;
            _groundTilemap = groundTilemap;
            ConfigsDict = configs.ToDictionary(x => x.EnvironmentType, x => x);
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

        public GameObject PlaceEnvironment(EnvironmentType type, Vector3 worldPosition)
        {
            if (!IsTileValid(worldPosition))
            {
                Debug.LogWarning("Cannot place building: no tile at position");
                return null;
            }

            Vector3 tilePosition = GetTilePosition(worldPosition);
            float topY = tilePosition.y + _groundTilemap.cellSize.y / 2f;
            Vector3 topPosition = new Vector3(worldPosition.x, topY, 0);
            return _environmentFactory.Create(type, topPosition);
        }
    }
}
