public enum HandType
{
    HighCard       = 0,
    OnePair        = 1,
    TwoPair        = 2,
    ThreeOfAKind   = 3,
    Straight       = 4,
    Flush          = 5,
    FullHouse      = 6,
    FourOfAKind    = 7,
    StraightFlush  = 8,
    RoyalFlush     = 9
}

public struct HandResult
{
    public HandType handType;
    public int baseScore;

    public HandResult(HandType type, int score)
    {
        handType = type;
        baseScore = score;
    }

    public static readonly int[] BaseScores = new int[]
    {
        5,   // HighCard
        10,  // OnePair
        20,  // TwoPair
        30,  // ThreeOfAKind
        40,  // Straight
        50,  // Flush
        60,  // FullHouse
        80,  // FourOfAKind
        100, // StraightFlush
        150  // RoyalFlush
    };
}
