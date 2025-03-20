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

    // 所有任务数据
    private List<QuestData> allQuests = new List<QuestData>();

    // 活跃任务
    private List<QuestData> activeQuests = new List<QuestData>();

    // 完成的任务
    private List<QuestData> completedQuests = new List<QuestData>();
    
    // 完成的任务目标
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
        LoadQuests();
        UpdateQuestLog();
    }
    
    public void ToggleQuestLog()
    {
        questLogPanel.SetActive(!questLogPanel.activeSelf);
    }

    /// <summary>
    /// 加载所有任务
    /// </summary>
    private void LoadQuests()
    {
        // 从资源中加载任务
        QuestData[] questsFromResources = Resources.LoadAll<QuestData>("Quests");
        allQuests.AddRange(questsFromResources); // 添加到列表中
    }

    /// <summary>
    /// 开始任务
    /// </summary>
    /// <param name="questID">任务ID</param>
    public void StartQuest(string questID)
    {
        QuestData quest = allQuests.Find(q => q.questID == questID); // 从所有任务中查找任务

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

        // 激活任务
        quest.isActive = true;
        activeQuests.Add(quest);

        // 通知玩家
        ShowQuestNotification($"新任务: {quest.questName}");

        // 更新任务日志的UI
        UpdateQuestLog();
    }

    // 任务是否激活
    public bool IsQuestActive(string questID)
    {
        return activeQuests.Exists(q => q.questID == questID);
    }
    // 任务是否完成
    public bool IsQuestCompleted(string questID)
    {
        return completedQuests.Exists(q => q.questID == questID);
    }
    // 获取激活的任务ID
    public List<string> GetActiveQuestIDs()
    {
        return activeQuests.Select(q => q.questID).ToList(); // 返回列表的副本
    }
    // 获取完成的任务ID
    public List<string> GetCompletedQuestIDs()
    {
        return completedQuests.Select(q => q.questID).ToList(); // 返回列表的副本
    }
    // 重新设置任务
    public void ResetQuests()
    {
        activeQuests.Clear();
        completedQuests.Clear();
        completedObjectives.Clear();
    }
    // 获取所有完成的任务目标
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

    // 完成任务
    public void CompleteQuest(string questID)
    {
        QuestData quest = activeQuests.Find(q => q.questID == questID);

        if (quest == null)
        {
            Debug.LogError($"Active quest with ID {questID} not found!");
            return;
        }

        // 从活跃任务移动到完成任务
        activeQuests.Remove(quest);
        quest.isCompleted = true;
        completedQuests.Add(quest);

        // 通知玩家
        ShowQuestNotification($"任务完成: {quest.questName}");

        // 授予奖励
        GiveQuestRewards(quest);

        // 更新任务日志的UI
        UpdateQuestLog();
    }

    /// <summary>
    /// 更新任务目标
    /// </summary>
    /// <param name="questID">任务ID</param>
    /// <param name="objectiveID">目标ID</param>
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

        // 更新目标进度
        objective.isCompleted = true;

        // 检查是否所有目标都已完成
        bool allObjectivesCompleted = true;
        foreach (QuestObjective obj in quest.objectives)
        {
            if (!obj.isCompleted)
            {
                allObjectivesCompleted = false;
                break;
            }
        }

        // 如果所有目标都完成了，完成任务
        if (allObjectivesCompleted)
        {
            CompleteQuest(questID);
        }
        else
        {
            // 通知玩家目标完成
            ShowQuestNotification($"任务进度: {objective.description}");

            // 更新任务日志UI
            UpdateQuestLog();
        }
    }

    /// <summary>
    /// 提供任务奖励
    /// </summary>
    /// <param name="quest">任务</param>
    private void GiveQuestRewards(QuestData quest)
    {
        // Give XP, items, etc. // 根据奖励类型给予奖励 比如物品、经验、金币等
        foreach (QuestReward reward in quest.rewards)
        {
            switch (reward.rewardType)
            {
                case QuestRewardType.Item:
                    ItemData item = ItemDatabase.Instance.GetItem(reward.rewardID);
                    if (item != null)
                    {
                        InventoryManager.Instance.AddItem(item);
                    }
                    break;

                case QuestRewardType.Experience:
                    // 如果有经验系统，给玩家经验
                    int xpAmount = int.Parse(reward.rewardID);
                    // PlayerExperience.Instance.AddXP(xpAmount);
                    break;

                case QuestRewardType.Gold:
                    // 如果有货币系统，给玩家金币
                    int goldAmount = int.Parse(reward.rewardID);
                    // PlayerCurrency.Instance.AddGold(goldAmount);
                    break;
            }
        }
    }

    /// <summary>
    /// 更新任务日志UI
    /// </summary>
    private void UpdateQuestLog()
    {
        // 清除现有条目
        foreach (Transform child in questLogContainer)
        {
            Destroy(child.gameObject);
        }

        // 添加活跃任务(当前任务)到任务日志
        foreach (QuestData quest in activeQuests)
        {
            GameObject entryGO = Instantiate(questLogEntryPrefab, questLogContainer); // 实例化任务日志条目
            QuestLogEntry entry = entryGO.GetComponent<QuestLogEntry>(); // 获取任务日志条目组件
            entry.SetQuest(quest); // 设置任务
        }
    }

    /// <summary>
    /// 展示任务通知
    /// </summary>
    /// <param name="message">消息内容</param>
    private void ShowQuestNotification(string message)
    {
        GameObject notificationGO = Instantiate(questNotificationPrefab, questNotificationContainer);// 实例化任务通知
        TextMeshProUGUI notificationText = notificationGO.GetComponentInChildren<TextMeshProUGUI>();// 获取任务通知文本
        notificationText.text = message;

        // 一段时间后销毁
        Destroy(notificationGO, notificationDuration);
    }
}