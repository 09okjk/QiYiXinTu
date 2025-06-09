using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Save;
using UnityEngine;
using TMPro;

public class QuestManager : MonoBehaviour
{
    public static QuestManager Instance { get; private set; }
    
    [SerializeField] private TextMeshProUGUI questText; // 任务文本(指引文本)
    [SerializeField] private List<QuestData> allQuests = new List<QuestData>();
    private readonly Dictionary<string, QuestData> questDictionary = new Dictionary<string, QuestData>();
    
    // 当前任务
    public QuestData currentQuest { get; private set; }
    // 当前任务ID
    public string currentQuestID { get; private set; }
    // 任务完成回调
    private Action<bool> onQuestCompleteCallback;
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            
            InitializeQuest();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // 订阅各种可能导致任务完成的事件
        // 例如：对话完成、物品使用、任务完成等
        DialogueManager.Instance.OnDialogueEnd += OnConditionFinished;
        InventoryManager.Instance.OnAddItem += OnConditionFinished;
    }

    private void OnDestroy()
    {
        // 取消订阅事件
        DialogueManager.Instance.OnDialogueEnd -= OnConditionFinished;
        InventoryManager.Instance.OnAddItem -= OnConditionFinished;
    }
    
    private void InitializeQuest()
    {
        allQuests.Clear();
        var questArray = Resources.LoadAll<QuestData>("ScriptableObjects/Quests");
        foreach (var questData in questArray)
        {
            if (questDictionary.TryAdd(questData.questID, questData))
            {
                allQuests.Add(questData);
            }
        }
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
            
        if (questDictionary.TryGetValue(questID, out var quest))
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
        if (questDictionary.TryGetValue(questID, out var quest))
        {
            quest.isCompleted = true;
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
        if (currentQuest == null)
        {
            return false;
        }
        string value = currentQuest.conditionValue;
        switch (currentQuest.questConditionType)
        {
            case QuestCondition.None:
                return true; // 没有条件
            case QuestCondition.CompleteDialogue:
                // 检查对话是否完成
                string[] dialogueIDs = value.Split(';');
                value = dialogueIDs.Where(dialogueID => dialogueID == completedValue).Aggregate(value, (current, dialogueID) => current.Replace(dialogueID, ""));
                return string.IsNullOrEmpty(value);
            case QuestCondition.HaveItem:
                // 检查是否拥有物品
                string[] itemIDs = value.Split(';');
                value = itemIDs.Where(item => item == completedValue).Aggregate(value, (current, item) => current.Replace(item, ""));
                return string.IsNullOrEmpty(value);
            case QuestCondition.CompleteQuest:
                // 检查任务是否完成
                return IsQuestCompleted(value);
            default:
                return false;
        }
    }
    
    // 显示任务文本
    public void ToggleQuestText(string text = "")
    {
        questText.gameObject.SetActive(text != "");
        questText.text = text;
    }
    
    // 加载所有任务数据
    public void LoadAllQuests(List<AsyncSaveLoadSystem.QuestSaveData> quests)
    {
        allQuests = quests;
        questDictionary.Clear();
        foreach (var quest in allQuests)
        {
            if (!questDictionary.ContainsKey(quest.questID))
            {
                questDictionary.Add(quest.questID, quest);
            }
        }
    }
    
    // 获取所有任务
    public List<QuestData> GetAllQuests()
    {
        return new List<QuestData>(allQuests);
    }
    
    public QuestData GetQuest(string questID)
    {
        questDictionary.TryGetValue(questID, out var quest);
        return quest;
    }
    
    public bool IsQuestCompleted(string questID)
    {
        if (questDictionary.TryGetValue(questID, out var quest))
        {
            return quest.isCompleted;
        }
        return false;
    }
}