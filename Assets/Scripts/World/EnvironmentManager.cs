using Assets.ScriptableObjects;
using Assets.Scripts.Enum;
using Assets.Scripts.Factories;
using UnityEngine;
using UnityEngine.Tilemaps;
using Zenject;

namespace Assets.Scripts.World
{
    public class EnvironmentManager : PlaceManager<EnvironmentConfig, EnvironmentType>
    {
        private readonly IEnvironmentFactory _environmentFactory;

        [Inject]
        public EnvironmentManager(IEnvironmentFactory environmentFactory, [Inject(Id = "Ground")] Tilemap groundTilemap, EnvironmentConfig[] configs) 
            : base(groundTilemap, configs)
        {
            _environmentFactory = environmentFactory;
        }

        protected override EnvironmentType GetTypeKey(EnvironmentConfig config) => config.EnvironmentType;

        public override GameObject Place(EnvironmentType type, Vector3 worldPosition)
        {
            return PlaceInternal(type, worldPosition, null);
        }

        public GameObject Place(EnvironmentType type, Vector3 worldPosition, Vector3 scale)
        {
            return PlaceInternal(type, worldPosition, scale);
        }

        private GameObject PlaceInternal(EnvironmentType type, Vector3 worldPosition, Vector3? scale)
        {
            if (!IsTileValid(worldPosition))
            {
                Debug.LogWarning("Cannot place object: no tile at position");
                return null;
            }

            Vector3 topPosition = GetTopPosition(worldPosition);
            return scale.HasValue
                ? _environmentFactory.Create(type, topPosition, scale.Value)
                : _environmentFactory.Create(type, topPosition);
        }

        public override EnvironmentConfig GetConfig(EnvironmentType type) => ConfigsDict[type];
    }
}
