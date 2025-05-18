using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Quest", menuName = "Quests/Quest Data")]
public class QuestData : ScriptableObject
{
    public string questID;
    public string questName;
    public QuestCondition questConditionType = QuestCondition.None; // 任务条件
    public string conditionValue; // 任务条件值
    [TextArea] public string questText;
    public string nextQuestID;
    public bool isCompleted;
}

[Serializable]
// 任务条件
public enum QuestCondition
{
    None, //没有条件
    CompleteDialogue, //完成对话
    HaveItem, //拥有物品
    CompleteQuest //完成任务
}
