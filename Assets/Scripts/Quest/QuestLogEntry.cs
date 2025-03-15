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
        
        // Set name
        if (questNameText != null)
        {
            questNameText.text = quest.questName;
        }
        
        UpdateDisplay();
    }
    
    private void UpdateDisplay()
    {
        // Clear existing objective entries
        foreach (Transform child in objectiveContainer)
        {
            Destroy(child.gameObject);
        }
        
        // Set description
        if (questDescriptionText != null)
        {
            questDescriptionText.text = quest.description;
        }
        
        // Add objective entries
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
                entryToggle.interactable = false; // Read-only
            }
        }
    }
    
    public void ToggleDetails()
    {
        if (detailsPanel != null)
        {
            detailsPanel.SetActive(!detailsPanel.activeSelf);
        }
    }
}