using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 战斗主控制器，管理回合流程
/// 挂在场景根对象 CombatManager 上
/// </summary>
public class CombatManager : MonoBehaviour
{
    public static CombatManager Instance { get; private set; }

    [Header("测试用卡牌数据（把 CardData 资产拖进来）")]
    public List<CardData> testDeck = new();

    [Header("UI 引用")]
    public Button playButton;       // "出牌" 按钮
    public Button discardButton;    // "弃牌" 按钮（可选）
    public TextMeshProUGUI phaseText;   // 当前回合阶段文字
    public TextMeshProUGUI shieldText;  // 玩家护盾值文字

    [Header("玩家属性")]
    public int playerShield = 0;

    public enum CombatPhase { PlayerTurn, EnemyTurn, Victory, Defeat }
    public CombatPhase CurrentPhase { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Start()
    {
        // 按钮绑定
        if (playButton != null)
            playButton.onClick.AddListener(OnPlayButtonClicked);
        if (discardButton != null)
            discardButton.onClick.AddListener(OnDiscardButtonClicked);

        StartCombat();
    }

    private void StartCombat()
    {
        // 洗牌并发手牌（简单测试：直接用 Inspector 配置的 testDeck 前8张）
        var dealList = new List<CardData>(testDeck);
        if (dealList.Count == 0)
        {
            Debug.LogWarning("[CombatManager] testDeck 为空，请在 Inspector 中添加 CardData！");
        }

        HandManager.Instance?.DealHand(dealList);
        SetPhase(CombatPhase.PlayerTurn);
    }

    // ── 按钮响应 ──────────────────────────────────────────────

    private void OnPlayButtonClicked()
    {
        if (CurrentPhase != CombatPhase.PlayerTurn) return;

        var played = HandManager.Instance?.PlaySelectedCards();
        if (played == null || played.Count == 0)
        {
            Debug.Log("[Combat] 没有选中的牌！");
            return;
        }

        // 评分
        var result = ScoreSystem.EvaluateHand(played);
        Debug.Log($"[Combat] 打出 {result.handName}，获得 {result.TotalScore} 分");

        ScoreManager.Instance?.AddScore(result);

        // 简单模型：每次出牌后进入敌人回合
        SetPhase(CombatPhase.EnemyTurn);
        EnemyAttack();
    }

    private void OnDiscardButtonClicked()
    {
        if (CurrentPhase != CombatPhase.PlayerTurn) return;

        // 弃掉选中的牌（不评分，不触发技能）
        HandManager.Instance?.PlaySelectedCards();
        Debug.Log("[Combat] 弃牌完成");
    }

    // ── 敌人回合 ──────────────────────────────────────────────

    private void EnemyAttack()
    {
        // 简单示例：敌人固定攻击 12 点
        int damage = 12;
        int finalDamage = Mathf.Max(0, damage - playerShield);
        playerShield = Mathf.Max(0, playerShield - damage);

        Debug.Log($"[Enemy Attack] 受到 {damage} 伤害，护盾抵消后实际扣 {finalDamage} 点");
        // TODO: 扣减玩家 HP，连接玩家 HP UI

        RefreshShieldUI();
        SetPhase(CombatPhase.PlayerTurn); // 敌人攻击后回到玩家回合
    }

    // ── 对外接口 ──────────────────────────────────────────────

    public void GainShield(int amount)
    {
        playerShield += amount;
        Debug.Log($"[Combat] 获得 {amount} 护盾，当前护盾: {playerShield}");
        RefreshShieldUI();
    }

    public void OnEnemyDefeated()
    {
        SetPhase(CombatPhase.Victory);
        if (playButton != null) playButton.interactable = false;
        Debug.Log("[Combat] 战斗胜利！");
        // TODO: 跳转到奖励界面 / 地图
    }

    // ── 私有工具 ──────────────────────────────────────────────

    private void SetPhase(CombatPhase phase)
    {
        CurrentPhase = phase;
        if (phaseText != null)
        {
            phaseText.text = phase switch
            {
                CombatPhase.PlayerTurn => "玩家回合 — 选牌并出牌",
                CombatPhase.EnemyTurn  => "敌人回合...",
                CombatPhase.Victory    => "胜利！",
                CombatPhase.Defeat     => "失败...",
                _                      => ""
            };
        }
    }

    private void RefreshShieldUI()
    {
        if (shieldText != null)
            shieldText.text = $"护盾: {playerShield}";
    }
}
