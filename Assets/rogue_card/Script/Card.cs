using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

/// <summary>
/// 挂在每张卡牌 Prefab 上，负责显示和交互
/// </summary>
public class Card : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    [Header("UI 引用")]
    public Image artworkImage;          // 卡面图片
    public TextMeshProUGUI valueText;   // 左上角点数
    public TextMeshProUGUI suitText;    // 左上角花色符号
    public Image suitColorImage;        // 花色颜色标记（可选）
    public Image selectionHighlight;    // 选中时高亮边框

    // 运行时数据
    public CardData Data { get; private set; }
    public bool IsSelected { get; private set; }

    // 卡牌在手牌中的原始位置（用于选中抬起动画）
    private Vector3 _basePosition;
    private const float SELECTED_OFFSET_Y = 40f;   // 选中时向上偏移像素

    public void Initialize(CardData data)
    {
        Data = data;

        if (artworkImage != null && data.artwork != null)
            artworkImage.sprite = data.artwork;

        // if (valueText != null)
        //     valueText.text = data.DisplayValue;

        // if (suitText != null)
        //     suitText.text = GetSuitSymbol(data.suit);

        // // 红色花色（红心/方块）使用红色文字
        // Color suitColor = (data.suit == CardSuit.Hearts || data.suit == CardSuit.Diamonds)
        //     ? Color.red : Color.black;

        // if (valueText != null)   valueText.color = suitColor;
        // if (suitText != null)    suitText.color = suitColor;

        SetSelected(false);
    }

    /// <summary>记录卡牌在手牌布局中的基础位置</summary>
    public void SetBasePosition(Vector3 pos)
    {
        _basePosition = pos;
        transform.localPosition = pos;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        ToggleSelected();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        // 悬停时轻微放大
        transform.localScale = Vector3.one * 1.08f;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        transform.localScale = Vector3.one;
    }

    public void ToggleSelected()
    {
        SetSelected(!IsSelected);
        HandManager.Instance?.OnCardSelectionChanged();
    }

    public void SetSelected(bool selected)
    {
        IsSelected = selected;

        if (selectionHighlight != null)
            selectionHighlight.gameObject.SetActive(selected);

        // 选中时卡牌上移
        Vector3 target = _basePosition + Vector3.up * (selected ? SELECTED_OFFSET_Y : 0f);
        transform.localPosition = target;
    }

    private string GetSuitSymbol(CardSuit suit)
    {
        return suit switch
        {
            CardSuit.Spades   => "♠",
            CardSuit.Hearts   => "♥",
            CardSuit.Diamonds => "♦",
            CardSuit.Clubs    => "♣",
            _                 => "?"
        };
    }
}
