using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 监听 DeckManager.OnHandChanged，动态实例化/销毁手牌 Card 对象，
/// 并将玩家点击的选中状态同步到 BattleManager。
/// </summary>
public class HandUIController : MonoBehaviour
{
    [Header("References")]
    public BattleManager battleManager;
    public DeckManager   deckManager;

    [Header("UI")]
    public Transform  handArea;    // HorizontalLayoutGroup 容器
    public GameObject cardPrefab;

    // 当前实例化的手牌 GameObject（CardData → Card 组件）
    private readonly Dictionary<CardData, Card> _cardViews = new();

    private void Start()
    {
        deckManager.OnHandChanged += RefreshHand;
        battleManager.OnSelectedCardsChanged += SyncHighlights;
    }

    private void OnDestroy()
    {
        if (deckManager != null)   deckManager.OnHandChanged -= RefreshHand;
        if (battleManager != null) battleManager.OnSelectedCardsChanged -= SyncHighlights;
    }

    // ── 手牌刷新 ─────────────────────────────────

    private void RefreshHand(List<CardData> hand)
    {
        // 移除已经不在手牌中的卡
        var toRemove = new List<CardData>();
        foreach (var kv in _cardViews)
        {
            if (!hand.Contains(kv.Key))
            {
                Destroy(kv.Value.gameObject);
                toRemove.Add(kv.Key);
            }
        }
        foreach (var key in toRemove)
            _cardViews.Remove(key);

        // 新增手牌
        foreach (var cardData in hand)
        {
            if (_cardViews.ContainsKey(cardData)) continue;

            var go   = Instantiate(cardPrefab, handArea);
            var card = go.GetComponent<Card>();
            card.Initialize(cardData);

            // 点击时通知 BattleManager
            var btn = go.GetComponent<UnityEngine.UI.Button>();
            if (btn != null)
            {
                var captured = cardData;
                btn.onClick.AddListener(() => battleManager.ToggleCardSelection(captured));
            }

            _cardViews[cardData] = card;
        }
    }

    // ── 高亮同步 ─────────────────────────────────

    private void SyncHighlights(List<CardData> selected)
    {
        foreach (var kv in _cardViews)
            kv.Value.SetSelected(selected.Contains(kv.Key));
    }

    /// <summary>出牌/弃牌后由 BattleUIController 调用，清空视觉选中</summary>
    public void ClearSelection()
    {
        foreach (var kv in _cardViews)
            kv.Value.SetSelected(false);
    }
}
