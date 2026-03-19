using System.Collections.Generic;
using Assets.ScriptableObjects;
using Assets.ScriptableObjects.BuildingConfig;
using Assets.Scripts.Buildings;
using Assets.Scripts.Enum;
using UnityEngine;
using UnityEngine.Tilemaps;
using Zenject;

namespace Assets.Scripts.World
{
    public struct TileData
    {
        public bool Busy;
        public Vector3Int Position;

        public TileData(bool busy, Vector3Int pos)
        {
            Busy = busy;
            Position = pos;
        }
    }
    public class WorldGenerateController 
    {
        private Tilemap _groundTilemap;
        private WorldGridConfig _worldGridConfig;
        private List<TileData> _tileData = new List<TileData>();
        private BuildingManager _buildingManager;
        
        [Inject]
        public WorldGenerateController([Inject(Id = "Ground")]Tilemap tilemap, WorldGridConfig worldGridConfig, BuildingManager buildingManager)
        {
            _groundTilemap = tilemap;
            _worldGridConfig = worldGridConfig;
            _buildingManager = buildingManager;
            GenerateTerrain();
            GenerateStartBuildings();
        }

        private void GenerateTerrain()
        {
            _groundTilemap.ClearAllTiles();

            for (var x = -_worldGridConfig.Width/ 2; x < _worldGridConfig.Width / 2; x++)
            {
                for (var y = _worldGridConfig.GroundY; y > _worldGridConfig.GroundY - _worldGridConfig.GroundHeight; y--)
                {
                    var pos = new Vector3Int(x, y, 0);
                    _groundTilemap.SetTile(pos, _worldGridConfig.GroundRuleTile);
                    if(y == _worldGridConfig.GroundY)_tileData.Add(new TileData(false, pos));
                }
            }   
        }

        private void GenerateStartBuildings()
        {
            /*for (var x = -_worldGridConfig.Width / 2; x < _worldGridConfig.Width / 2; x++)
            {
                if (IsAreaFree(x + _worldGridConfig.Width / 2, _buildingManager._configsDict[BuildingType.Camp].Area))
                {
                    _buildingManager.PlaceBuilding(BuildingType.Camp, _tileData[x + _worldGridConfig.Width / 2].Position);
                    OccupyAreaTile(x + _worldGridConfig.Width / 2, _buildingManager._configsDict[BuildingType.Camp].Area);
                }
            }*/

            var center = _worldGridConfig.Width / 2;
            if (IsAreaFree(center, _buildingManager._configsDict[BuildingType.Camp].Area))
            {
                _buildingManager.PlaceBuilding(BuildingType.Camp, _tileData[_worldGridConfig.Width / 2].Position);
                OccupyAreaTile(center, _buildingManager._configsDict[BuildingType.Camp].Area);
            }
        }

        private void OccupyAreaTile(int centerIndex, int size)
        {
            int halfSize = size / 2;

            for (int i = centerIndex - halfSize; i < centerIndex + halfSize; i++)
            {
                if (i < 0 || i >= _tileData.Count) continue;

                var data = _tileData[i];
                data.Busy = true;
                _tileData[i] = data;
            }
        }
        
        private bool IsAreaFree(int centerIndex, int size)
        {
            int halfSize = size / 2;

            for (int i = centerIndex - halfSize; i <= centerIndex + halfSize; i++)
            {
                if (i < 0 || i >= _tileData.Count) return false;

                if (_tileData[i].Busy) return false;
            }

            return true;
        }
    }
}
