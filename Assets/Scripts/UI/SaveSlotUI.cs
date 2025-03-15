using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SaveSlotUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI slotNameText;
    [SerializeField] private TextMeshProUGUI dateText;
    [SerializeField] private TextMeshProUGUI sceneNameText;
    [SerializeField] private Button saveButton;
    [SerializeField] private Button loadButton;
    [SerializeField] private Image slotBackground;
    [SerializeField] private Color emptySlotColor;
    [SerializeField] private Color existingSlotColor;
    
    private int slotIndex;
    private bool isEmpty;
    
    public void SetupExistingSlot(int index, SaveDataInfo info)
    {
        slotIndex = index;
        isEmpty = false;
        
        slotNameText.text = info.saveName;
        dateText.text = info.saveDate.ToString("yyyy-MM-dd HH:mm");
        sceneNameText.text = info.sceneName;
        
        slotBackground.color = existingSlotColor;
        
        // Enable both buttons
        saveButton.interactable = true;
        loadButton.interactable = true;
    }
    
    public void SetupEmptySlot(int index)
    {
        slotIndex = index;
        isEmpty = true;
        
        slotNameText.text = "Empty Slot";
        dateText.text = "";
        sceneNameText.text = "";
        
        slotBackground.color = emptySlotColor;
        
        // Only enable save button
        saveButton.interactable = true;
        loadButton.interactable = false;
    }
    
    public void OnSaveButtonClicked()
    {
        // Confirm overwrite if slot is not empty
        if (!isEmpty)
        {
            // Show confirmation dialog (UI manager implementation needed)
            UIManager.Instance.ShowConfirmDialog(
                "Overwrite Save?",
                "Are you sure you want to overwrite this save?",
                () => SaveLoadSystem.SaveGame(slotIndex),
                null
            );
        }
        else
        {
            SaveLoadSystem.SaveGame(slotIndex);
        }
    }
    
    public void OnLoadButtonClicked()
    {
        if (!isEmpty)
        {
            SaveLoadSystem.LoadGame(slotIndex);
        }
    }
}