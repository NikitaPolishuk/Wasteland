using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Assets.Scripts.World
{
    public abstract class PlaceManager<TConfig, TType> where TConfig : ScriptableObject
    {
        protected readonly Tilemap _groundTilemap;
        public readonly Dictionary<TType, TConfig> ConfigsDict;

        protected PlaceManager(Tilemap groundTilemap, TConfig[] configs)
        {
            _groundTilemap = groundTilemap;
            ConfigsDict = configs.ToDictionary(x => GetTypeKey(x), x => x);
        }

        protected abstract TType GetTypeKey(TConfig config);
        
        public abstract GameObject Place(TType type, Vector3 worldPosition);
        public abstract TConfig GetConfig(TType type);

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

        protected Vector3 GetTopPosition(Vector3 worldPosition)
        {
            Vector3 tilePosition = GetTilePosition(worldPosition);
            float topY = tilePosition.y + _groundTilemap.cellSize.y / 2f;
            return new Vector3(worldPosition.x, topY, 0);
        }
    }
}
