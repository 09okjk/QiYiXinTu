# Importer 类使用指南与 CSV 格式样例

基于您提到的 DialogueImporter、ItemImporter 和 QuestImporter 类，以下是每个类所需的 CSV 文件格式样例和使用方法。

## 1. DialogueImporter

### CSV 格式样例

```
ID,NodeID,Speaker,Text,NextNodeID,ChoiceText1,ChoiceNextNodeID1,ChoiceText2,ChoiceNextNodeID2,ChoiceText3,ChoiceNextNodeID3
dialogue001,1,村长,欢迎来到我们村庄，冒险者。,2,,,,,,
dialogue001,2,村长,我们村子最近遇到了一些麻烦。,3,,,,,,
dialogue001,3,村长,你能帮助我们吗？,-1,我愿意帮忙,4,我需要报酬,5,我现在没空,6
dialogue001,4,村长,太感谢了！请去森林调查那些奇怪的声音。,-1,,,,,,
dialogue001,5,村长,当然，完成任务后我会给你10枚金币。,-1,,,,,,
dialogue001,6,村长,我理解，如果你改变主意请再来找我。,-1,,,,,,
dialogue002,1,商人,看看我的商品吧，价格公道！,-1,我想看看武器,2,我想看看药水,3,现在不需要,4
dialogue002,2,商人,这把剑只要50金币，非常锋利！,-1,,,,,,
```

### 格式说明

- **ID**: 对话的唯一标识符，用于区分不同对话
- **NodeID**: 对话节点的ID，每个对话中的节点编号
- **Speaker**: 说话者的名字
- **Text**: 对话文本内容
- **NextNodeID**: 下一个对话节点的ID，-1表示对话结束或需要选择
- **ChoiceText1-3**: 选项文本（如果有）
- **ChoiceNextNodeID1-3**: 选择对应选项后跳转到的节点ID

### 使用方法

```csharp
// 导入对话数据
void ImportDialogues()
{
    // 指定CSV文件路径
    string filePath = "Assets/Resources/Data/dialogues.csv";
    
    // 使用导入器导入对话数据
    List<DialogueData> dialogues = DialogueImporter.ImportDialogues(filePath);
    
    // 使用导入的对话数据
    foreach (var dialogue in dialogues)
    {
        Debug.Log($"已导入对话: {dialogue.ID} 共有 {dialogue.Nodes.Count} 个节点");
        
        // 存储到游戏数据管理器或使用ScriptableObject保存
        // GameManager.Instance.AddDialogue(dialogue);
    }
}
```

## 2. ItemImporter

### CSV 格式样例

```
ItemID,ItemName,Description,IconPath,ItemType,Value,CanBeUsed,UseEffect
potion_health,生命药水,恢复30点生命值,Icons/Potions/health_potion,Consumable,20,TRUE,RestoreHealth:30
potion_mana,魔法药水,恢复25点魔法值,Icons/Potions/mana_potion,Consumable,25,TRUE,RestoreMana:25
key_dungeon,地牢钥匙,打开地牢大门的钥匙,Icons/Keys/dungeon_key,Key,0,TRUE,OpenDoor:dungeon_gate
sword_iron,铁剑,普通的铁制长剑,Icons/Weapons/iron_sword,Weapon,75,FALSE,Attack:15
map_forest,森林地图,显示森林区域的详细地图,Icons/Quest/forest_map,QuestItem,10,TRUE,RevealMap:forest
```

### 格式说明

- **ItemID**: 物品的唯一标识符
- **ItemName**: 物品名称
- **Description**: 物品描述
- **IconPath**: 物品图标在Resources文件夹中的路径
- **ItemType**: 物品类型（Consumable消耗品、Weapon武器、Armor护甲、QuestItem任务物品、Key钥匙等）
- **Value**: 物品价值/售价
- **CanBeUsed**: 是否可以使用（TRUE/FALSE）
- **UseEffect**: 使用效果（效果类型:数值）

### 使用方法

```csharp
// 导入物品数据
void ImportItems()
{
    // 指定CSV文件路径
    string filePath = "Assets/Resources/Data/items.csv";
    
    // 使用导入器导入物品数据
    List<ItemData> items = ItemImporter.ImportItems(filePath);
    
    // 使用导入的物品数据
    foreach (var item in items)
    {
        Debug.Log($"已导入物品: {item.itemName} (ID: {item.itemID})");
        
        // 将物品数据添加到物品管理器
        // ItemDatabase.Instance.AddItem(item);
    }
}
```

