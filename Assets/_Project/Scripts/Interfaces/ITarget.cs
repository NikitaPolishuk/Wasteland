using UnityEngine;

namespace Assets.Scripts.Interfaces
{
    public interface ITarget
    {
        Transform Transform { get; }
        bool IsAlive { get; }
    }
}
