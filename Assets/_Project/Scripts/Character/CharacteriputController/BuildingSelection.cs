namespace Assets.Scripts.Character
{
    public static class BuildingSelection
    {
        public static int Select(float[] distances, bool[] available, int current)
        {
            int selected = -1;
            float nearest = float.PositiveInfinity;

            for (int i = 0; i < distances.Length; i++)
            {
                if (available[i] == false) continue;
                if (distances[i] < nearest || (distances[i] == nearest && i == current))
                {
                    selected = i;
                    nearest = distances[i];
                }
            }

            return selected;
        }
    }
}
