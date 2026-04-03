using UnityEngine;
using UnityEngine.UI;

public class Card : MonoBehaviour
{
    [Header("Data")]
    public CardData data;

    [Header("UI References")]
    public Image artworkImage;
    public Image selectionHighlight;

    private bool _isSelected;

    public bool IsSelected => _isSelected;

    public void Initialize(CardData cardData)
    {
        data = cardData;

        if (cardData.artwork != null)
            artworkImage.sprite = cardData.artwork;

        SetSelected(false);
    }

    public void SetSelected(bool selected)
    {
        _isSelected = selected;
        if (selectionHighlight != null)
            selectionHighlight.gameObject.SetActive(selected);
    }

    public void OnClick()
    {
        SetSelected(!_isSelected);
    }
}
