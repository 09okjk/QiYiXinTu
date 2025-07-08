using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Save;
using UnityEngine;
using TMPro;
using Utils;

[Serializable]
public class QuestGameData
{
    public string questID;
    public string questName;
    public QuestCondition questConditionType = QuestCondition.None; // 任务条件
    public string conditionValue; // 任务条件值
    [TextArea] public string questText;
    public string nextQuestID;
    public bool isCompleted;
}
public class QuestManager : MonoBehaviour
{
    public static QuestManager Instance { get; private set; }
    
    [SerializeField] private TextMeshProUGUI questText; // 任务文本(指引文本)
    
    // 运行时任务数据副本
    private Dictionary<string, QuestGameData> runtimeQuestDictionary = new();
    // 原始任务数据（只读）
    private QuestData[] originalQuestDataList;
    private List<QuestGameData> runtimeAllQuests = new();
    
    // 当前任务
    public QuestGameData currentQuest { get; private set; }
    // 当前任务ID
    public string currentQuestID => currentQuest?.questID;
    // 任务完成回调
    private Action<bool> onQuestCompleteCallback;
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            
            LoadQuestData();
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
        runtimeQuestDictionary.Clear();
    }
    
    /// <summary>
    /// 加载原始任务数据（只读）
    /// </summary>
    private void LoadQuestData()
    {
        try
        {
            SetAllQuest();
        }
        catch (Exception e)
        {
            Debug.LogError($"加载原始任务数据时发生错误: {e.Message}");
        }
    }

    private void SetAllQuest(Dictionary<string,QuestGameData> questGameDatas = null)
    {
        if (questGameDatas == null)
        {
            runtimeQuestDictionary.Clear();
            originalQuestDataList = Resources.LoadAll<QuestData>("ScriptableObjects/Quests");
            foreach (var questData in originalQuestDataList)
            {
                var questGameData = new QuestGameData
                {
                    questID = questData.questID,
                    questName = questData.questName,
                    questConditionType = questData.questConditionType,
                    conditionValue = questData.conditionValue,
                    questText = questData.questText,
                    nextQuestID = questData.nextQuestID,
                    isCompleted = questData.isCompleted
                };
                runtimeQuestDictionary[questGameData.questID] = questGameData;
            }

            Debug.Log($"成功加载 {originalQuestDataList?.Length ?? 0} 个原始任务数据");
        }
        else
        {
            runtimeQuestDictionary = new Dictionary<string, QuestGameData>(questGameDatas);
        }
    }

    /// <summary>
    /// 重置所有任务数据到原始状态
    /// </summary>
    public void ResetAllQuestData()
    {
        SetAllQuest();
        // 重置当前任务状态
        currentQuest = null;
        onQuestCompleteCallback = null;
        
        Debug.Log("已重置所有任务数据到原始状态");
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
        if (string.IsNullOrEmpty(questID))
        {
            Debug.LogError("任务ID为空");
            return;
        }
            
        // 使用运行时数据副本
        if (runtimeQuestDictionary.TryGetValue(questID, out var quest))
        {
            if (quest.isCompleted)
            {
                Debug.LogWarning($"任务已完成: {quest.questName}");
                return; // 任务已完成，不能重复开始
            }
            currentQuest = quest;
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

    // 设置所有任务
    public void SetAllQuests(Dictionary<string, QuestGameData> questGameDatas)
    {
        SetAllQuest(questGameDatas);
    }
    
    // 获取所有任务
    public Dictionary<string,QuestGameData> GetAllQuests()
    {
        return new Dictionary<string,QuestGameData>(runtimeQuestDictionary);
    }
    
    public QuestGameData GetQuest(string questID)
    {
        runtimeQuestDictionary.TryGetValue(questID, out var quest);
        return quest;
    }

    public void SetCurrentQuest(QuestGameData questGameData)
    {
        currentQuest = questGameData;
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