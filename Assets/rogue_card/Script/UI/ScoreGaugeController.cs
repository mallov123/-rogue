using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 控制分数阈值计量条：
/// - 动画平滑填充 Slider
/// - 在对应百分比位置显示 5 个阈值标记
/// - 触发阈值时高亮对应标记
/// </summary>
public class ScoreGaugeController : MonoBehaviour
{
    [Header("Score Bar")]
    public Slider scoreSlider;
    [Tooltip("计量条满分（对应最高阈值）")]
    public float maxScore = 150f;

    [Header("Threshold Markers")]
    [Tooltip("按顺序对应5个阈值的标记 UI 对象（Image+TextMeshProUGUI子对象）")]
    public List<GameObject> thresholdMarkers;

    [Tooltip("对应 BattleConfig 中的 thresholds 列表（顺序一致）")]
    public BattleConfig battleConfig;

    [Header("Colors")]
    public Color normalColor    = Color.white;
    public Color triggeredColor = new Color(1f, 0.85f, 0f); // 金色

    private float  _displayScore;
    private float  _targetScore;
    private Coroutine _animRoutine;

    private void Start()
    {
        scoreSlider.minValue = 0;
        scoreSlider.maxValue = 1; // 归一化
        scoreSlider.value    = 0;

        InitMarkers();
    }

    // ── 初始化标记位置 ────────────────────────────

    private void InitMarkers()
    {
        if (battleConfig == null || thresholdMarkers == null) return;

        var sorted = battleConfig.thresholds;
        for (int i = 0; i < thresholdMarkers.Count; i++)
        {
            if (i >= sorted.Count || thresholdMarkers[i] == null) continue;

            float pct = Mathf.Clamp01(sorted[i].scoreThreshold / maxScore);

            // 水平锚定在进度条对应百分比位置
            var rt = thresholdMarkers[i].GetComponent<RectTransform>();
            if (rt != null)
            {
                rt.anchorMin = new Vector2(pct, rt.anchorMin.y);
                rt.anchorMax = new Vector2(pct, rt.anchorMax.y);
                rt.anchoredPosition = new Vector2(0, rt.anchoredPosition.y);
            }

            // 显示阈值数值
            var label = thresholdMarkers[i].GetComponentInChildren<TextMeshProUGUI>();
            if (label != null)
                label.text = sorted[i].scoreThreshold.ToString("0");

            // 显示阈值图标
            if (sorted[i].actionIcon != null)
            {
                var img = thresholdMarkers[i].GetComponentInChildren<Image>();
                if (img != null)
                    img.sprite = sorted[i].actionIcon;
            }

            ResetMarkerColor(i);
        }
    }

    // ── 分数更新（由 BattleManager.OnScoreChanged 调用）──

    public void UpdateScore(float score)
    {
        _targetScore = score;

        if (_animRoutine != null)
            StopCoroutine(_animRoutine);
        _animRoutine = StartCoroutine(AnimateScore());

        // 重置所有标记颜色（每局出牌后分数归零前调用）
        if (score <= 0)
            ResetAllMarkers();
    }

    // ── 阈值高亮（由 BattleManager.OnThresholdTriggered 调用）──

    public void HighlightThreshold(ThresholdData threshold)
    {
        if (battleConfig == null) return;
        int idx = battleConfig.thresholds.IndexOf(threshold);
        if (idx < 0 || idx >= thresholdMarkers.Count) return;

        var img = thresholdMarkers[idx].GetComponentInChildren<Image>();
        if (img != null) img.color = triggeredColor;
    }

    // ── 动画 ──────────────────────────────────────

    private IEnumerator AnimateScore()
    {
        float duration = 0.4f;
        float elapsed  = 0f;
        float start    = _displayScore;

        while (elapsed < duration)
        {
            elapsed      += Time.deltaTime;
            _displayScore = Mathf.Lerp(start, _targetScore, elapsed / duration);
            scoreSlider.value = _displayScore / maxScore;
            yield return null;
        }

        _displayScore     = _targetScore;
        scoreSlider.value = _displayScore / maxScore;
    }

    // ── 工具 ──────────────────────────────────────

    private void ResetAllMarkers()
    {
        for (int i = 0; i < thresholdMarkers.Count; i++)
            ResetMarkerColor(i);
    }

    private void ResetMarkerColor(int index)
    {
        if (thresholdMarkers[index] == null) return;
        var img = thresholdMarkers[index].GetComponentInChildren<Image>();
        if (img != null) img.color = normalColor;
    }
}
