using Assets.ScriptableObjects;
using Assets.Scripts.Buildings;
using Assets.Scripts.Enum;
using UnityEngine;
using UnityEngine.Tilemaps;
using Zenject;

namespace Assets.Scripts.World
{
    public class WorldGenerateController
    {
        [Inject]
        public WorldGenerateController([Inject(Id = "Ground")] Tilemap tilemap, WorldGridConfig config,
            WorldLayout layout, BuildingManager buildings, EnvironmentManager environment)
        {
            tilemap.ClearAllTiles();
            for (int index = 0; index < config.Width; index++)
                for (int depth = 0; depth < config.GroundHeight; depth++)
                    tilemap.SetTile(new Vector3Int(layout.LeftCell + index, config.GroundY - depth, 0), config.GroundRuleTile);

            foreach (WorldBuildingPlacement placement in layout.Buildings)
            {
                float center = (placement.Start + placement.End) / 2f;
                Vector3 position = tilemap.CellToWorld(new Vector3Int(layout.LeftCell, config.GroundY, 0));
                position += tilemap.transform.TransformVector(new Vector3(center * tilemap.cellSize.x, tilemap.cellSize.y, 0));
                buildings.Place(placement.Config, position);
            }

            EnvironmentLayout environmentLayout = EnvironmentLayout.Generate(config, layout, new System.Random());
            foreach (EnvironmentPlacement placement in environmentLayout.Placements)
            {
                float center = (placement.Start + placement.End) / 2f;
                Vector3 position = tilemap.CellToWorld(new Vector3Int(layout.LeftCell, config.GroundY, 0));
                position += tilemap.transform.TransformVector(new Vector3(center * tilemap.cellSize.x, tilemap.cellSize.y / 2f, 0));
                environment.Place(placement.Config, position, placement.Scale);
            }
        }
    }
}
