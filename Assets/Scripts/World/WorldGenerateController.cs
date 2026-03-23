using System.Collections.Generic;
using System.Linq;
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
        public int Index;
        public bool Busy;
        public bool Environment;
        public Vector3Int Position;

        public TileData(bool busy, Vector3Int pos, int index, bool environment)
        {
            Busy = busy;
            Environment = environment;
            Position = pos;
            Index = index;
        }
    }

    public class WorldGenerateController
    {
        private Tilemap _groundTilemap;
        private WorldGridConfig _worldGridConfig;
        private List<TileData> _tileData = new List<TileData>();
        private BuildingManager _buildingManager;
        private EnvironmentManager _environmentManager;

        [Inject]
        public WorldGenerateController([Inject(Id = "Ground")] Tilemap tilemap, WorldGridConfig worldGridConfig, BuildingManager buildingManager,
            EnvironmentManager environmentManager)
        {
            _groundTilemap = tilemap;
            _worldGridConfig = worldGridConfig;
            _buildingManager = buildingManager;
            _environmentManager = environmentManager;
            GenerateTerrain();
            GenerateStartBuildings();

            while (!_tileData.All(tile => tile.Busy))
            {
                ForestGeneration(true);
                LeaGeneration(true);
                ForestGeneration(false);
                LeaGeneration(false);
            }

            GrassGeneration();
            RockGeneration();
        }

        private void GenerateTerrain()
        {
            _groundTilemap.ClearAllTiles();

            for (var x = -_worldGridConfig.Width / 2; x < _worldGridConfig.Width / 2; x++)
            {
                for (var y = _worldGridConfig.GroundY; y > _worldGridConfig.GroundY - _worldGridConfig.GroundHeight; y--)
                {
                    var pos = new Vector3Int(x, y, 0);
                    _groundTilemap.SetTile(pos, _worldGridConfig.GroundRuleTile);
                    if (y == _worldGridConfig.GroundY) _tileData.Add(new TileData(false, pos, _tileData.Count, false));
                }
            }
        }

        private void GenerateStartBuildings()
        {
            var center = _worldGridConfig.Width / 2;
            if (IsAreaFree(center, _buildingManager.ConfigsDict[BuildingType.Camp].Area))
            {
                _buildingManager.PlaceBuilding(BuildingType.Camp, _tileData[center].Position);
                OccupyAreaTile(center, _buildingManager.ConfigsDict[BuildingType.Camp].Area);
            }

            PlaceOnFreeTile(BuildingType.Wall, true);
            PlaceOnFreeTile(BuildingType.Wall, false);
            PlaceOnFreeTile(BuildingType.Wall, true);
            PlaceOnFreeTile(BuildingType.Wall, false);
        }
        
        private GameObject PlaceOnFreeTile<T>(T objectType, bool side)
        {
            if(!FindFreeTile(side,  out var freeTile)) return null;
            
            if(objectType is BuildingType buildingType)
            {
                OccupyAreaTile(freeTile.Index, _buildingManager.ConfigsDict[buildingType].Area);
                return _buildingManager.PlaceBuilding(buildingType, freeTile.Position);
            }
            
            if(objectType is EnvironmentType environmentType)
            {
                OccupyAreaTile(freeTile.Index, _environmentManager.ConfigsDict[environmentType].Area);
                return _environmentManager.PlaceEnvironment(environmentType, freeTile.Position);
            }

            return null;
        }

        private void ForestGeneration(bool side)
        {
            for (int  i = 0; i < Random.Range(4, 15); i++)
            {
                var treeType = Random.Range(1, 3) == 1 ? EnvironmentType.Tree2 : EnvironmentType.Tree1;
                var tree = PlaceOnFreeTile(treeType, side);
                if (tree == null) break;
                tree.GetComponent<TreeController>().BackTree(i % 2 == 0);
            }
        }
        
        private void LeaGeneration(bool side)
        {
            for (int  i = 0; i < Random.Range(10, 20); i++)
            {
                if(!FindFreeTile(side,  out var freeTile)) break;
                OccupyAreaTile(freeTile.Index, 1, false);
            }
        }
        
        private void GrassGeneration()
        {
            for (int  i = 0; i < _tileData.Count; i++)
            { 
                var treeType = Random.Range(1, 3) == 1 ? EnvironmentType.Grass1 : EnvironmentType.Grass2;
                if (!_tileData[i].Environment && Random.Range(1, 3) > 1)
                {
                    _environmentManager.PlaceEnvironment(treeType, _tileData[i].Position);
                    OccupyAreaTile(_tileData[i].Index, 1, true);
                }
            }
        }
        
        private void RockGeneration()
        {
            for (int  i = 0; i < _tileData.Count; i++)
            { 
                var treeType = Random.Range(1, 3) == 1 ? EnvironmentType.Rock2 : EnvironmentType.Rock1;
                if (!_tileData[i].Environment)
                {
                    if (i + 1 < _tileData.Count && i - 1 > 0 && !_tileData[i + 1].Environment && !_tileData[i - 1].Environment)
                    {
                        _environmentManager.PlaceEnvironment(treeType, _tileData[i].Position);
                        OccupyAreaTile(_tileData[i].Index, 1, true);
                    }
                }
            }
        }

        private bool FindFreeTile(bool left, out TileData tileData)
        {
            tileData = new TileData();
            var center = _worldGridConfig.Width / 2;
            var start = center;
            var end = left ? 1 : _tileData.Count;
            var step = left ? -1 : 1;

            for (int i = start; left ? i >= end : i <= end; i += step)
            {
                if (!_tileData[i-1].Busy)
                {
                    tileData = _tileData[i-1];
                    return true;
                }
            }

            return false;
        }

        private void OccupyAreaTile(int centerIndex, int size, bool environment = true)
        {
            if (size == 1)
            {
                var data = _tileData[centerIndex ];
                data.Busy = true;
                if (environment) data.Environment = true;
                _tileData[centerIndex] = data;
                return;
            }
            
            int halfSize = size / 2;

            for (int i = centerIndex - halfSize; i < centerIndex + halfSize; i++)
            {
                if (i < 0 || i >= _tileData.Count) continue;

                var data = _tileData[i];
                data.Busy = true;
                if (environment) data.Environment = true;
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