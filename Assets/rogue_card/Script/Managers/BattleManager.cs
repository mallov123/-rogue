using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BattleManager : MonoBehaviour
{
    public static BattleManager Instance { get; private set; }

    [Header("References")]
    public BattleConfig battleConfig;
    public DeckManager  deckManager;
    public Enemy        enemy;

    [Header("Player Stats")]
    public int playerMaxHP = 100;

    // 运行时玩家状态
    public int  PlayerHP    { get; private set; }
    public int  PlayerBlock { get; private set; }

    // 战斗状态
    public BattleState State { get; private set; }

    // 当前选中的手牌（CardData）
    private readonly List<CardData> _selectedCards = new();

    // 事件
    public event Action<BattleState>       OnStateChanged;
    public event Action<int, int>          OnPlayerHPChanged;     // (current, max)
    public event Action<int>               OnPlayerBlockChanged;
    public event Action<HandResult>        OnHandEvaluated;       // 出牌后广播牌型结果
    public event Action<ThresholdData>     OnThresholdTriggered;  // 每触发一个阈值广播一次
    public event Action<float>             OnScoreChanged;        // 当前分数
    public event Action<List<CardData>>    OnSelectedCardsChanged;

    private float _currentScore;

    public enum BattleState
    {
        Idle,
        DrawPhase,
        PlayerTurn,
        EnemyTurn,
        Victory,
        Defeat
    }

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    // ─────────────────────────────────────────────
    //  战斗入口
    // ─────────────────────────────────────────────

    public void StartBattle(List<CardData> deck)
    {
        PlayerHP    = playerMaxHP;
        PlayerBlock = 0;
        _currentScore = 0;
        _selectedCards.Clear();

        OnPlayerHPChanged?.Invoke(PlayerHP, playerMaxHP);
        OnPlayerBlockChanged?.Invoke(PlayerBlock);

        deckManager.InitDeck(deck);
        enemy.Initialize();

        StartCoroutine(DrawPhaseRoutine());
    }

    // ─────────────────────────────────────────────
    //  回合流程
    // ─────────────────────────────────────────────

    private IEnumerator DrawPhaseRoutine()
    {
        SetState(BattleState.DrawPhase);
        deckManager.StartTurn();
        yield return null; // 等一帧让UI刷新
        SetState(BattleState.PlayerTurn);
    }

    // ─────────────────────────────────────────────
    //  玩家操作
    // ─────────────────────────────────────────────

    /// <summary>UI层调用：切换某张牌的选中状态</summary>
    public void ToggleCardSelection(CardData card)
    {
        if (State != BattleState.PlayerTurn) return;

        if (_selectedCards.Contains(card))
            _selectedCards.Remove(card);
        else
            _selectedCards.Add(card);

        OnSelectedCardsChanged?.Invoke(_selectedCards);
    }

    /// <summary>出牌：评估牌型 → 计算分数 → 触发阈值 → 结束玩家回合</summary>
    public void PlaySelectedCards()
    {
        if (State != BattleState.PlayerTurn) return;
        if (_selectedCards.Count == 0) return;

        HandResult result = HandEvaluator.Evaluate(_selectedCards);
        _currentScore = result.baseScore;

        OnHandEvaluated?.Invoke(result);
        OnScoreChanged?.Invoke(_currentScore);

        // 将出的牌移入弃牌堆
        deckManager.PlayCards(new List<CardData>(_selectedCards));
        _selectedCards.Clear();
        OnSelectedCardsChanged?.Invoke(_selectedCards);

        // 触发所有达到的阈值（从低到高）
        var sorted = battleConfig.thresholds
            .Where(t => t != null)
            .OrderBy(t => t.scoreThreshold)
            .ToList();

        foreach (var threshold in sorted)
        {
            if (_currentScore >= threshold.scoreThreshold)
            {
                OnThresholdTriggered?.Invoke(threshold);
                ApplyThresholdAction(threshold);
            }
        }

        StartCoroutine(EndPlayerTurnRoutine());
    }

    /// <summary>弃牌并重抽：不消耗出牌机会</summary>
    public void DiscardAndRedraw()
    {
        if (State != BattleState.PlayerTurn) return;
        if (_selectedCards.Count == 0) return;

        bool success = deckManager.DiscardAndRedraw(new List<CardData>(_selectedCards));
        if (success)
        {
            _selectedCards.Clear();
            OnSelectedCardsChanged?.Invoke(_selectedCards);
        }
    }

    // ─────────────────────────────────────────────
    //  阈值行动执行
    // ─────────────────────────────────────────────

    private void ApplyThresholdAction(ThresholdData threshold)
    {
        switch (threshold.actionType)
        {
            case ActionType.Attack:
                enemy.TakeDamage(threshold.effectValue);
                CheckVictory();
                break;

            case ActionType.Block:
                PlayerBlock += threshold.effectValue;
                OnPlayerBlockChanged?.Invoke(PlayerBlock);
                break;

            case ActionType.Buff:
                // 预留：可对玩家施加 Strength 等效果
                break;

            case ActionType.Debuff:
                // 预留：可对敌人施加 Weak / Vulnerable 等效果
                break;
        }
    }

    // ─────────────────────────────────────────────
    //  敌人回合
    // ─────────────────────────────────────────────

    private IEnumerator EndPlayerTurnRoutine()
    {
        if (State == BattleState.Victory || State == BattleState.Defeat) yield break;

        deckManager.EndTurn();
        yield return new WaitForSeconds(0.5f);

        SetState(BattleState.EnemyTurn);
        yield return new WaitForSeconds(0.5f);

        ExecuteEnemyTurn();
    }

    private void ExecuteEnemyTurn()
    {
        enemy.ProcessTurnStartEffects();
        if (State == BattleState.Victory) return;

        IntentData intent = enemy.ExecuteIntent(out int value);
        if (intent == null) { StartCoroutine(DrawPhaseRoutine()); return; }

        switch (intent.actionType)
        {
            case ActionType.Attack:
                TakePlayerDamage(value);
                break;
            case ActionType.Block:
                enemy.AddBlock(value);
                break;
            case ActionType.Buff:
                enemy.AddStatusEffect(StatusEffectType.Strength, value, -1);
                break;
            case ActionType.Debuff:
                // 对玩家施加虚弱/易伤等未来扩展
                break;
        }

        if (State != BattleState.Defeat)
            StartCoroutine(DrawPhaseRoutine());
    }

    // ─────────────────────────────────────────────
    //  玩家受伤
    // ─────────────────────────────────────────────

    private void TakePlayerDamage(int amount)
    {
        int blocked = Mathf.Min(PlayerBlock, amount);
        PlayerBlock -= blocked;
        amount      -= blocked;

        PlayerHP = Mathf.Max(0, PlayerHP - amount);

        OnPlayerBlockChanged?.Invoke(PlayerBlock);
        OnPlayerHPChanged?.Invoke(PlayerHP, playerMaxHP);

        if (PlayerHP <= 0)
        {
            SetState(BattleState.Defeat);
        }
    }

    // ─────────────────────────────────────────────
    //  胜负检查
    // ─────────────────────────────────────────────

    private void CheckVictory()
    {
        if (enemy.IsDead)
            SetState(BattleState.Victory);
    }

    // ─────────────────────────────────────────────
    //  工具
    // ─────────────────────────────────────────────

    private void SetState(BattleState newState)
    {
        State = newState;
        OnStateChanged?.Invoke(newState);
    }
}
