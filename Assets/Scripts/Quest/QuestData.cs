using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum QuestRewardType
{
    Item,
    Experience,
    Gold
}

[CreateAssetMenu(fileName = "New Quest", menuName = "Quests/Quest Data")]
public class QuestData : ScriptableObject
{
    public string questID;
    public string questName;
    [TextArea] public string description;
    public List<QuestObjective> objectives = new List<QuestObjective>();
    public List<QuestReward> rewards = new List<QuestReward>();
    
    // 运行时状态（不序列化）
    [System.NonSerialized] public bool isActive;
    [System.NonSerialized] public bool isCompleted;
}

[System.Serializable]
public class QuestObjective
{
    public string objectiveID;
    [TextArea] public string description;
    
    // 运行时状态（不序列化）
    [System.NonSerialized] public bool isCompleted;
}

[System.Serializable]
public class QuestReward
{
    public QuestRewardType rewardType;
    public string rewardID; // 物品ID，或者货币/经验的数量
    public string description;
}