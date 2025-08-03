using UnityEngine;

namespace Assets.Scripts.Interfaces
{
    public interface IMovable
    {
        float Speed { get; }
        void HorizontalMove(Vector2 direction, float speed);
    }
}
