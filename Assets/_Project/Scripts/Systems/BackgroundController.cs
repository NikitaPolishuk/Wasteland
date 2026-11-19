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
            _length = GetComponent<SpriteRenderer>().bounds.size.x;
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
