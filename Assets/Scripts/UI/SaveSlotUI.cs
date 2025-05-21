using System;
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
    [SerializeField] private RawImage slotBackground;
    [SerializeField] private Color emptySlotColor;
    [SerializeField] private Color existingSlotColor;
    
    private int slotIndex;
    private bool isEmpty;

    private void Start()
    {
        saveButton.onClick.AddListener(OnSaveButtonClicked);
        loadButton.onClick.AddListener(OnLoadButtonClicked);
    }

    public void SetupExistingSlot(int index, SaveDataInfo info)
    {
        slotIndex = index;
        isEmpty = false;
        
        slotNameText.text = info.saveName;
        dateText.text = info.saveDate.ToString("yyyy-MM-dd HH:mm");
        sceneNameText.text = info.sceneName;
        
        slotBackground.color = existingSlotColor;
        
        // 启用两个按钮
        saveButton.interactable = true;
        loadButton.interactable = true;
    }
    
    public void SetupEmptySlot(int index)
    {
        slotIndex = index;
        isEmpty = true;
        
        slotNameText.text = "存档"+(index+1);
        dateText.text = "";
        sceneNameText.text = "";
        
        slotBackground.color = emptySlotColor;
        
        // 只启用保存按钮
        saveButton.interactable = true;
        loadButton.interactable = false;
    }
    
    public void OnSaveButtonClicked()
    {
        // 如果槽不为空，请确认覆盖
        if (!isEmpty)
        {
            // 显示确认对话框（需要 UI 管理器实现）
            UIManager.Instance.ShowConfirmDialog(
                "Overwrite Save?",
                "Are you sure you want to overwrite this save?",
                null, () => SaveLoadSystem.SaveGame(slotIndex));
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
            MenuManager.Instance.CloseAllPanels();
        }
    }
}