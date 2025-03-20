using System.Collections.Generic;
using UnityEngine;

public enum NPCType
{
    Villager,
    Merchant,
    QuestGiver,
    Enemy,
    Companion
}

[CreateAssetMenu(fileName = "New NPC", menuName = "Characters/NPC Data")]
public class NPCData : ScriptableObject
{
    [Header("基本信息")]
    public string npcID;
    public string npcName;
    [TextArea] public string description;
    public Sprite avatar;
    public NPCType npcType;
    
    [Header("对话信息")]
    public string dialogueID; // 对话ID，用于动态加载对话数据
    
    [Header("任务信息")]
    public List<string> availableQuestIDs = new List<string>(); // 该NPC可提供的任务ID列表
    
    [Header("商店信息")]
    public bool isMerchant;
    public List<string> soldItemIDs = new List<string>(); // 出售物品ID列表
    
    [Header("额外属性")]
    public NPCProperty[] properties; // 与ItemData类似的扩展属性
}

[System.Serializable]
public class NPCProperty
{
    public string key;
    public string value;
}