using System.Collections.Generic;
using System.Linq;

/// <summary>
/// 纯静态工具类，评估1-5张牌的最佳德州扑克牌型。
/// 1张 → 最高只能是 HighCard
/// 2张 → 最高 OnePair
/// 3张 → 最高 ThreeOfAKind
/// 4张 → 最高 FourOfAKind（或 TwoPair / Straight / Flush）
/// 5张 → 全牌型均可
/// </summary>
public static class HandEvaluator
{
    public static HandResult Evaluate(List<CardData> cards)
    {
        if (cards == null || cards.Count == 0)
            return new HandResult(HandType.HighCard, HandResult.BaseScores[(int)HandType.HighCard]);

        int count = cards.Count;
        var values = cards.Select(c => c.value).OrderBy(v => v).ToList();
        var suits  = cards.Select(c => c.suit).ToList();

        bool isFlush    = count >= 5 && suits.Distinct().Count() == 1;
        bool isStraight = count >= 5 && IsStraight(values);
        var  groups     = values.GroupBy(v => v).OrderByDescending(g => g.Count()).ThenByDescending(g => g.Key).ToList();
        int  maxGroup   = groups[0].Count();

        // 5张：同花顺 / 皇家同花顺
        if (count == 5 && isFlush && isStraight)
        {
            bool isRoyal = values.SequenceEqual(new[] { 1, 10, 11, 12, 13 }) ||
                           values.SequenceEqual(new[] { 10, 11, 12, 13, 14 });
            HandType t = isRoyal ? HandType.RoyalFlush : HandType.StraightFlush;
            return Make(t);
        }

        // 四条
        if (maxGroup == 4)
            return Make(HandType.FourOfAKind);

        // 葫芦（三条+对子），至少需要5张或3+2的分组（理论上需要至少5张）
        if (maxGroup == 3 && groups.Count >= 2 && groups[1].Count() == 2)
            return Make(HandType.FullHouse);

        // 同花（5张）
        if (isFlush)
            return Make(HandType.Flush);

        // 顺子（5张）
        if (isStraight)
            return Make(HandType.Straight);

        // 三条
        if (maxGroup == 3)
            return Make(HandType.ThreeOfAKind);

        // 两对
        int pairCount = groups.Count(g => g.Count() == 2);
        if (pairCount >= 2)
            return Make(HandType.TwoPair);

        // 一对
        if (maxGroup == 2)
            return Make(HandType.OnePair);

        // 高牌
        return Make(HandType.HighCard);
    }

    private static HandResult Make(HandType type)
    {
        return new HandResult(type, HandResult.BaseScores[(int)type]);
    }

    /// <summary>判断是否为顺子（含 A-low: A,2,3,4,5）</summary>
    private static bool IsStraight(List<int> sortedValues)
    {
        if (sortedValues.Count != 5) return false;

        // 普通顺子
        bool normal = true;
        for (int i = 1; i < sortedValues.Count; i++)
        {
            if (sortedValues[i] - sortedValues[i - 1] != 1)
            {
                normal = false;
                break;
            }
        }
        if (normal) return true;

        // A-low 顺子：A,2,3,4,5（存储为1,2,3,4,5）
        return sortedValues.SequenceEqual(new[] { 1, 2, 3, 4, 5 });
    }
}
