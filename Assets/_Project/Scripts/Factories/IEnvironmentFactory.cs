using Assets.Scripts.Enum;
using Assets.ScriptableObjects;
using UnityEngine;

namespace Assets.Scripts.Factories
{
    public interface IEnvironmentFactory 
    {
        GameObject Create(EnvironmentConfig config, Vector2 position, Vector3 scale);
        GameObject Create(EnvironmentType type, Vector2 position);
        GameObject Create(EnvironmentType type, Vector2 position, Vector3 scale);
    }
}
