using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class EnemyUIController : MonoBehaviour
{
    [Header("References")]
    public Enemy enemy;

    [Header("UI")]
    public Image          enemySprite;
    public TextMeshProUGUI nameText;
    public Slider         hpBar;
    public TextMeshProUGUI hpText;
    public Slider         blockBar;
    public TextMeshProUGUI blockText;

    [Header("Intent")]
    public Image          intentIcon;
    public TextMeshProUGUI intentValueText;

    [Header("Status Effects")]
    public Transform      statusEffectArea;   // HorizontalLayoutGroup 容器
    public GameObject     statusEffectPrefab; // 包含 Image + TextMeshProUGUI 的小图标预制体

    private readonly List<GameObject> _statusIcons = new();

    private void Start()
    {
        enemy.OnHPChanged      += UpdateHP;
        enemy.OnBlockChanged   += UpdateBlock;
        enemy.OnEffectsChanged += UpdateStatusEffects;
        enemy.OnIntentChanged  += UpdateIntent;

        // 初始化静态显示
        nameText.text = enemy.data.enemyName;
        if (enemy.data.enemySprite != null)
            enemySprite.sprite = enemy.data.enemySprite;

        hpBar.maxValue   = enemy.MaxHP;
        blockBar.maxValue = enemy.MaxHP;
    }

    private void OnDestroy()
    {
        if (enemy == null) return;
        enemy.OnHPChanged      -= UpdateHP;
        enemy.OnBlockChanged   -= UpdateBlock;
        enemy.OnEffectsChanged -= UpdateStatusEffects;
        enemy.OnIntentChanged  -= UpdateIntent;
    }

    // ── HP ───────────────────────────────────────

    private void UpdateHP(int current, int max)
    {
        hpBar.maxValue = max;
        hpBar.value    = current;
        hpText.text    = $"{current} / {max}";
    }

    // ── 护甲 ──────────────────────────────────────

    private void UpdateBlock(int block)
    {
        blockBar.value  = block;
        blockText.text  = block > 0 ? block.ToString() : "";
        blockBar.gameObject.SetActive(block > 0);
    }

    // ── 意图 ──────────────────────────────────────

    private void UpdateIntent(IntentData intent)
    {
        if (intent == null)
        {
            intentIcon.gameObject.SetActive(false);
            intentValueText.text = "";
            return;
        }

        intentIcon.gameObject.SetActive(true);

        if (intent.icon != null)
            intentIcon.sprite = intent.icon;

        intentValueText.text = intent.actionType switch
        {
            ActionType.Attack => $"攻击 {intent.magnitude}",
            ActionType.Block  => $"防御 {intent.magnitude}",
            ActionType.Buff   => "增益",
            ActionType.Debuff => "减益",
            _                 => ""
        };
    }

    // ── 状态效果 ──────────────────────────────────

    private void UpdateStatusEffects(List<StatusEffect> effects)
    {
        // 清除旧图标
        foreach (var icon in _statusIcons)
            Destroy(icon);
        _statusIcons.Clear();

        if (statusEffectPrefab == null || statusEffectArea == null) return;

        foreach (var effect in effects)
        {
            var go  = Instantiate(statusEffectPrefab, statusEffectArea);
            var img = go.GetComponentInChildren<Image>();
            var txt = go.GetComponentInChildren<TextMeshProUGUI>();

            if (txt != null)
                txt.text = effect.magnitude > 1 ? effect.magnitude.ToString() : "";

            // 可在此根据 effect.type 设置对应图标 sprite
            _statusIcons.Add(go);
        }
    }
}
