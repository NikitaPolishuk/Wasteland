using UnityEngine;

namespace Assets.Scripts.Interfaces
{
    public interface IMovable
    {
        float Speed { get; }
        IMoveController MoveController { get; }
    }
}
