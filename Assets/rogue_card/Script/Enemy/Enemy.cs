using System;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    [Header("Data")]
    public EnemyData data;

    // 运行时状态
    public int CurrentHP  { get; private set; }
    public int MaxHP      => data.maxHP;
    public int Block      { get; private set; }
    public bool IsDead    => CurrentHP <= 0;

    public List<StatusEffect> ActiveEffects { get; private set; } = new();

    private int _intentIndex;

    // 事件
    public event Action<int, int> OnHPChanged;     // (current, max)
    public event Action<int>      OnBlockChanged;   // (block)
    public event Action<List<StatusEffect>> OnEffectsChanged;
    public event Action<IntentData>         OnIntentChanged;

    public void Initialize()
    {
        CurrentHP     = data.maxHP;
        Block         = 0;
        _intentIndex  = 0;
        ActiveEffects.Clear();

        OnHPChanged?.Invoke(CurrentHP, MaxHP);
        OnBlockChanged?.Invoke(Block);
        BroadcastIntent();
    }

    /// <summary>承受伤害（先抵消护甲）</summary>
    public void TakeDamage(int amount)
    {
        // 易伤：承受伤害+50%
        if (HasEffect(StatusEffectType.Vulnerable))
            amount = Mathf.RoundToInt(amount * 1.5f);

        int blocked = Mathf.Min(Block, amount);
        Block      -= blocked;
        amount     -= blocked;

        CurrentHP = Mathf.Max(0, CurrentHP - amount);

        OnBlockChanged?.Invoke(Block);
        OnHPChanged?.Invoke(CurrentHP, MaxHP);
    }

    /// <summary>获得护甲</summary>
    public void AddBlock(int amount)
    {
        Block += amount;
        OnBlockChanged?.Invoke(Block);
    }

    /// <summary>施加状态效果（相同类型叠加 magnitude）</summary>
    public void AddStatusEffect(StatusEffectType type, int magnitude, int duration)
    {
        var existing = ActiveEffects.Find(e => e.type == type);
        if (existing != null)
        {
            existing.magnitude += magnitude;
            existing.duration   = Mathf.Max(existing.duration, duration);
        }
        else
        {
            ActiveEffects.Add(new StatusEffect(type, magnitude, duration));
        }
        OnEffectsChanged?.Invoke(ActiveEffects);
    }

    /// <summary>执行当前意图，并切换到下一个</summary>
    public IntentData ExecuteIntent(out int resolvedValue)
    {
        IntentData intent = GetCurrentIntent();
        resolvedValue = CalculateIntentValue(intent);

        // 切换到下一意图（循环）
        _intentIndex = (data.intents.Count > 0)
            ? (_intentIndex + 1) % data.intents.Count
            : 0;
        BroadcastIntent();
        return intent;
    }

    public IntentData GetCurrentIntent()
    {
        if (data.intents == null || data.intents.Count == 0) return null;
        return data.intents[_intentIndex];
    }

    /// <summary>回合开始时处理持续效果（Poison/Burn等）</summary>
    public void ProcessTurnStartEffects()
    {
        // 护甲每回合归零
        Block = 0;
        OnBlockChanged?.Invoke(Block);

        for (int i = ActiveEffects.Count - 1; i >= 0; i--)
        {
            var effect = ActiveEffects[i];
            switch (effect.type)
            {
                case StatusEffectType.Poison:
                case StatusEffectType.Burn:
                    TakeDamage(effect.magnitude);
                    break;
            }

            if (effect.duration != -1)
            {
                effect.duration--;
                if (effect.duration <= 0)
                    ActiveEffects.RemoveAt(i);
            }
        }
        OnEffectsChanged?.Invoke(ActiveEffects);
    }

    private int CalculateIntentValue(IntentData intent)
    {
        if (intent == null) return 0;
        int value = intent.magnitude;

        // 虚弱：攻击力降低25%
        if (intent.actionType == ActionType.Attack && HasEffect(StatusEffectType.Weak))
            value = Mathf.RoundToInt(value * 0.75f);

        // 力量加成
        var strength = ActiveEffects.Find(e => e.type == StatusEffectType.Strength);
        if (strength != null && intent.actionType == ActionType.Attack)
            value += strength.magnitude;

        return value;
    }

    private bool HasEffect(StatusEffectType type)
        => ActiveEffects.Exists(e => e.type == type);

    private void BroadcastIntent()
    {
        OnIntentChanged?.Invoke(GetCurrentIntent());
    }
}
