using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class IntentData
{
    public ActionType actionType;
    public int magnitude;
    public Sprite icon;
}

[CreateAssetMenu(fileName = "NewEnemy", menuName = "RogueCard/EnemyData")]
public class EnemyData : ScriptableObject
{
    public string enemyName;
    public int maxHP;
    public Sprite enemySprite;
    public List<IntentData> intents;
}
