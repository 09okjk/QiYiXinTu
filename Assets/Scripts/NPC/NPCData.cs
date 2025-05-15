using System.Collections.Generic;
using Core;
using UnityEngine;

public enum NpcEmotion
{
    Neutral, // 中立
    Happy, // 高兴
    Sad, // 伤心
    Angry, // 生气
    Surprised, // 惊讶
    Scared // 害怕
}

[CreateAssetMenu(fileName = "New NPC", menuName = "Characters/NPC Data")]
public class NPCData : EntityData
{
    [Header("基本信息")]
    public string npcID;
    public string npcName;
    public string spriteID;
    public NpcEmotion npcEmotion = NpcEmotion.Neutral; // NPC情绪状态
    
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