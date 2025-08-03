using UnityEngine;

namespace Assets.Scripts.Interfaces
{
    public interface IMoveController
    {
        void Move(Vector2 direction, float speed);
    }
}

