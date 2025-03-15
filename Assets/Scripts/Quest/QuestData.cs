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
    
    // Runtime state (not serialized)
    [System.NonSerialized] public bool isActive;
    [System.NonSerialized] public bool isCompleted;
}

[System.Serializable]
public class QuestObjective
{
    public string objectiveID;
    [TextArea] public string description;
    
    // Runtime state (not serialized)
    [System.NonSerialized] public bool isCompleted;
}

[System.Serializable]
public class QuestReward
{
    public QuestRewardType rewardType;
    public string rewardID; // Item ID, or amount for currency/XP
    public string description;
}