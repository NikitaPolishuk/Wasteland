using UnityEngine;

namespace Systems
{
    public class BackgroundController : MonoBehaviour
    {
        private float _startPos, _length;
        public GameObject _cam;

        public float ParallaxEffect;
        
        void Start()
        {
            _startPos = transform.position.x;
            SpriteRenderer[] renderers = GetComponentsInChildren<SpriteRenderer>();
            if (renderers.Length == 0)
            {
                Debug.LogError($"Background {name} has no SpriteRenderer.", this);
                enabled = false;
                return;
            }
            Bounds bounds = renderers[0].bounds;
            foreach (SpriteRenderer spriteRenderer in renderers)
                bounds.Encapsulate(spriteRenderer.bounds);
            _length = bounds.size.x;
        }
        
        void FixedUpdate()
        {
            float distance = _cam.transform.position.x * ParallaxEffect;
            var movement = _cam.transform.position.x * (1 - ParallaxEffect);
            transform.position = new Vector3(_startPos + distance, transform.position.y, transform.position.z);

            if (movement > _startPos + _length)
            {
                _startPos += _length;
            }
            else if (movement < _startPos - _length)
            {
                _startPos -= _length;
            }
        }
    }
}
