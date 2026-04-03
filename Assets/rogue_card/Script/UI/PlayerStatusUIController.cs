using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerStatusUIController : MonoBehaviour
{
    [Header("HP")]
    public Slider          hpBar;
    public TextMeshProUGUI hpText;

    [Header("Block")]
    public GameObject      blockPanel; // 有护甲时显示
    public TextMeshProUGUI blockText;

    [Header("Pile Counts")]
    public TextMeshProUGUI drawPileCountText;
    public TextMeshProUGUI discardPileCountText;

    [Header("Discard Remaining")]
    public TextMeshProUGUI discardRemainingText;
    [Tooltip("引用 DeckManager 以订阅牌堆数量事件")]
    public DeckManager deckManager;

    private void Start()
    {
        if (deckManager != null)
        {
            deckManager.OnPileCountChanged      += UpdatePileCounts;
            deckManager.OnDiscardsRemainingChanged += UpdateDiscardsRemaining;
        }
    }

    private void OnDestroy()
    {
        if (deckManager != null)
        {
            deckManager.OnPileCountChanged         -= UpdatePileCounts;
            deckManager.OnDiscardsRemainingChanged -= UpdateDiscardsRemaining;
        }
    }

    // ── HP（由 BattleUIController 通过 BattleManager 事件调用）──

    public void UpdateHP(int current, int max)
    {
        if (hpBar != null)
        {
            hpBar.maxValue = max;
            hpBar.value    = current;
        }
        if (hpText != null)
            hpText.text = $"{current} / {max}";
    }

    // ── 护甲 ─────────────────────────────────────

    public void UpdateBlock(int block)
    {
        if (blockPanel != null)
            blockPanel.SetActive(block > 0);
        if (blockText != null)
            blockText.text = block.ToString();
    }

    // ── 牌堆计数 ─────────────────────────────────

    private void UpdatePileCounts(int drawCount, int discardCount)
    {
        if (drawPileCountText != null)
            drawPileCountText.text = drawCount.ToString();
        if (discardPileCountText != null)
            discardPileCountText.text = discardCount.ToString();
    }

    // ── 剩余弃牌次数 ──────────────────────────────

    private void UpdateDiscardsRemaining(int remaining)
    {
        if (discardRemainingText != null)
            discardRemainingText.text = $"弃牌: {remaining}";
    }
}
