using System;
using UnityEngine;

namespace Assets.Scripts.Interfaces
{
    public interface IInput
    {
        Action<Vector2> OnMove { get; set; }
        Action OnIntection { get; set; }
    }
}