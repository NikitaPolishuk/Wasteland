using Assets.ScriptableObjects;
using Assets.Scripts.Interfaces;
using UnityEngine;
using Zenject;

namespace Assets.Scripts.MoveControllers
{
    public class RigedbodyMoveController : MonoBehaviour, IMoveController
    {
        [SerializeField] private SpriteRenderer _spriteRenderer;
        private Rigidbody2D _rb;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _spriteRenderer = GetComponent<SpriteRenderer>();
        }

        public void Move(Vector2 direction, float speed)
        {
            if (direction.x < 0) _spriteRenderer.flipX = true;
            else if (direction.x > 0)
            {
                _spriteRenderer.flipX = false;
            }
   
            _rb.linearVelocity = new Vector2(direction.x * speed, _rb.linearVelocity.y);
        }
    }
}
