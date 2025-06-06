﻿using System;
using System.Collections.Generic;
using Dialogue;
using UnityEngine;

[CreateAssetMenu(fileName = "New Dialogue", menuName = "Dialogue/Dialogue Data")]
public class DialogueData : ScriptableObject
{
    public string dialogueID;
    public DialogueState state = DialogueState.WithOutStart; // 新增：对话状态
    public string currentNodeID;
    public List<DialogueNode> nodes = new List<DialogueNode>();
}

[Serializable]
public class DialogueNode
{
    public string nodeID;
    public string text;
    public DialogueSpeaker speaker;
    public string nextNodeID; // 新增：下一个对话节点的索引
    public List<DialogueChoice> choices = new List<DialogueChoice>();
    public string questID ; // 新增：任务ID
    public List<string> rewardIDs = new List<string>(); // 新增：奖励ID列表
    public bool isFollow; // 新增：是否跟随
    public DialogueConditionType conditionType = DialogueConditionType.None; // 新增：对话条件类型
    public string conditionValue; // 新增：对话条件值
}

[Serializable]
public class DialogueChoice
{
    public string text;
    public string nextNodeID;
}

[Serializable]
public class DialogueSpeaker
{
    public string speakerID;
    public string speakerName ; // 新增：说话者名称
    public SpeakerType speakerType = SpeakerType.Npc;
    public Emotion emotion = Emotion.Neutral; // 新增：说话者情绪
}

[Serializable]
public enum DialogueState
{
    Finished, // 0 对话结束
    Ongoing, // 1 对话进行中
    WithOutStart // 2 对话未开始
}

[Serializable]
public enum SpeakerType
{
    Player, // 0 玩家
    Npc, // 1 非玩家角色
    System, // 2 系统
    PlayerChoice, // 3 玩家选择
    NpcNotice // 4 NPC通知
}

// 对话条件类型
[Serializable]
public enum DialogueConditionType
{
    None, // 0 无条件
    QuestCompleted, // 1 任务完成
    ItemAcquired, // 2 获得物品
    SceneName, // 3 场景名称
    NpcCheck, // 4 NPC检查
    DialogueCompleted, // 5 对话完成
    EnemyCleared, // 6 敌人清除
}