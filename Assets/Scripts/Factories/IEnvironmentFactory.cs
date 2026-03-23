using Assets.Scripts.Enum;
using UnityEngine;

namespace Assets.Scripts.Factories
{
    public interface IEnvironmentFactory 
    {
        GameObject Create(EnvironmentType type, Vector2 position);
    }
}
