using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro;

public class QuestManager : MonoBehaviour
{
    public static QuestManager Instance { get; private set; }

    [SerializeField] private GameObject questLogPanel;
    [SerializeField] private Transform questLogContainer;
    [SerializeField] private GameObject questLogEntryPrefab;
    [SerializeField] private GameObject questNotificationPrefab;
    [SerializeField] private Transform questNotificationContainer;
    [SerializeField] private float notificationDuration = 5f;

    // All quests data
    private List<QuestData> allQuests = new List<QuestData>();

    // Active quests
    private List<QuestData> activeQuests = new List<QuestData>();

    // Completed quests
    private List<QuestData> completedQuests = new List<QuestData>();

    private Dictionary<string, List<string>> completedObjectives = new Dictionary<string, List<string>>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // Load all quests
        LoadQuests();
        UpdateQuestLog();
    }

    private void LoadQuests()
    {
        // Load quests from resources
        QuestData[] questsFromResources = Resources.LoadAll<QuestData>("Quests");
        allQuests.AddRange(questsFromResources);
    }

    public void StartQuest(string questID)
    {
        QuestData quest = allQuests.Find(q => q.questID == questID);

        if (quest == null)
        {
            Debug.LogError($"Quest with ID {questID} not found!");
            return;
        }

        if (IsQuestActive(questID) || IsQuestCompleted(questID))
        {
            Debug.Log($"Quest {questID} is already active or completed.");
            return;
        }

        // Activate quest
        quest.isActive = true;
        activeQuests.Add(quest);

        // Notify player
        ShowQuestNotification($"新任务: {quest.questName}");

        // Update UI
        UpdateQuestLog();
    }

    public bool IsQuestActive(string questID)
    {
        return activeQuests.Exists(q => q.questID == questID);
    }

    public bool IsQuestCompleted(string questID)
    {
        return completedQuests.Exists(q => q.questID == questID);
    }

    public List<string> GetActiveQuestIDs()
    {
        return activeQuests.Select(q => q.questID).ToList(); // 返回列表的副本
    }

    public List<string> GetCompletedQuestIDs()
    {
        return completedQuests.Select(q => q.questID).ToList(); // 返回列表的副本
    }
    
    public void ResetQuests()
    {
        activeQuests.Clear();
        completedQuests.Clear();
        completedObjectives.Clear();
    }
    public Dictionary<string, List<string>> GetAllCompletedObjectives() 
    {
        // 创建一个深拷贝
        Dictionary<string, List<string>> result = new Dictionary<string, List<string>>();
        foreach (var kvp in completedObjectives) 
        {
            result[kvp.Key] = new List<string>(kvp.Value);
        }
        return result;
    }

    public void CompleteQuest(string questID)
    {
        QuestData quest = activeQuests.Find(q => q.questID == questID);

        if (quest == null)
        {
            Debug.LogError($"Active quest with ID {questID} not found!");
            return;
        }

        // Move from active to completed
        activeQuests.Remove(quest);
        quest.isCompleted = true;
        completedQuests.Add(quest);

        // Notify player
        ShowQuestNotification($"任务完成: {quest.questName}");

        // Grant rewards
        GiveQuestRewards(quest);

        // Update UI
        UpdateQuestLog();
    }

    public void UpdateQuestObjective(string questID, string objectiveID)
    {
        QuestData quest = activeQuests.Find(q => q.questID == questID);

        if (quest == null)
        {
            Debug.LogError($"Active quest with ID {questID} not found!");
            return;
        }

        QuestObjective objective = quest.objectives.Find(o => o.objectiveID == objectiveID);

        if (objective == null)
        {
            Debug.LogError($"Objective with ID {objectiveID} not found in quest {questID}!");
            return;
        }

        // Update objective progress
        objective.isCompleted = true;

        // Check if all objectives are completed
        bool allObjectivesCompleted = true;
        foreach (QuestObjective obj in quest.objectives)
        {
            if (!obj.isCompleted)
            {
                allObjectivesCompleted = false;
                break;
            }
        }

        // If all objectives are completed, complete the quest
        if (allObjectivesCompleted)
        {
            CompleteQuest(questID);
        }
        else
        {
            // Notify player of objective completion
            ShowQuestNotification($"任务进度: {objective.description}");

            // Update UI
            UpdateQuestLog();
        }
    }

    private void GiveQuestRewards(QuestData quest)
    {
        // Give XP, items, etc.
        foreach (QuestReward reward in quest.rewards)
        {
            switch (reward.rewardType)
            {
                case QuestRewardType.Item:
                    ItemData item = Resources.Load<ItemData>($"Items/{reward.rewardID}");
                    if (item != null)
                    {
                        InventoryManager.Instance.AddItem(item);
                    }
                    break;

                case QuestRewardType.Experience:
                    // Add XP to player if you have an experience system
                    int xpAmount = int.Parse(reward.rewardID);
                    // PlayerExperience.Instance.AddXP(xpAmount);
                    break;

                case QuestRewardType.Gold:
                    // Add gold to player if you have a currency system
                    int goldAmount = int.Parse(reward.rewardID);
                    // PlayerCurrency.Instance.AddGold(goldAmount);
                    break;
            }
        }
    }

    private void UpdateQuestLog()
    {
        // Clear existing entries
        foreach (Transform child in questLogContainer)
        {
            Destroy(child.gameObject);
        }

        // Add active quests
        foreach (QuestData quest in activeQuests)
        {
            GameObject entryGO = Instantiate(questLogEntryPrefab, questLogContainer);
            QuestLogEntry entry = entryGO.GetComponent<QuestLogEntry>();
            entry.SetQuest(quest);
        }
    }

    private void ShowQuestNotification(string message)
    {
        GameObject notificationGO = Instantiate(questNotificationPrefab, questNotificationContainer);
        TextMeshProUGUI notificationText = notificationGO.GetComponentInChildren<TextMeshProUGUI>();
        notificationText.text = message;

        // Destroy after duration
        Destroy(notificationGO, notificationDuration);
    }
}