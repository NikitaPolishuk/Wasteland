using UnityEngine;

namespace Assets.Scripts.MoveControllers
{
    public class TransformMoveController : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer _spriteRenderer;
        [SerializeField] private LayerMask _collisionMask;
        [SerializeField] private float _collisionRadius = 0.3f;

        public void Move(Vector2 direction, float speed)
        {
            if (direction.x < 0) _spriteRenderer.flipX = true;
            else if (direction.x > 0) _spriteRenderer.flipX = false;

            Vector2 newPos = (Vector2)transform.position + direction * speed * Time.deltaTime;

            if (!Physics2D.OverlapCircle(newPos, _collisionRadius, _collisionMask))
            {
                transform.position = newPos;
            }
        }

        public bool CanMove(Vector2 direction, float speed)
        {
            Vector2 newPos = (Vector2)transform.position + direction * speed * Time.deltaTime;
            return !Physics2D.OverlapCircle(newPos, _collisionRadius, _collisionMask);
        }
    }
}
