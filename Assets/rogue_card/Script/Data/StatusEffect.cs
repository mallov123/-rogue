public enum StatusEffectType
{
    Poison,   // 每回合开始扣血
    Burn,     // 每回合开始扣血（不可叠加层数）
    Shield,   // 额外护甲
    Strength, // 提升攻击力
    Weak,     // 降低攻击力
    Vulnerable// 承受更多伤害
}

[System.Serializable]
public class StatusEffect
{
    public StatusEffectType type;
    public int magnitude;  // 效果值（层数或数值）
    public int duration;   // 剩余回合数，-1 = 永久

    public StatusEffect(StatusEffectType type, int magnitude, int duration)
    {
        this.type      = type;
        this.magnitude = magnitude;
        this.duration  = duration;
    }
}
