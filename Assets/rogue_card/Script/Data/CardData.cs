using UnityEngine;

[CreateAssetMenu(fileName = "NewCard", menuName = "RogueCard/CardData")]
public class CardData : ScriptableObject
{
    public Suit suit;
    [Range(1, 13)]
    public int value; // 1=Ace, 11=Jack, 12=Queen, 13=King
    public Sprite artwork;

}
