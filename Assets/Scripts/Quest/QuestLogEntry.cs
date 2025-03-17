using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class QuestLogEntry : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI questNameText;
    [SerializeField] private TextMeshProUGUI questDescriptionText;
    [SerializeField] private Transform objectiveContainer;
    [SerializeField] private GameObject objectiveEntryPrefab;
    [SerializeField] private GameObject detailsPanel;
    
    private QuestData quest;
    
    public void SetQuest(QuestData newQuest)
    {
        quest = newQuest;
        
        // 设置名称
        if (questNameText != null)
        {
            questNameText.text = quest.questName;
        }
        
        UpdateDisplay();
    }
    
    /// <summary>
    /// 更新任务显示
    /// </summary>
    private void UpdateDisplay()
    {
        // 清除现有目标条目
        foreach (Transform child in objectiveContainer)
        {
            Destroy(child.gameObject);
        }
        
        // 设置描述
        if (questDescriptionText != null)
        {
            questDescriptionText.text = quest.description;
        }
        
        // 添加目标条目
        foreach (QuestObjective objective in quest.objectives)
        {
            GameObject entryGO = Instantiate(objectiveEntryPrefab, objectiveContainer);
            TextMeshProUGUI entryText = entryGO.GetComponentInChildren<TextMeshProUGUI>();
            Toggle entryToggle = entryGO.GetComponentInChildren<Toggle>();
            
            if (entryText != null)
            {
                entryText.text = objective.description;
            }
            
            if (entryToggle != null)
            {
                entryToggle.isOn = objective.isCompleted;
                entryToggle.interactable = false; // 只读
            }
        }
    }
    
    /// <summary>
    /// 描述面板开关
    /// </summary>
    public void ToggleDetails()
    {
        if (detailsPanel != null)
        {
            detailsPanel.SetActive(!detailsPanel.activeSelf);
        }
    }
}