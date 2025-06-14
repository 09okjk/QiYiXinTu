using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Save;
using UnityEngine;
using TMPro;
using Utils;

public class QuestManager : MonoBehaviour
{
    public static QuestManager Instance { get; private set; }
    
    [SerializeField] private TextMeshProUGUI questText; // 任务文本(指引文本)
    
    // 原始任务数据（只读）
    private QuestData[] originalQuestDataList;
    // 运行时任务数据副本
    private Dictionary<string, QuestData> runtimeQuestDictionary = new Dictionary<string, QuestData>();
    private List<QuestData> runtimeAllQuests = new List<QuestData>();
    
    // 当前任务
    public QuestData currentQuest { get; private set; }
    // 当前任务ID
    public string currentQuestID { get; set; }
    // 任务完成回调
    private Action<bool> onQuestCompleteCallback;
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            
            LoadOriginalQuestData();
            CreateRuntimeDataCopies();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // 订阅各种可能导致任务完成的事件
        DialogueManager.Instance.OnDialogueEnd += OnConditionFinished;
        InventoryManager.Instance.OnAddItem += OnConditionFinished;
    }

    private void OnDestroy()
    {
        // 取消订阅事件
        if (DialogueManager.Instance != null)
            DialogueManager.Instance.OnDialogueEnd -= OnConditionFinished;
        if (InventoryManager.Instance != null)
            InventoryManager.Instance.OnAddItem -= OnConditionFinished;
        
        CleanupRuntimeData();
    }
    
    /// <summary>
    /// 加载原始任务数据（只读）
    /// </summary>
    private void LoadOriginalQuestData()
    {
        try
        {
            originalQuestDataList = Resources.LoadAll<QuestData>("ScriptableObjects/Quests");
            Debug.Log($"成功加载 {originalQuestDataList?.Length ?? 0} 个原始任务数据");
        }
        catch (Exception e)
        {
            Debug.LogError($"加载原始任务数据时发生错误: {e.Message}");
        }
    }

    /// <summary>
    /// 创建运行时数据副本
    /// </summary>
    private void CreateRuntimeDataCopies()
    {
        runtimeQuestDictionary.Clear();
        runtimeAllQuests.Clear();
    
        if (originalQuestDataList == null) return;

        foreach (var originalQuest in originalQuestDataList)
        {
            if (originalQuest != null && !string.IsNullOrEmpty(originalQuest.questID))
            {
                // 使用增强后的工具类
                var runtimeCopy = Utils.ScriptableObjectUtils.CreateQuestDataCopy(originalQuest);
                runtimeQuestDictionary[originalQuest.questID] = runtimeCopy;
                runtimeAllQuests.Add(runtimeCopy);
            }
        }
    
        Debug.Log($"创建了 {runtimeQuestDictionary.Count} 个任务运行时数据副本");
    }

    /// <summary>
    /// 重置所有任务数据到原始状态
    /// </summary>
    public void ResetAllQuestData()
    {
        foreach (var originalQuest in originalQuestDataList)
        {
            if (originalQuest != null && runtimeQuestDictionary.ContainsKey(originalQuest.questID))
            {
                var runtimeQuest = runtimeQuestDictionary[originalQuest.questID];
                ScriptableObjectUtils.ResetToOriginal(originalQuest, runtimeQuest);
            }
        }
        
        // 重置当前任务状态
        currentQuest = null;
        currentQuestID = null;
        onQuestCompleteCallback = null;
        
        Debug.Log("已重置所有任务数据到原始状态");
    }

    /// <summary>
    /// 清理运行时数据
    /// </summary>
    private void CleanupRuntimeData()
    {
        foreach (var runtimeQuest in runtimeQuestDictionary.Values)
        {
            if (runtimeQuest != null)
            {
                DestroyImmediate(runtimeQuest);
            }
        }
        runtimeQuestDictionary.Clear();
        runtimeAllQuests.Clear();
    }
    
    private void OnConditionFinished(string dialogueID)
    {
        if(CheckQuestCondition(dialogueID))
        {
            FinishQuest(currentQuestID);
        }
    }
    
    // 开始任务
    public void StartQuest(string questID, Action<bool> onComplete = null)
    {
        if (currentQuest)
        {
            currentQuest = null;
        }
        
        if (string.IsNullOrEmpty(questID))
        {
            Debug.LogError("任务ID为空");
            return;
        }
            
        // 使用运行时数据副本
        if (runtimeQuestDictionary.TryGetValue(questID, out var quest))
        {
            currentQuest = quest;
            currentQuestID = questID;
            ToggleQuestText(currentQuest.questText);
            onQuestCompleteCallback = onComplete;
            Debug.Log($"任务开始: {quest.questName}");
        }
        else
        {
            Debug.LogError($"任务不存在: {questID}");
        }
    }
    
    // 完成任务
    public void FinishQuest(string questID)
    {
        // 使用运行时数据副本
        if (runtimeQuestDictionary.TryGetValue(questID, out var quest))
        {
            quest.isCompleted = true; // 修改运行时副本，不影响原始资源
            string nextQuestID = quest.nextQuestID;
            currentQuest = null;
            currentQuestID = null;
            onQuestCompleteCallback?.Invoke(true);
            ToggleQuestText();
            
            // 自动接取下一个任务
            if (!string.IsNullOrEmpty(nextQuestID))
            {
                StartQuest(nextQuestID);
            }
            else
            {
                Debug.Log("没有下一个任务");
            }
            Debug.Log($"任务完成: {quest.questName}");
        }
        else
        {
            Debug.LogError($"任务不存在: {questID}");
        }
    }
    
    // 判断任务完成条件
    private bool CheckQuestCondition(string completedValue)
    {
        if (currentQuest == null) return false;
        
        switch (currentQuest.questConditionType)
        {
            case QuestCondition.None:
                return true;
            case QuestCondition.CompleteDialogue:
                return completedValue == currentQuest.conditionValue;
            case QuestCondition.HaveItem:
                return InventoryManager.Instance.HasItem(currentQuest.conditionValue);
            case QuestCondition.CompleteQuest:
                return IsQuestCompleted(currentQuest.conditionValue);
            default:
                return false;
        }
    }
    
    // 显示任务文本
    public void ToggleQuestText(string text = "")
    {
        if (questText != null)
        {
            questText.gameObject.SetActive(text != "");
            questText.text = text;
        }
    }
    
    // 加载所有任务数据
    public void LoadAllQuests(List<AsyncSaveLoadSystem.QuestSaveData> quests)
    {
        if (quests == null || quests.Count == 0)
        {
            Debug.LogWarning("没有任务数据可加载");
            return;
        }
        
        foreach (var questSaveData in quests)
        {
            // 查找对应的运行时副本并更新
            if (runtimeQuestDictionary.TryGetValue(questSaveData.questID, out var runtimeQuest))
            {
                runtimeQuest.questName = questSaveData.questName;
                runtimeQuest.questText = questSaveData.questText;
                runtimeQuest.isCompleted = questSaveData.isCompleted;
                runtimeQuest.conditionValue = questSaveData.conditionValue;
                runtimeQuest.questConditionType = questSaveData.questConditionType;
                runtimeQuest.nextQuestID = questSaveData.nextQuestID;
            }
        }
    }
    
    // 获取所有任务
    public List<QuestData> GetAllQuests()
    {
        return new List<QuestData>(runtimeAllQuests);
    }
    
    public QuestData GetQuest(string questID)
    {
        runtimeQuestDictionary.TryGetValue(questID, out var quest);
        return quest;
    }
    
    public bool IsQuestCompleted(string questID)
    {
        if (runtimeQuestDictionary.TryGetValue(questID, out var quest))
        {
            return quest.isCompleted;
        }
        return false;
    }
}