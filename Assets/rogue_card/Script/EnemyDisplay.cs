using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 敌人显示组件，挂在敌人 UI 对象上
/// </summary>
public class EnemyDisplay : MonoBehaviour
{
    public static EnemyDisplay Instance { get; private set; }

    [Header("敌人属性")]
    public string enemyName = "哥布林";
    public int    maxHp     = 100;
    public int    currentHp;

    [Header("UI 引用")]
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI hpText;
    public Slider          hpSlider;
    public TextMeshProUGUI intentText;  // 敌人意图提示（下回合行动）
    public Image           enemySprite; // 敌人立绘

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Start()
    {
        currentHp = maxHp;
        RefreshUI();
        SetIntent("攻击 12");  // 初始意图示例
    }

    public void TakeDamage(int damage)
    {
        currentHp = Mathf.Max(0, currentHp - damage);
        Debug.Log($"[Enemy] {enemyName} 受到 {damage} 伤害，剩余HP: {currentHp}/{maxHp}");

        RefreshUI();

        if (currentHp <= 0)
            OnEnemyDead();
    }

    public void SetIntent(string intentDescription)
    {
        if (intentText != null)
            intentText.text = $"意图: {intentDescription}";
    }

    private void RefreshUI()
    {
        if (nameText != null) nameText.text = enemyName;
        if (hpText   != null) hpText.text   = $"HP: {currentHp}/{maxHp}";
        if (hpSlider != null)
        {
            hpSlider.maxValue = maxHp;
            hpSlider.value    = currentHp;
        }
    }

    private void OnEnemyDead()
    {
        Debug.Log($"[Enemy] {enemyName} 已被击败！");
        // TODO: 触发战斗胜利逻辑、掉落奖励等
        CombatManager.Instance?.OnEnemyDefeated();
    }
}
