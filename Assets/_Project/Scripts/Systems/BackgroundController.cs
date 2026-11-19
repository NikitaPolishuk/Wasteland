using System.Collections.Generic;
using UnityEngine;

namespace Systems
{
    [DefaultExecutionOrder(10000)]
    public class BackgroundController : MonoBehaviour
    {
        private const int COVERAGE_MARGIN = 1;

        public GameObject _cam;
        public float ParallaxEffect;

        private readonly List<SpriteRenderer> _sources = new List<SpriteRenderer>();
        private readonly List<List<SpriteRenderer>> _copies = new List<List<SpriteRenderer>>();
        private Camera _camera;
        private Vector3 _initialPosition;
        private float _initialCameraX;
        private float _leftEdge;
        private float _rightEdge;
        private float _period;
        private bool _initialized;

        private void Start()
        {
            if (_initialized) return;

            InitializeBounds();
            if (enabled == false) return;

            List<BackgroundController> siblings = new List<BackgroundController> { this };
            if (transform.parent != null)
            {
                foreach (BackgroundController sibling in transform.parent.GetComponentsInChildren<BackgroundController>(true))
                {
                    if (sibling == this || sibling.transform.parent != transform.parent || sibling._cam != _cam || sibling.ParallaxEffect != ParallaxEffect) continue;
                    sibling.InitializeBounds();
                    if (sibling.enabled) siblings.Add(sibling);
                }
            }

            float left = _leftEdge;
            float right = _rightEdge;
            foreach (BackgroundController sibling in siblings)
            {
                left = Mathf.Min(left, sibling._leftEdge);
                right = Mathf.Max(right, sibling._rightEdge);
            }

            foreach (BackgroundController sibling in siblings) sibling._period = right - left;
        }

        private void InitializeBounds()
        {
            if (_initialized) return;
            _initialized = true;
            _camera = _cam != null ? _cam.GetComponent<Camera>() : null;
            if (_camera == null || _camera.orthographic == false)
            {
                Debug.LogError($"Background '{name}' requires an orthographic camera.", this);
                enabled = false;
                return;
            }

            CollectSources(this, _sources);
            if (_sources.Count == 0)
            {
                Debug.LogError($"Background '{name}' has no SpriteRenderer with a sprite.", this);
                enabled = false;
                return;
            }

            Bounds ownBounds = GetBounds(_sources);
            _period = ownBounds.size.x;
            if (_period <= Mathf.Epsilon)
            {
                Debug.LogError($"Background '{name}' has no usable horizontal width.", this);
                enabled = false;
                return;
            }

            _initialPosition = transform.position;
            _initialCameraX = _camera.transform.position.x;
            _leftEdge = ownBounds.min.x;
            _rightEdge = ownBounds.max.x;
            foreach (SpriteRenderer source in _sources) _copies.Add(new List<SpriteRenderer>());
        }

        private void LateUpdate()
        {
            float cameraX = _camera.transform.position.x;
            float parallaxOffset = (cameraX - _initialCameraX) * ParallaxEffect;
            transform.position = new Vector3(_initialPosition.x + parallaxOffset, _initialPosition.y, _initialPosition.z);

            float cameraHalfWidth = _camera.orthographicSize * _camera.aspect;
            float left = cameraX - cameraHalfWidth;
            float right = cameraX + cameraHalfWidth;
            int first = Mathf.FloorToInt((left - (_rightEdge + parallaxOffset)) / _period) - COVERAGE_MARGIN;
            int last = Mathf.CeilToInt((right - (_leftEdge + parallaxOffset)) / _period) + COVERAGE_MARGIN;

            int used = 0;
            for (int index = first; index <= last; index++)
            {
                if (index == 0) continue;
                for (int sourceIndex = 0; sourceIndex < _sources.Count; sourceIndex++)
                {
                    SpriteRenderer source = _sources[sourceIndex];
                    List<SpriteRenderer> copies = _copies[sourceIndex];
                    if (used == copies.Count) copies.Add(CreateCopy(source));
                    SpriteRenderer copy = copies[used];
                    copy.transform.localPosition = source.transform.InverseTransformVector(new Vector3(index * _period, 0, 0));
                    copy.gameObject.SetActive(true);
                }
                used++;
            }

            foreach (List<SpriteRenderer> copies in _copies)
                for (int index = used; index < copies.Count; index++) copies[index].gameObject.SetActive(false);
        }

        private static void CollectSources(BackgroundController controller, List<SpriteRenderer> sources)
        {
            foreach (SpriteRenderer renderer in controller.GetComponentsInChildren<SpriteRenderer>())
            {
                if (renderer.sprite != null && renderer.GetComponentInParent<BackgroundController>() == controller) sources.Add(renderer);
            }
        }

        private static Bounds GetBounds(List<SpriteRenderer> sources)
        {
            Bounds bounds = sources[0].bounds;
            for (int index = 1; index < sources.Count; index++) bounds.Encapsulate(sources[index].bounds);
            return bounds;
        }

        private static SpriteRenderer CreateCopy(SpriteRenderer source)
        {
            GameObject copyObject = new GameObject("Background repeat");
            copyObject.layer = source.gameObject.layer;
            copyObject.transform.SetParent(source.transform, false);
            SpriteRenderer copy = copyObject.AddComponent<SpriteRenderer>();
            copy.sprite = source.sprite;
            copy.color = source.color;
            copy.flipX = source.flipX;
            copy.flipY = source.flipY;
            copy.drawMode = source.drawMode;
            copy.size = source.size;
            copy.tileMode = source.tileMode;
            copy.adaptiveModeThreshold = source.adaptiveModeThreshold;
            copy.sharedMaterial = source.sharedMaterial;
            copy.sortingLayerID = source.sortingLayerID;
            copy.sortingOrder = source.sortingOrder;
            copy.maskInteraction = source.maskInteraction;
            copy.spriteSortPoint = source.spriteSortPoint;
            copy.enabled = source.enabled;
            return copy;
        }
    }
}
