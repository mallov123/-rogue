using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 总协调器：订阅 BattleManager 事件，控制按钮可用状态，
/// 以及负责初始化战斗（将所有 CardData 传入 BattleManager）。
/// </summary>
public class BattleUIController : MonoBehaviour
{
    [Header("Managers")]
    public BattleManager battleManager;

    [Header("Buttons")]
    public Button playHandButton;
    public Button discardButton;

    [Header("Sub Controllers")]
    public EnemyUIController      enemyUI;
    public HandUIController       handUI;
    public ScoreGaugeController   scoreGaugeUI;
    public PlayerStatusUIController playerStatusUI;

    [Header("Battle Init")]
    [Tooltip("拖入所有52张（或测试用）CardData asset")]
    public CardData[] allCards;

    private void Awake()
    {
        // 订阅事件（Awake 保证在所有 Start 之前完成）
        battleManager.OnStateChanged         += HandleStateChanged;
        battleManager.OnHandEvaluated        += HandleHandEvaluated;
        battleManager.OnScoreChanged         += scoreGaugeUI.UpdateScore;
        battleManager.OnThresholdTriggered   += scoreGaugeUI.HighlightThreshold;
        battleManager.OnPlayerHPChanged      += playerStatusUI.UpdateHP;
        battleManager.OnPlayerBlockChanged   += playerStatusUI.UpdateBlock;
        battleManager.OnSelectedCardsChanged += HandleSelectionChanged;

        // 按钮绑定
        playHandButton.onClick.AddListener(OnPlayHand);
        discardButton.onClick.AddListener(OnDiscard);

        SetButtonsInteractable(false, false);
    }

    private void Start()
    {
        // 所有 Awake 执行完毕后再启动战斗，确保所有事件已订阅
        battleManager.StartBattle(new System.Collections.Generic.List<CardData>(allCards));
    }

    private void OnDestroy()
    {
        if (battleManager == null) return;
        battleManager.OnStateChanged         -= HandleStateChanged;
        battleManager.OnHandEvaluated        -= HandleHandEvaluated;
        battleManager.OnScoreChanged         -= scoreGaugeUI.UpdateScore;
        battleManager.OnThresholdTriggered   -= scoreGaugeUI.HighlightThreshold;
        battleManager.OnPlayerHPChanged      -= playerStatusUI.UpdateHP;
        battleManager.OnPlayerBlockChanged   -= playerStatusUI.UpdateBlock;
        battleManager.OnSelectedCardsChanged -= HandleSelectionChanged;

        playHandButton.onClick.RemoveListener(OnPlayHand);
        discardButton.onClick.RemoveListener(OnDiscard);
    }

    // ── 状态切换 ──────────────────────────────────

    private void HandleStateChanged(BattleManager.BattleState state)
    {
        bool isPlayerTurn = state == BattleManager.BattleState.PlayerTurn;
        // 出牌按钮：玩家回合且有选中的牌时才启用（选中事件里额外判断）
        playHandButton.interactable = isPlayerTurn;
        discardButton.interactable  = isPlayerTurn;

        if (state == BattleManager.BattleState.Victory)
            ShowResult(true);
        else if (state == BattleManager.BattleState.Defeat)
            ShowResult(false);
    }

    // ── 选中牌变化 ────────────────────────────────

    private void HandleSelectionChanged(System.Collections.Generic.List<CardData> selected)
    {
        bool hasSelection = selected.Count > 0;
        bool isPlayerTurn = battleManager.State == BattleManager.BattleState.PlayerTurn;

        playHandButton.interactable = isPlayerTurn && hasSelection;
        discardButton.interactable  = isPlayerTurn && hasSelection;
    }

    // ── 出牌结果提示 ──────────────────────────────

    private void HandleHandEvaluated(HandResult result)
    {
        // 可在此播放牌型名称动画（目前仅 Log）
        Debug.Log($"[Battle] 牌型: {result.handType}  基础分: {result.baseScore}");
    }

    // ── 按钮回调 ──────────────────────────────────

    private void OnPlayHand()
    {
        battleManager.PlaySelectedCards();
        handUI.ClearSelection();
    }

    private void OnDiscard()
    {
        battleManager.DiscardAndRedraw();
        handUI.ClearSelection();
    }

    // ── 胜负 ──────────────────────────────────────

    private void ShowResult(bool victory)
    {
        SetButtonsInteractable(false, false);
        Debug.Log(victory ? "[Battle] 胜利！" : "[Battle] 失败！");
        // TODO: 显示胜负面板
    }

    private void SetButtonsInteractable(bool play, bool discard)
    {
        playHandButton.interactable = play;
        discardButton.interactable  = discard;
    }
}
