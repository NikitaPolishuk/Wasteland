using System;
using System.Collections.Generic;
using Assets.ScriptableObjects;

namespace Assets.Scripts.World
{
    public class EnvironmentSegment
    {
        public int Start;
        public int End;
        public bool IsForest;
    }

    public class EnvironmentPlacement
    {
        public EnvironmentConfig Config;
        public int Start;
        public int End;
        public float Scale;
    }

    public class EnvironmentLayout
    {
        public readonly List<EnvironmentSegment> Segments = new List<EnvironmentSegment>();
        public readonly List<EnvironmentPlacement> Placements = new List<EnvironmentPlacement>();
        private const int BUILDING_PROTECTION = 1;

        private class PlacementContext
        {
            public bool[] Occupied;
            public bool[] Other;
            public bool[] ProtectedCells;
            public bool[] Large;
            public bool[] TreeCells;
            public float MinScale = 1f;
            public float MaxScale = 1f;
        }

        public static void Validate(WorldGridConfig config, List<string> errors)
        {
            if (config.ForestMinLength <= 0 || config.ForestMaxLength < config.ForestMinLength) errors.Add("Forest length range is invalid.");
            if (config.OpenMinLength <= 0 || config.OpenMaxLength < config.OpenMinLength) errors.Add("Open length range is invalid.");
            ValidateScaleRange("Tree", config.TreeScaleMin, config.TreeScaleMax, errors);
            ValidateScaleRange("Rock", config.RockScaleMin, config.RockScaleMax, errors);
            ValidateScaleRange("Grass", config.GrassScaleMin, config.GrassScaleMax, errors);
            ValidateVariants("Trees", config.Trees, config.TreeDensity, errors);
            ValidateVariants("Rocks", config.Rocks, Math.Max(config.ForestRockDensity, config.OpenRockDensity), errors);
            ValidateVariants("Grass", config.Grass, Math.Max(config.ForestGrassDensity, config.OpenGrassDensity), errors);
            ValidateDensity("Forest rocks", config.ForestRockDensity, errors);
            ValidateDensity("Open rocks", config.OpenRockDensity, errors);
            ValidateDensity("Forest grass", config.ForestGrassDensity, errors);
            ValidateDensity("Open grass", config.OpenGrassDensity, errors);
        }

        public static EnvironmentLayout Generate(WorldGridConfig config, WorldLayout world, Random random)
        {
            if (config == null || world == null || world.IsValid == false || random == null) throw new ArgumentException("A valid world and random source are required.");
            EnvironmentLayout result = new EnvironmentLayout();
            int left = config.Width;
            int right = 0;
            foreach (WorldBuildingPlacement building in world.Buildings)
            {
                left = Math.Min(left, building.Start);
                right = Math.Max(right, building.End);
            }
            result.AddSegments(0, left, true, config, random);
            result.AddSegments(right, config.Width, false, config, random);
            bool[] large = new bool[config.Width];
            bool[] grass = new bool[config.Width];
            bool[] protectedCells = new bool[config.Width];
            foreach (WorldBuildingPlacement building in world.Buildings)
                for (int i = Math.Max(0, building.Start - BUILDING_PROTECTION); i < Math.Min(config.Width, building.End + BUILDING_PROTECTION); i++) protectedCells[i] = true;
            foreach (EnvironmentSegment segment in result.Segments)
                if (segment.IsForest) result.Place(segment, config.Trees, config.TreeDensity, new PlacementContext { Occupied = large, ProtectedCells = protectedCells, MinScale = config.TreeScaleMin, MaxScale = config.TreeScaleMax }, random);
            bool[] treeCells = (bool[])large.Clone();
            List<EnvironmentSegment> regions = new List<EnvironmentSegment>(result.Segments);
            // Gaps between buildings use open-ground densities; the protected cells remain unavailable.
            List<WorldBuildingPlacement> buildings = new List<WorldBuildingPlacement>(world.Buildings);
            buildings.Sort((a, b) => a.Start.CompareTo(b.Start));
            for (int i = 1; i < buildings.Count; i++)
                if (buildings[i - 1].End < buildings[i].Start) regions.Add(new EnvironmentSegment { Start = buildings[i - 1].End, End = buildings[i].Start });
            foreach (EnvironmentSegment region in regions)
                result.Place(region, config.Rocks, region.IsForest ? config.ForestRockDensity : config.OpenRockDensity, new PlacementContext { Occupied = large, Other = grass, ProtectedCells = protectedCells, MinScale = config.RockScaleMin, MaxScale = config.RockScaleMax }, random);
            foreach (EnvironmentSegment region in regions)
                result.Place(region, config.Grass, region.IsForest ? config.ForestGrassDensity : config.OpenGrassDensity, new PlacementContext { Occupied = grass, ProtectedCells = protectedCells, Large = large, TreeCells = treeCells, MinScale = config.GrassScaleMin, MaxScale = config.GrassScaleMax }, random);
            return result;
        }

