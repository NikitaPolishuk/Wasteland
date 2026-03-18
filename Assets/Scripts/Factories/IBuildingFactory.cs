using Assets.Scripts.Enum;
using UnityEngine;

namespace Assets.Scripts.Factories
{
    public interface IBuildingFactory
    {
        GameObject Create(BuildingType type, Vector2 position);
    }
}
