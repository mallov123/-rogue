using System;
using System.Collections.Generic;
using UnityEngine;

public class DeckManager : MonoBehaviour
{
    public static DeckManager Instance { get; private set; }

    [Header("Config")]
    public BattleConfig battleConfig;

    [Header("Card Prefab")]
    public GameObject cardPrefab;

    // 三个牌堆（存储数据）
    private List<CardData> _drawPile = new();
    private List<CardData> _handCards = new();
    private List<CardData> _discardPile = new();

    // 当前回合已弃牌次数
    private int _discardsUsedThisTurn;

    // 事件
    public event Action<List<CardData>> OnHandChanged;
    public event Action<int, int> OnPileCountChanged; // (drawCount, discardCount)
    public event Action<int> OnDiscardsRemainingChanged; // 剩余弃牌次数

    public int DrawPileCount   => _drawPile.Count;
    public int DiscardPileCount => _discardPile.Count;
    public int DiscardsRemaining => battleConfig.maxDiscardsPerTurn - _discardsUsedThisTurn;
    public List<CardData> HandCards => _handCards;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    /// <summary>用给定牌组初始化抽牌堆并洗牌</summary>
    public void InitDeck(List<CardData> allCards)
    {
        _drawPile = new List<CardData>(allCards);
        _handCards.Clear();
        _discardPile.Clear();
        _discardsUsedThisTurn = 0;
        ShuffleDrawPile();
        BroadcastPileCount();
    }

    /// <summary>回合开始：重置弃牌计数，补满手牌</summary>
    public void StartTurn()
    {
        _discardsUsedThisTurn = 0;
        OnDiscardsRemainingChanged?.Invoke(DiscardsRemaining);
        DrawToFull();
    }

    /// <summary>抽牌至手牌上限</summary>
    public void DrawToFull()
    {
        int needed = battleConfig.maxHandSize - _handCards.Count;
        DrawCards(needed);
    }

    /// <summary>抽 n 张牌；抽牌堆不足时将弃牌堆洗回</summary>
    public void DrawCards(int count)
    {
        for (int i = 0; i < count; i++)
        {
            if (_drawPile.Count == 0)
            {
                if (_discardPile.Count == 0) break;
                RecycleDiscardPile();
            }
            int last = _drawPile.Count - 1;
            _handCards.Add(_drawPile[last]);
            _drawPile.RemoveAt(last);
        }
        OnHandChanged?.Invoke(_handCards);
        BroadcastPileCount();
    }

    /// <summary>弃掉指定牌并重抽相同数量；返回是否成功</summary>
    public bool DiscardAndRedraw(List<CardData> toDiscard)
    {
        if (DiscardsRemaining <= 0) return false;
        if (toDiscard == null || toDiscard.Count == 0) return false;

        int count = toDiscard.Count;
        foreach (var card in toDiscard)
        {
            if (_handCards.Remove(card))
                _discardPile.Add(card);
        }

        _discardsUsedThisTurn++;
        OnDiscardsRemainingChanged?.Invoke(DiscardsRemaining);

        DrawCards(count);
        return true;
    }

    /// <summary>出牌：将手牌移入弃牌堆，回合出牌后调用</summary>
    public void PlayCards(List<CardData> played)
    {
        foreach (var card in played)
        {
            if (_handCards.Remove(card))
                _discardPile.Add(card);
        }
        OnHandChanged?.Invoke(_handCards);
        BroadcastPileCount();
    }

    /// <summary>回合结束：将剩余手牌移入弃牌堆</summary>
    public void EndTurn()
    {
        _discardPile.AddRange(_handCards);
        _handCards.Clear();
        OnHandChanged?.Invoke(_handCards);
        BroadcastPileCount();
    }

    private void ShuffleDrawPile()
    {
        for (int i = _drawPile.Count - 1; i > 0; i--)
        {
            int j = UnityEngine.Random.Range(0, i + 1);
            (_drawPile[i], _drawPile[j]) = (_drawPile[j], _drawPile[i]);
        }
    }

    private void RecycleDiscardPile()
    {
        _drawPile.AddRange(_discardPile);
        _discardPile.Clear();
        ShuffleDrawPile();
        BroadcastPileCount();
    }

    private void BroadcastPileCount()
    {
        OnPileCountChanged?.Invoke(_drawPile.Count, _discardPile.Count);
    }
}
