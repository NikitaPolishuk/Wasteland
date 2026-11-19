using System.Collections.Generic;
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
            HashSet<EnvironmentPlacement> backgroundTrees = SelectBackgroundTrees(config, environmentLayout);
            foreach (EnvironmentPlacement placement in environmentLayout.Placements)
            {
                float center = (placement.Start + placement.End) / 2f;
                Vector3 position = tilemap.CellToWorld(new Vector3Int(layout.LeftCell, config.GroundY, 0));
                position += tilemap.transform.TransformVector(new Vector3(center * tilemap.cellSize.x, tilemap.cellSize.y / 2f, 0));
                GameObject instance = environment.Place(placement.Config, position, placement.Scale);
                if (instance != null && backgroundTrees.Contains(placement))
                {
                    TreeController tree = instance.GetComponent<TreeController>();
                    if (tree != null) tree.BackTree(true);
                }
            }
        }

        private static HashSet<EnvironmentPlacement> SelectBackgroundTrees(WorldGridConfig config, EnvironmentLayout layout)
        {
            HashSet<EnvironmentConfig> treeConfigs = new HashSet<EnvironmentConfig>();
            if (config.Trees != null)
                foreach (EnvironmentVariant variant in config.Trees) treeConfigs.Add(variant.Config);

            HashSet<EnvironmentPlacement> backgroundTrees = new HashSet<EnvironmentPlacement>();
            foreach (EnvironmentSegment segment in layout.Segments)
            {
                if (segment.IsForest == false) continue;

                List<EnvironmentPlacement> trees = new List<EnvironmentPlacement>();
                foreach (EnvironmentPlacement placement in layout.Placements)
                {
                    if (treeConfigs.Contains(placement.Config) && placement.Start >= segment.Start && placement.End <= segment.End)
                        trees.Add(placement);
                }

                trees.Sort((first, second) => first.Start.CompareTo(second.Start));
                for (int index = 0; index < trees.Count; index += 2) backgroundTrees.Add(trees[index]);
            }

            return backgroundTrees;
        }
    }
}
