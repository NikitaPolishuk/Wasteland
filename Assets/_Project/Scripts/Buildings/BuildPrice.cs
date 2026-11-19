using TMPro;
using UnityEngine;

public class BuildPrice : MonoBehaviour
{
    private const float TEXT_WIDTH = 2f;
    private const float TEXT_HEIGHT = 1f;
    private const float FONT_SIZE = 0.8f;

    private TMP_Text _priceText;

    public void SetPrice(int price)
    {
        if (_priceText == null)
        {
            GameObject textObject = new GameObject("Amount", typeof(RectTransform), typeof(TextMeshProUGUI));
            textObject.transform.SetParent(transform, false);
            RectTransform rect = (RectTransform)textObject.transform;
            rect.sizeDelta = new Vector2(TEXT_WIDTH, TEXT_HEIGHT);
            _priceText = textObject.GetComponent<TMP_Text>();
            _priceText.fontSize = FONT_SIZE;
            _priceText.alignment = TextAlignmentOptions.MidlineLeft;
            _priceText.raycastTarget = false;
        }

        _priceText.text = price.ToString();
    }

    public void ActivePrice(bool active)
    {
        gameObject.SetActive(active);
    }
}
