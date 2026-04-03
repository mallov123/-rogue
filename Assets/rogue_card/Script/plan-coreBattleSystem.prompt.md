# 计划：核心战斗界面与战斗逻辑

## 项目现状
- Unity 2D (URP), 几乎空白
- 只有：SampleScene（空），CardPrefab（UI框架，Card脚本引用但脚本未实现），NewEmptyCSharpScript.cs（占位）
- 无任何ScriptableObject、Manager、逻辑代码

---

## Phase 1: 数据层（ScriptableObjects）

**Step 1** - `CardData.cs` (ScriptableObject)
- Suit 枚举: Hearts/Diamonds/Clubs/Spades
- int value (1-13)
- Sprite artwork

**Step 2** - `EnemyData.cs` (ScriptableObject)
- string enemyName, int maxHP
- List<IntentData> (每回合意图列表)
- IntentData: 枚举ActionType(Attack/Block/Buff/Debuff) + int magnitude + Sprite icon

**Step 3** - `ThresholdData.cs` (ScriptableObject)
- float scoreThreshold (触发分数)
- ActionType actionType
- int effectValue (伤害/防御量)
- Sprite actionIcon (显示在阈值条上)
- BattleConfig.asset 包含 5个ThresholdData引用

---

## Phase 2: 卡牌与牌堆系统

**Step 4** - `Card.cs` (MonoBehaviour，更新现有引用)
- CardData data 引用
- 方法: Initialize(CardData), SetSelected(bool)
- UI子组件引用: artworkImage, valueText, suitText, selectionHighlight

**Step 5** - `DeckManager.cs` (MonoBehaviour)
- List<CardData> drawPile, handCards, discardPile
- 方法: InitDeck(), DrawCards(int n), DiscardSelected(), PlayHand()
- 事件: OnHandChanged, OnPileCountChanged

---

## Phase 3: 手牌评估器

**Step 6** - `HandResult.cs`
- 枚举 HandType: HighCard, OnePair, TwoPair, ThreeOfAKind, Straight, Flush, FullHouse, FourOfAKind, StraightFlush, RoyalFlush
- struct: HandType type, int baseScore, int totalScore

**Step 7** - `HandEvaluator.cs` (静态工具类)
- 基础分: HighCard=5, OnePair=10, TwoPair=20, ThreeOfAKind=30, Straight=40, Flush=50, FullHouse=60, FourOfAKind=80, StraightFlush=100, RoyalFlush=150
- static HandResult Evaluate(List<Card> playedCards)
- 支持1-5张牌的评估

---

## Phase 4: 战斗逻辑

**Step 8** - `Enemy.cs` (MonoBehaviour)
- int currentHP, maxHP
- List<StatusEffect> activeEffects
- int currentIntentIndex
- 方法: TakeDamage(int), AddBlock(int), ExecuteIntent(), GetCurrentIntent()

**Step 9** - `BattleManager.cs` (MonoBehaviour, 状态机)
- 状态枚举: DrawPhase / PlayerTurn / EnemyTurn / Victory / Defeat
- float currentScore, List<ThresholdData> thresholds
- 方法: StartBattle(), PlaySelectedCards(), DiscardAndRedraw(), EndPlayerTurn()
- 每回合流程:
  1. DrawPhase: DeckManager抽满手牌(7张)
  2. PlayerTurn: 等待玩家出牌/弃牌
  3. 出牌: 评估手牌→计算分数→触发通过的阈值行动→执行(攻击/防御)
  4. EnemyTurn: Enemy执行当前意图→切换下一意图
  5. 回到DrawPhase

---

## Phase 5: 战斗场景与UI布局

**Step 10** - 新建 BattleScene
- Canvas (Screen Space Overlay, 1920x1080参考分辨率)
- 上半部分 EnemyPanel (anchorMin y=0.5, anchorMax y=1)
- 下半部分 PlayerPanel (anchorMin y=0, anchorMax y=0.5)
  - LeftPanel: HP条、分数阈值条、抽牌堆
  - CenterPanel: 手牌区域 (Horizontal Layout Group)
  - RightPanel: 弃牌堆

**Step 11** - EnemyPanel子对象
- EnemySprite (Image)
- NameText (TextMeshPro)
- HPBar (Slider)
- StatusEffectArea (HorizontalLayoutGroup, 放状态图标)
- IntentDisplay: IntentIcon(Image) + IntentValueText

**Step 12** - ScoreGauge (阈值计量条)
- Background Slider (fill area)
- 5个ThresholdMarker子对象 (锚定在对应百分比位置)
- 每个marker: 图标+数值标签

**Step 13** - 更新CardPrefab
- 补全valueText(TextMeshPro), suitText(TextMeshPro), suitColorImage引用
- 添加Button组件用于点击选中

---

## Phase 6: UI控制器

**Step 14** - `BattleUIController.cs`
- 订阅BattleManager事件，协调所有子UI
- 管理PlayHand按钮、Discard按钮的启用状态

**Step 15** - `EnemyUIController.cs`
- 更新NameText, HPBar, StatusEffects, IntentDisplay

**Step 16** - `HandUIController.cs`
- 实例化/销毁手牌Card对象
- 跟踪选中的牌，高亮显示
- 连接到BattleManager.PlaySelectedCards / DiscardAndRedraw

**Step 17** - `ScoreGaugeController.cs`
- 动画填充进度条 (DOTween或Coroutine)
- 标记5个阈值位置
- 已触发阈值高亮显示

**Step 18** - `PlayerStatusUIController.cs`
- 更新HP条、抽牌堆数量、弃牌堆数量

---

## 关键文件路径
- `Assets/rogue_card/Script/Data/CardData.cs`
- `Assets/rogue_card/Script/Data/EnemyData.cs`
- `Assets/rogue_card/Script/Data/ThresholdData.cs`
- `Assets/rogue_card/Script/Card/Card.cs` (重写现有)
- `Assets/rogue_card/Script/Managers/DeckManager.cs`
- `Assets/rogue_card/Script/Managers/BattleManager.cs`
- `Assets/rogue_card/Script/Evaluation/HandEvaluator.cs`
- `Assets/rogue_card/Script/Evaluation/HandResult.cs`
- `Assets/rogue_card/Script/Enemy/Enemy.cs`
- `Assets/rogue_card/Script/UI/BattleUIController.cs`
- `Assets/rogue_card/Script/UI/EnemyUIController.cs`
- `Assets/rogue_card/Script/UI/HandUIController.cs`
- `Assets/rogue_card/Script/UI/ScoreGaugeController.cs`
- `Assets/rogue_card/Script/UI/PlayerStatusUIController.cs`
- `Assets/rogue_card/PreFab/CardPrefab.prefab` (更新)
- `Assets/rogue_card/Scenes/BattleScene.unity` (新建)

---

## 验证步骤
1. 在Edit Mode验证HandEvaluator单元逻辑(手动测试各种牌型)
2. Play Mode: 抽牌→弃牌→出牌→查看分数条填充
3. Play Mode: 确认阈值触发行动(攻击伤害/防御护甲)
4. Play Mode: 敌人回合执行意图，HP正确扣减
5. Play Mode: 胜负条件触发

---

## 决策记录
- 手牌上限: 7张
- 出牌张数: 1-5张均可（张数越少牌型越弱，如1张只能是High Card）
- 弃牌次数: 每回合最多2-3次（可通过BattleConfig配置），弃牌后重抽同等数量
- 分数阈值范围: 0-150, 5个阈值建议位于30/60/90/120/150
- 阈值触发: 单回合结算，每回合出牌后一次性计算分数，触发当次达到的所有阈值行动，下回合分数归零
