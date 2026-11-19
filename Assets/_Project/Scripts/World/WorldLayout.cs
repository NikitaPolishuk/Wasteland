using System.Collections.Generic;
using Assets.ScriptableObjects;
using Assets.ScriptableObjects.BuildingConfig;
using Assets.Scripts.Enum;

namespace Assets.Scripts.World
{
    public class WorldBuildingPlacement
    {
        public BuildingConfig Config;
        public int Start;
        public int End;
    }

    public class WorldLayout
    {
        public readonly List<string> Errors = new List<string>();
        public readonly List<WorldBuildingPlacement> Buildings = new List<WorldBuildingPlacement>();
        public int LeftCell;
        public bool IsValid => Errors.Count == 0;

        public static WorldLayout Calculate(WorldGridConfig config, bool requireTile = true)
        {
            WorldLayout result = new WorldLayout();
            if (config == null)
            {
                result.Errors.Add("World configuration is missing.");
                return result;
            }
            if (config.Width <= 0) result.Errors.Add("World width must be positive.");
            if (config.GroundHeight <= 0) result.Errors.Add("Ground thickness must be positive.");
            if ((long)config.GroundY - config.GroundHeight + 1 < int.MinValue) result.Errors.Add("Ground height exceeds cell coordinates.");
            if (requireTile && config.GroundRuleTile == null) result.Errors.Add("Ground Rule Tile is missing.");
            result.LeftCell = -(config.Width / 2);
            long center = (long)config.Width / 2 + config.CampOffset;
            if (result.ValidateBuilding(config.Camp, "Camp", true))
            {
                long start = center - config.Camp.Area / 2;
                result.AddBuilding(config.Camp, start, config.Width, "Camp");
                result.AddSide(config.LeftBuildings, start, config.Width, true);
                result.AddSide(config.RightBuildings, start + config.Camp.Area, config.Width, false);
            }
            if (config.PlayerCell < 0 || config.PlayerCell >= config.Width) result.Errors.Add("Player cell is outside the world.");
            foreach (WorldBuildingPlacement building in result.Buildings)
                if (config.PlayerCell >= building.Start && config.PlayerCell < building.End) result.Errors.Add("Player cell overlaps a building.");
            HashSet<BuildingType> types = new HashSet<BuildingType>();
            if (config.Requirements != null)
                foreach (WorldBuildingRequirement requirement in config.Requirements)
                {
                    if (requirement == null) { result.Errors.Add("Building requirement is missing."); continue; }
                    if (types.Add(requirement.Type) == false) result.Errors.Add($"Duplicate requirement: {requirement.Type}.");
                    if (requirement.Minimum < 0) result.Errors.Add($"Negative minimum: {requirement.Type}.");
                    int count = 0;
                    foreach (WorldBuildingPlacement building in result.Buildings)
                        if (building.Config.BuildingType == requirement.Type) count++;
                    if (count < requirement.Minimum) result.Errors.Add($"Not enough buildings of type {requirement.Type}: {count}/{requirement.Minimum}.");
                }
            EnvironmentLayout.Validate(config, result.Errors);
            return result;
        }

        private bool ValidateBuilding(BuildingConfig config, string label, bool camp)
        {
            if (config == null) { Errors.Add($"{label}: building configuration is missing."); return false; }
            bool valid = true;
            if ((config.BuildingType == BuildingType.Camp) != camp) { Errors.Add($"{label}: invalid building type."); valid = false; }
            if (config.Area <= 0) { Errors.Add($"{label}: width must be positive."); valid = false; }
            if (config.BuildingPrefab == null) { Errors.Add($"{label}: prefab is missing."); valid = false; }
            return valid;
        }

        private void AddSide(List<WorldBuildingEntry> entries, long edge, int width, bool left)
        {
            if (entries == null) return;
            for (int i = 0; i < entries.Count; i++)
            {
                WorldBuildingEntry entry = entries[i];
                string label = $"{(left ? "Left" : "Right")} building {i + 1}";
                if (entry == null) { Errors.Add($"{label}: entry is missing."); continue; }
                if (entry.Gap < 0) Errors.Add($"{label}: gap cannot be negative.");
                if (ValidateBuilding(entry.Config, label, false) == false || entry.Gap < 0) continue;
                long start = left ? edge - entry.Gap - entry.Config.Area : edge + entry.Gap;
                AddBuilding(entry.Config, start, width, label);
                edge = left ? start : start + entry.Config.Area;
            }
        }

        private void AddBuilding(BuildingConfig config, long start, int width, string label)
        {
            long end = start + config.Area;
            if (start < 0 || end > width) { Errors.Add($"{label}: building exceeds world bounds."); return; }
            Buildings.Add(new WorldBuildingPlacement { Config = config, Start = (int)start, End = (int)end });
        }
    }
}
