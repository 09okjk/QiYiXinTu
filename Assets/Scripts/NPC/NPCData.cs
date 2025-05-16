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
    
    [Header("对话ID列表")]
    public List<string> dialogueIDs; // 对话ID，用于动态加载对话数据
    
    [Header("额外属性")]
    public NPCProperty[] properties; // 与ItemData类似的扩展属性
}

[System.Serializable]
public class NPCProperty
{
    public string key;
    public string value;
}