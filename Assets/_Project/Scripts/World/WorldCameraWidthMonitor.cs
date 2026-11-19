using UnityEngine;
using UnityEngine.Tilemaps;
using Zenject;

namespace Assets.Scripts.World
{
    public class WorldCameraWidthMonitor : ITickable
    {
        private readonly Camera _camera;
        private readonly Tilemap _tilemap;
        private readonly WorldGridConfigBounds _bounds;
        private bool _isTooWide;

        public WorldCameraWidthMonitor(Camera camera, Tilemap tilemap, WorldGridConfigBounds bounds)
        {
            _camera = camera;
            _tilemap = tilemap;
            _bounds = bounds;
        }

        public static bool IsTooWide(Camera camera, Tilemap tilemap, int width)
        {
            if (camera == null || camera.orthographic == false) return true;
            float worldWidth = Mathf.Abs(tilemap.transform.TransformVector(new Vector3(tilemap.cellSize.x * width, 0, 0)).x);
            return camera.orthographicSize * 2f * camera.aspect > worldWidth;
        }

        public void Tick()
        {
            bool tooWide = IsTooWide(_camera, _tilemap, _bounds.Width);
            if (tooWide && _isTooWide == false) Debug.LogWarning("World is narrower than the visible camera area after resize.");
            _isTooWide = tooWide;
        }
    }

    public class WorldGridConfigBounds
    {
        public int Width;
    }
}