## 3. QuestImporter

### CSV 格式样例

```
QuestID,Title,Description,IsStoryQuest,PreviousQuestID,ExperienceReward,GoldReward,ItemRewards,ObjectiveID,ObjectiveDesc,ObjectiveType,ObjectiveTarget,ObjectiveAmount,ObjectiveProgress
quest001,寻找草药,为村医寻找治疗瘟疫的草药,TRUE,,100,50,potion_health:2,obj001,收集红色草药,Collect,herb_red,5,0
quest001,,,,,,,,obj002,收集蓝色草药,Collect,herb_blue,3,0
quest001,,,,,,,,obj003,将草药交给村医,TalkTo,village_doctor,1,0
quest002,消灭狼群,解决威胁村庄的狼群,FALSE,quest001,150,80,sword_iron:1,obj001,消灭狼,Kill,wolf,10,0
quest002,,,,,,,,obj002,消灭头狼,Kill,wolf_alpha,1,0
quest002,,,,,,,,obj003,向村长报告,TalkTo,village_elder,1,0
quest003,探索古墓,找出古墓中的宝藏,FALSE,,200,100,map_forest:1|key_dungeon:1,obj001,找到古墓入口,Discover,ancient_tomb,1,0
quest003,,,,,,,,obj002,解开石碑谜题,Interact,tomb_puzzle,1,0
quest003,,,,,,,,obj003,找到宝藏,Collect,ancient_treasure,1,0
```

### 格式说明

- **QuestID**: 任务的唯一标识符
- **Title**: 任务标题（只在任务的第一行填写）
- **Description**: 任务描述（只在任务的第一行填写）
- **IsStoryQuest**: 是否为主线任务（TRUE/FALSE）
- **PreviousQuestID**: 前置任务ID（如果有）
- **ExperienceReward**: 经验奖励
- **GoldReward**: 金币奖励
- **ItemRewards**: 物品奖励（格式：物品ID:数量|物品ID:数量）
- **ObjectiveID**: 目标的唯一标识符
- **ObjectiveDesc**: 目标描述
- **ObjectiveType**: 目标类型（Collect收集、Kill击杀、TalkTo对话、Discover发现、Interact交互等）
- **ObjectiveTarget**: 目标对象的ID
- **ObjectiveAmount**: 目标需要完成的数量
- **ObjectiveProgress**: 当前进度（默认为0）

### 使用方法

```csharp
// 导入任务数据
void ImportQuests()
{
    // 指定CSV文件路径
    string filePath = "Assets/Resources/Data/quests.csv";
    
    // 使用导入器导入任务数据
    List<QuestData> quests = QuestImporter.ImportQuests(filePath);
    
    // 使用导入的任务数据
    foreach (var quest in quests)
    {
        Debug.Log($"已导入任务: {quest.Title} (ID: {quest.QuestID})，目标数量: {quest.Objectives.Count}");
        
        // 将任务数据添加到任务管理器
        // QuestManager.Instance.RegisterQuest(quest);
    }
}
```

## 使用建议

1. **文件放置位置**：
    - 将CSV文件放在项目的`Assets/Resources/Data/`目录下
    - 也可以放在其他目录，但需要在导入时指定正确的路径

2. **编辑CSV文件**：
    - 使用Excel或Google表格创建和编辑CSV文件
    - 保存时选择UTF-8编码以支持中文和特殊字符

3. **导入时机**：
    - 在游戏初始化时导入数据（如GameManager的Start或Awake方法中）
    - 编辑器模式下也可以创建自定义编辑器工具来导入和预览数据

4. **数据验证**：
    - 建议在导入后对数据进行验证，确保没有ID冲突或必填字段缺失

5. **缓存导入的数据**：
    - 导入后将数据缓存到管理器中，避免重复导入
    - 可以使用ScriptableObject将导入的数据保存为资源文件

通过这些Importer类，您可以实现游戏数据的外部化管理，便于非程序员团队成员（如策划和设计师）编辑和维护游戏内容，而不需要直接修改代码。