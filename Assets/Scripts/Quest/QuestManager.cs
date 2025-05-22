using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
        else
        {
            Destroy(gameObject);
        }
    }
    
    private void Start()
    {
        // 订阅各种可能导致任务完成的事件
        // 例如：对话完成、物品使用、任务完成等
        DialogueManager.Instance.OnDialogueComplete += OnDialogueFinished;
        InventoryManager.Instance.OnAddItem += OnDialogueFinished;
    }

    private void OnDialogueFinished()
    {
        if(CheckQuestCondition())
        {
            FinishQuest(currentQuestID);
        }
    }

    private void OnDestroy()
    {
        // 取消订阅事件
        DialogueManager.Instance.OnDialogueComplete -= OnDialogueFinished;
        InventoryManager.Instance.OnAddItem -= OnDialogueFinished;
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
    private bool CheckQuestCondition()
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
                foreach (var dialogueID in dialogueIDs)
                {
                    if (!DialogueManager.Instance.IsDialogueFinished(dialogueID))
                    {
                        return false;
                    }
                }
                return true;
            case QuestCondition.HaveItem:
                // 检查是否拥有物品
                string[] itemIDs = value.Split(';');
                foreach (var item in itemIDs)
                {
                    if (!InventoryManager.Instance.HasItem(item))
                    {
                        return false;
                    }
                }
                return true;
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