        private static void ValidateScaleRange(string label, float minimum, float maximum, List<string> errors)
        {
            if (float.IsNaN(minimum) || float.IsInfinity(minimum) || float.IsNaN(maximum) || float.IsInfinity(maximum) || minimum <= 0 || maximum < minimum)
                errors.Add($"{label} scale range is invalid.");
        }

        private static void ValidateDensity(string label, float value, List<string> errors)
        {
            if (float.IsNaN(value) || float.IsInfinity(value) || value < 0 || value > 1) errors.Add($"{label}: density must be between 0 and 1.");
        }

        private static void ValidateVariants(string label, List<EnvironmentVariant> variants, float density, List<string> errors)
        {
            ValidateDensity(label, density, errors);
            double sum = 0;
            if (variants != null)
                foreach (EnvironmentVariant variant in variants)
                {
                    if (variant == null || variant.Config == null) { errors.Add($"{label}: variant configuration is missing."); continue; }
                    if (variant.Config.Area <= 0 || variant.Config.EnvironmentPrefab == null) errors.Add($"{label}: variant requires positive width and a prefab.");
                    if (float.IsNaN(variant.Weight) || float.IsInfinity(variant.Weight) || variant.Weight < 0) errors.Add($"{label}: weight must be finite and nonnegative.");
                    else sum += variant.Weight;
                }
            if (density > 0 && sum <= 0) errors.Add($"{label}: positive density requires variants with positive weight.");
        }

        private void AddSegments(int start, int end, bool reverse, WorldGridConfig config, Random random)
        {
            bool forest = true;
            int edge = reverse ? end : start;
            while (reverse ? edge > start : edge < end)
            {
                int min = forest ? config.ForestMinLength : config.OpenMinLength;
                int max = forest ? config.ForestMaxLength : config.OpenMaxLength;
                long length = min + (long)(random.NextDouble() * ((long)max - min + 1));
                int next = reverse ? (int)Math.Max(start, edge - length) : (int)Math.Min(end, edge + length);
                Segments.Add(new EnvironmentSegment { Start = reverse ? next : edge, End = reverse ? edge : next, IsForest = forest });
                edge = next;
                forest = forest == false;
            }
        }

        private void Place(EnvironmentSegment region, List<EnvironmentVariant> variants, float density, PlacementContext context, Random random)
        {
            if (density <= 0 || variants == null) return;
            int target = (int)Math.Floor((region.End - region.Start) * (double)density);
            for (int count = 0; count < target; count++)
            {
                List<EnvironmentVariant> eligible = new List<EnvironmentVariant>();
                List<List<int>> starts = new List<List<int>>();
                double total = 0;
                foreach (EnvironmentVariant variant in variants)
                {
                    if (variant.Weight <= 0) continue;
                    List<int> positions = new List<int>();
                    for (int start = region.Start; start <= region.End - variant.Config.Area; start++)
                    {
                        bool free = true;
                        for (int i = start; i < start + variant.Config.Area; i++)
                        {
                            if (context.Occupied[i] || context.ProtectedCells[i] || (context.Other != null && context.Other[i])) { free = false; break; }
                            if (context.Large != null && context.Large[i] && context.TreeCells[i] == false) { free = false; break; }
                        }
                        if (free) positions.Add(start);
                    }
                    if (positions.Count > 0) { eligible.Add(variant); starts.Add(positions); total += variant.Weight; }
                }
                if (eligible.Count == 0) break;
                double choice = random.NextDouble() * total;
                int selected = eligible.Count - 1;
                for (int i = 0; i < eligible.Count; i++) { choice -= eligible[i].Weight; if (choice < 0) { selected = i; break; } }
                EnvironmentVariant picked = eligible[selected];
                int position = starts[selected][random.Next(starts[selected].Count)];
                for (int i = position; i < position + picked.Config.Area; i++) context.Occupied[i] = true;
                Placements.Add(new EnvironmentPlacement { Config = picked.Config, Start = position, End = position + picked.Config.Area, Scale = context.MinScale + (float)random.NextDouble() * (context.MaxScale - context.MinScale) });
            }
        }

    }
}
