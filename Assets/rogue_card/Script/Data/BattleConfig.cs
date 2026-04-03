using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "BattleConfig", menuName = "RogueCard/BattleConfig")]
public class BattleConfig : ScriptableObject
{
    [Header("Hand Settings")]
    public int maxHandSize = 7;

    [Header("Discard Settings")]
    [Tooltip("每回合最多弃牌次数")]
    public int maxDiscardsPerTurn = 2;

    [Header("Score Thresholds")]
    [Tooltip("按scoreThreshold从小到大排列，最多5个")]
    public List<ThresholdData> thresholds;
}
