using UnityEngine;

[CreateAssetMenu(fileName = "NewThreshold", menuName = "RogueCard/ThresholdData")]
public class ThresholdData : ScriptableObject
{
    public float scoreThreshold;
    public ActionType actionType;
    public int effectValue;
    public Sprite actionIcon;
}
