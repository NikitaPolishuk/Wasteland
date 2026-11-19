using Assets.Scripts.Enum;
using Assets.ScriptableObjects.BuildingConfig;
using UnityEngine;

namespace Assets.Scripts.Factories
{
    public interface IBuildingFactory
    {
        GameObject Create(BuildingType type, Vector2 position);
        GameObject Create(BuildingConfig config, Vector2 position);
    }
}
