using UnityEngine;

namespace Assets.Scripts.World
{
    public class TreeController : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer _spriteRenderer;

        public void BackTree(bool back)
        {
            if(back)
            {
                _spriteRenderer.sortingLayerName = "BackgroundFront";
                _spriteRenderer.sortingOrder = 3;
                _spriteRenderer.color = Color.gray7;
            }
        }
    }
}
