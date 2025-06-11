using System.Collections.Generic;
using Core;
using UnityEngine;

[CreateAssetMenu(fileName = "New NPC", menuName = "Characters/NPC Data")]
public class NPCData : EntityData
{
    [Header("基本信息")]
    public string npcID;
    public string npcName;
    public string spriteID;
    public string sceneName; // NPC所在场景名称
    
    [Header("行为设置")]
    // public bool isFollowing = false; // 是否跟随玩家
    // public bool canInteract = true; // 是否可以交互
    public float followSpeed = 2f; // 跟随速度
    public float followDistance = 1.5f; // 跟随距离
    public float interactionDistance = 2f; // 交互距离
    
    [Header("对话设置")]
    public List<string> dialogueIDs = new List<string>(); // 对话ID，用于动态加载对话数据
    
    [Header("特殊规则")]
    public List<NPCActivationRule> activationRules = new List<NPCActivationRule>(); // 激活规则
    
    [Header("额外属性")]
    public NPCProperty[] properties; // 扩展属性
}

/// <summary>
/// NPC激活规则
/// </summary>
[System.Serializable]
public class NPCActivationRule
{
    public string ruleName; // 规则名称
    public NPCActivationType activationType; // 激活类型
    public string conditionKey; // 条件键
    public string conditionValue; // 条件值
    public bool shouldActivate; // 满足条件时是否激活
}

/// <summary>
/// NPC激活类型
/// </summary>
public enum NPCActivationType
{
    Always, // 总是激活
    FirstEntry, // 首次进入场景
    GameStateFlag, // 游戏状态标志
    DialogueCompleted, // 对话完成
    ItemPossessed, // 拥有物品
    SceneName // 特定场景
}

[System.Serializable]
public class NPCProperty
{
    public string key;
    public string value;
}