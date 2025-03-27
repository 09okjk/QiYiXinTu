# 游戏数据导入系统使用说明

本文档介绍了游戏中各种数据类型的导入系统。这些导入工具允许你通过CSV表格批量创建游戏资源，包括物品、NPC、对话和任务等。

## 目录

1. [通用功能](#通用功能)
2. [物品系统 (Item System)](#物品系统-item-system)
3. [NPC系统 (NPC System)](#npc系统-npc-system)
4. [对话系统 (Dialogue System)](#对话系统-dialogue-system)
5. [任务系统 (Quest System)](#任务系统-quest-system)

## 通用功能

所有导入器共享以下特性：

- 支持从CSV文件导入数据
- 自动创建并保存ScriptableObject资源
- 提供清晰的进度显示
- 处理引号中的逗号
- 详细的错误报告和日志

## 物品系统 (Item System)

### 数据结构

`ItemData` 包含以下属性：

| 属性 | 类型 | 描述 |
|------|------|------|
| itemID | string | 物品唯一标识符 |
| itemName | string | 物品名称 |
| description | string | 物品描述 |
| icon | Sprite | 物品图标 |
| itemType | ItemType | 物品类型枚举 |
| properties | ItemProperty[] | 额外自定义属性 |

### CSV格式要求

列顺序必须如下：

1. itemID - 物品ID
2. itemName - 物品名称
3. description - 物品描述
4. iconFileName - 图标文件名（不含扩展名）
5. itemType - 物品类型（数字索引）
6. 之后的列：按键值对形式添加额外属性（每两列为一组）

### 示例CSV

```
itemID,itemName,description,iconFileName,itemType,key1,value1,key2,value2
potion_health,"生命药水","恢复50点生命值",health_potion,1,healing,50,duration,0
sword_iron,"铁剑","普通的铁剑，伤害+10",iron_sword,0,damage,10,durability,100
key_rusty,"生锈的钥匙","看起来可以打开某个老旧的门",rusty_key,4,quest,dungeon_entry
```

### 使用方法

1. 在Unity编辑器中选择 `Tools > Inventory System > Import Items CSV`
2. 浏览并选择CSV文件
3. 确认图标文件夹路径
4. 点击"导入"按钮

## NPC系统 (NPC System)

### 数据结构

`NPCData` 包含以下属性：

| 属性 | 类型 | 描述 |
|------|------|------|
| npcID | string | NPC唯一标识符 |
| npcName | string | NPC名称 |
| description | string | NPC描述 |
| avatar | Sprite | NPC头像 |
| npcType | NPCType | NPC类型枚举 |
| dialogueID | string | 对话ID，用于加载对话数据 |
| availableQuestIDs | List<string> | NPC可提供的任务ID列表 |
| isMerchant | bool | 是否为商人 |
| soldItemIDs | List<string> | 出售物品ID列表 |
| properties | NPCProperty[] | 额外自定义属性 |

### CSV格式要求

列顺序必须如下：

1. npcID - NPC ID
2. npcName - NPC名称
3. description - NPC描述
4. avatarFileName - 头像文件名（不含扩展名）
5. npcType - NPC类型索引值
6. dialogueID - 对话ID
7. questIDs - 任务ID列表（用分号分隔）
8. isMerchant - 是否商人（1或0）
9. soldItemIDs - 出售物品ID列表（用分号分隔）
10. 之后的列：按键值对形式添加额外属性（每两列为一组）

### 示例CSV

```
npcID,npcName,description,avatarFileName,npcType,dialogueID,questIDs,isMerchant,soldItemIDs,key1,value1,key2,value2
merchant_sam,"商人山姆","镇上最好的武器商人",merchant_sam,1,merchant_sam_greeting,quest_sword;quest_shield,1,sword_iron;shield_wood;potion_health,faction,merchants,home,easttown
elder_lisa,"长老丽莎","村庄的长者，知识渊博",elder_lisa,2,elder_lisa_greeting,quest_herbs;quest_lore,0,,wisdom,90,age,78
guard_tom,"守卫汤姆","城门的守卫",guard_tom,0,guard_tom_greeting,,0,,faction,guards,strength,65
```

### 使用方法

1. 在Unity编辑器中选择 `Tools > Character System > Import NPCs CSV`
2. 浏览并选择CSV文件
3. 确认头像文件夹路径
4. 点击"导入"按钮

## 对话系统 (Dialogue System)

### 数据结构

`DialogueData` 包含对话节点列表，每个节点包含：

| 属性 | 类型 | 描述 |
|------|------|------|
| dialogueID | string | 对话唯一标识符 |
| nodes | List<DialogueNode> | 对话节点列表 |

每个 `DialogueNode` 包含：

| 属性 | 类型 | 描述 |
|------|------|------|
| text | string | 对话文本 |
| speakerID | string | 说话者ID |
| speakerName | string | 说话者名称 |
| nextNodeIndex | int | 下一个对话节点索引 |
| speakerPosition | string | 说话者位置（左或右） |
| choices | List<DialogueChoice> | 玩家选项列表 |

### CSV格式要求

列顺序必须如下：

1. dialogueTitle - 对话标题（同时用作dialogueID）
2. nodeID - 节点ID（必须是整数）
3. speakerID - 说话者ID
4. speakerName - 说话者名称
5. text - 对话文本
6. nextNodeIndex - 下一节点ID（-1表示结束）
7. speakerPosition - 说话者位置（'left'或'right'）
8. 之后的列：每两列为一组，分别是选项文本和目标节点ID

### 示例CSV

```
dialogueTitle,nodeID,speakerID,speakerName,text,nextNodeIndex,speakerPosition,choice1,targetNode1,choice2,targetNode2
merchant_sam_greeting,1,merchant_sam,商人山姆,"你好，旅行者！想看看我的商品吗？",2,left
merchant_sam_greeting,2,player,玩家,"我想看看你有什么。",-1,right,"是的，我想看看",3,"不了，谢谢",4
merchant_sam_greeting,3,merchant_sam,商人山姆,"这是我最好的商品，都是精挑细选的！",-1,left
merchant_sam_greeting,4,merchant_sam,商人山姆,"好吧，如果你改变主意随时回来。",-1,left
elder_lisa_greeting,1,elder_lisa,长老丽莎,"欢迎来到我们的村庄，年轻人。",2,left
elder_lisa_greeting,2,player,玩家,"谢谢您，长老。",-1,right,"我想了解这个村庄的历史",3,"我在寻找草药",4
elder_lisa_greeting,3,elder_lisa,长老丽莎,"这个村庄有着悠久的历史...",-1,left
elder_lisa_greeting,4,elder_lisa,长老丽莎,"草药？你应该去森林的南边看看。",-1,left
```

### 使用方法

1. 在Unity编辑器中选择 `Tools > Dialogue System > Import Dialogue CSV`
2. 浏览并选择CSV文件
3. 点击"导入"按钮

## 任务系统 (Quest System)

### 数据结构

`QuestData` 包含以下属性：

| 属性 | 类型 | 描述 |
|------|------|------|
| questID | string | 任务唯一标识符 |
| questName | string | 任务名称 |
| description | string | 任务描述 |
| objectives | List<QuestObjective> | 任务目标列表 |
| rewards | List<QuestReward> | 任务奖励列表 |

### CSV格式要求

列顺序必须如下：

1. questID - 任务ID
2. questName - 任务名称
3. description - 任务描述
4. objectiveID - 目标ID
5. objectiveDescription - 目标描述
6. rewardType - 奖励类型（整数: 0=物品, 1=经验, 2=金币）
7. rewardID - 奖励ID或数量
8. rewardDescription - 奖励描述

注意：多个目标或奖励应分成多行，保持相同的任务ID和名称。

### 示例CSV

```
questID,questName,description,objectiveID,objectiveDescription,rewardType,rewardID,rewardDescription
quest_herbs,"采集草药","为村医采集治疗所需的草药",obj_herbs_red,"采集5份红草药",0,potion_health,"获得生命药水"
quest_herbs,"采集草药","为村医采集治疗所需的草药",obj_herbs_blue,"采集3份蓝草药",1,100,"获得100点经验"
quest_herbs,"采集草药","为村医采集治疗所需的草药",obj_return,"返回村医处",2,50,"获得50金币"
quest_sword,"寻找失落之剑","找到传说中的失落之剑",obj_dungeon,"进入古代遗迹",0,map_dungeon,"获得地下城地图"
quest_sword,"寻找失落之剑","找到传说中的失落之剑",obj_defeat,"击败守卫者",1,200,"获得200点经验"
quest_sword,"寻找失落之剑","找到传说中的失落之剑",obj_sword,"找到失落之剑",0,sword_legendary,"获得传说之剑"
```

### 使用方法

1. 在Unity编辑器中选择 `Tools > Quest System > Import Quests CSV`
2. 浏览并选择CSV文件
3. 点击"导入"按钮

## 注意事项

1. 请确保CSV文件使用UTF-8编码，以正确显示中文字符
2. 文本中如包含逗号，请使用双引号包围整个文本
3. 导入前，确保图标、头像等资源已经放置在正确的文件夹中
4. 导入后，资源将保存在对应的ScriptableObjects文件夹下