using UnityEngine;

/// <summary>
/// 卡牌的静态数据，创建方式：右键 Assets → Create → RogueCard → Card Data
/// </summary>
[CreateAssetMenu(fileName = "NewCard", menuName = "RogueCard/Card Data")]
public class CardData : ScriptableObject
{
    [Header("基础属性")]
    public string cardName = "未命名卡牌";
    public int value;           // 点数 1-13 (A=1, J=11, Q=12, K=13)
    public CardSuit suit;       // 花色
    public Sprite artwork;      // 卡面图片

    [Header("描述")]
    [TextArea] public string description;

    /// <summary>用于显示的点数文本 (1→A, 11→J, 12→Q, 13→K)</summary>
    public string DisplayValue
    {
        get
        {
            return value switch
            {
                1 => "A",
                11 => "J",
                12 => "Q",
                13 => "K",
                _ => value.ToString()
            };
        }
    }
}

public enum CardSuit
{
    Spades,     // 黑桃 ♠
    Hearts,     // 红心 ♥
    Diamonds,   // 方块 ♦
    Clubs       // 梅花 ♣
}
