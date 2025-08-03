using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BuildPrice : MonoBehaviour
{
    [SerializeField] private List<Image> _spriteRenderers;

    private void Start()
    {
        ResetPrice();
    }

    public void ResetPrice()
    {
        foreach (var sprite in _spriteRenderers)
        {
            sprite.color = Color.grey;
        }
    }

    public void ActivePrice(bool active)
    {
        gameObject.SetActive(active);
    }

}
