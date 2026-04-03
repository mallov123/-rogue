using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 纯静态评分逻辑 + 场景中负责分数UI的 ScoreManager
/// </summary>
/// 
// ── 评分结果 ──────────────────────────────────────────────────
public struct HandResult
{
    public string handName;
    public int    baseScore;
    public int    multiplier;
    public int    TotalScore => baseScore * multiplier;
}

// ── 阈值能力定义 ──────────────────────────────────────────────
[Serializable]
public class ScoreThreshold
{
    public string abilityName;          // 能力名称
    public int    requiredScore;        // 需要积累的总分
    public int    attackDamage;         // 触发后对敌人造成的伤害
    public int    shieldAmount;         // 触发后获得的护盾值
    [TextArea] public string description;
}

// ── 静态牌型判断 ──────────────────────────────────────────────
public static class ScoreSystem
{
    public static HandResult EvaluateHand(List<CardData> cards)
    {
        if (cards == null || cards.Count == 0)
            return new HandResult { handName = "无牌", baseScore = 0, multiplier = 1 };

        var values = cards.Select(c => c.value).OrderBy(v => v).ToList();
        var suits  = cards.Select(c => c.suit).ToList();

        bool isFlush    = suits.Distinct().Count() == 1;
        bool isStraight = IsStraight(values);

        var groups = values.GroupBy(v => v).OrderByDescending(g => g.Count()).ToList();
        int maxGroup  = groups[0].Count();
        int nextGroup = groups.Count > 1 ? groups[1].Count() : 0;

        // 从高到低匹配牌型
        if (isFlush && isStraight && values.Contains(1) && values.Contains(13))
            return Make("皇家同花顺", 100, 8);
        if (isFlush && isStraight)
            return Make("同花顺",    80, 6);
        if (maxGroup == 4)
            return Make("四条",      60, 5);
        if (maxGroup == 3 && nextGroup == 2)
            return Make("葫芦",      40, 4);
        if (isFlush)
            return Make("同花",      35, 4);
        if (isStraight)
            return Make("顺子",      30, 3);
        if (maxGroup == 3)
            return Make("三条",      20, 3);
        if (maxGroup == 2 && nextGroup == 2)
            return Make("两对",      20, 2);
        if (maxGroup == 2)
            return Make("一对",      10, 2);

        // 高牌（所有牌点数之和）
        int highScore = values.Sum();
        return new HandResult { handName = "高牌", baseScore = highScore, multiplier = 1 };
    }

    private static HandResult Make(string name, int score, int mult) =>
        new HandResult { handName = name, baseScore = score, multiplier = mult };

    private static bool IsStraight(List<int> sortedValues)
    {
        if (sortedValues.Count < 5) return false;
        // 普通顺子
        for (int i = 1; i < sortedValues.Count; i++)
            if (sortedValues[i] - sortedValues[i - 1] != 1) return false;
        return true;
    }
}

// ── ScoreManager（挂在场景对象上）────────────────────────────
public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }

    [Header("阈值能力配置（Inspector中填写）")]
    public List<ScoreThreshold> thresholds = new()
    {
        new ScoreThreshold { abilityName="小攻击", requiredScore=50,  attackDamage=10, description="对敌人造成10点伤害" },
        new ScoreThreshold { abilityName="格挡",   requiredScore=100, shieldAmount=15, description="获得15点护盾" },
        new ScoreThreshold { abilityName="重击",   requiredScore=200, attackDamage=30, description="对敌人造成30点伤害" },
    };

    [Header("UI 引用")]
    public TextMeshProUGUI totalScoreText;       // 当前总分显示
    public TextMeshProUGUI nextThresholdText;    // 下一个阈值提示
    public Slider          scoreProgressSlider;  // 进度条
    public Transform       thresholdButtonContainer; // 已解锁能力按钮的父节点

    [Header("按钮 Prefab")]
    public GameObject abilityButtonPrefab;       // 能力按钮 Prefab（含 Button + TMP）

    public int TotalScore { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Start() => RefreshUI();

    /// <summary>出牌后调用，传入本次牌型结果</summary>
    public void AddScore(HandResult result)
    {
        TotalScore += result.TotalScore;
        Debug.Log($"[Score] {result.handName} → +{result.TotalScore}分 (总计: {TotalScore})");

        CheckThresholds();
        RefreshUI();
    }

    private void CheckThresholds()
    {
        foreach (var t in thresholds)
        {
            if (TotalScore >= t.requiredScore)
                TriggerAbility(t);
        }
    }

    private void TriggerAbility(ScoreThreshold threshold)
    {
        Debug.Log($"[Ability] 触发能力：{threshold.abilityName}");

        if (threshold.attackDamage > 0)
            EnemyDisplay.Instance?.TakeDamage(threshold.attackDamage);

        if (threshold.shieldAmount > 0)
            CombatManager.Instance?.GainShield(threshold.shieldAmount);

        // 触发后从列表移除，防止重复触发
        thresholds.Remove(threshold);

        // 将能力显示为已解锁按钮
        if (abilityButtonPrefab != null && thresholdButtonContainer != null)
        {
            var btn = Instantiate(abilityButtonPrefab, thresholdButtonContainer);
            var label = btn.GetComponentInChildren<TextMeshProUGUI>();
            if (label != null) label.text = $"✓ {threshold.abilityName}";
        }
    }

    private void RefreshUI()
    {
        if (totalScoreText != null)
            totalScoreText.text = $"分数: {TotalScore}";

        // 找下一个未触发阈值
        var next = thresholds.OrderBy(t => t.requiredScore).FirstOrDefault();
        if (next != null)
        {
            if (nextThresholdText != null)
                nextThresholdText.text = $"下一个: {next.abilityName} ({TotalScore}/{next.requiredScore})";

            if (scoreProgressSlider != null)
            {
                scoreProgressSlider.maxValue = next.requiredScore;
                scoreProgressSlider.value    = Mathf.Min(TotalScore, next.requiredScore);
            }
        }
        else
        {
            if (nextThresholdText != null)
                nextThresholdText.text = "所有能力已解锁！";
        }
    }
}
