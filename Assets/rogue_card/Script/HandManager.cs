using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro;

/// <summary>
/// 管理手牌区域：生成/展示手牌、记录选中状态、出牌
/// 单例，挂在场景中的 HandManager 游戏对象上
/// </summary>
public class HandManager : MonoBehaviour
{
    public static HandManager Instance { get; private set; }

    [Header("引用")]
    public GameObject cardPrefab;           // 卡牌 Prefab
    public Transform handContainer;         // 手牌排列的父节点（HorizontalLayoutGroup）
    public TextMeshProUGUI handInfoText;    // 显示当前选中牌型名称

    [Header("手牌布局")]
    public float cardSpacing = 120f;        // 卡牌间距(px)
    public int maxHandSize = 8;             // 最多持有张数

    // 当前手牌
    private List<Card> _hand = new();
    // 当前选中的牌
    public List<Card> SelectedCards => _hand.Where(c => c.IsSelected).ToList();

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    /// <summary>用给定 CardData 列表初始化手牌（战斗开始时调用）</summary>
    public void DealHand(List<CardData> cardDataList)
    {
        ClearHand();
        foreach (var data in cardDataList)
            AddCardToHand(data);
    }

    public void AddCardToHand(CardData data)
    {
        if (_hand.Count >= maxHandSize) return;

        GameObject go = Instantiate(cardPrefab, handContainer);
        Card card = go.GetComponent<Card>();
        card.Initialize(data);
        _hand.Add(card);

        RefreshLayout();
    }

    /// <summary>出牌：移除选中的牌，返回它们的 CardData 用于评分</summary>
    public List<CardData> PlaySelectedCards()
    {
        var selected = SelectedCards;
        if (selected.Count == 0) return new List<CardData>();

        var played = selected.Select(c => c.Data).ToList();
        foreach (var card in selected)
        {
            _hand.Remove(card);
            Destroy(card.gameObject);
        }

        RefreshLayout();
        OnCardSelectionChanged(); // 刷新牌型提示
        return played;
    }

    /// <summary>当选中状态改变时刷新牌型提示文字</summary>
    public void OnCardSelectionChanged()
    {
        var selected = SelectedCards;
        if (handInfoText == null) return;

        if (selected.Count == 0)
        {
            handInfoText.text = "选择你的牌";
            return;
        }

        var result = ScoreSystem.EvaluateHand(selected.Select(c => c.Data).ToList());
        handInfoText.text = $"{result.handName}  +{result.baseScore}分";
    }

    private void ClearHand()
    {
        foreach (var card in _hand)
            if (card != null) Destroy(card.gameObject);
        _hand.Clear();
    }

    /// <summary>等间距排列手牌</summary>
    private void RefreshLayout()
    {
        int count = _hand.Count;
        float totalWidth = (count - 1) * cardSpacing;
        float startX = -totalWidth / 2f;

        for (int i = 0; i < count; i++)
        {
            float x = startX + i * cardSpacing;
            _hand[i].SetBasePosition(new Vector3(x, 0, 0));
        }
    }
}
