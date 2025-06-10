using System;
using System.Collections;
using System.Collections.Generic;
using Save;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class SaveSlotUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI slotNameText;
    [SerializeField] private TextMeshProUGUI dateText;
    [SerializeField] private TextMeshProUGUI sceneNameText;
    [SerializeField] private Button saveButton;
    [SerializeField] private Button loadButton;
    
    private int slotIndex;
    private bool isEmpty;

    private void Start()
    {
        saveButton.onClick.AddListener(OnSaveButtonClicked);
        loadButton.onClick.AddListener(OnLoadButtonClicked);
    }

    // 设置已存在的槽位
    public void SetupExistingSlot(int index, SaveDataInfo info)
    {
        slotIndex = index;
        isEmpty = false;
        
        slotNameText.text = info.saveName;
        dateText.text = info.saveDate.ToString("yyyy-MM-dd HH:mm");
        sceneNameText.text = info.sceneName;
        
        // 启用两个按钮
        loadButton.interactable = true;
        saveButton.interactable = SceneManager.GetActiveScene().name != "MainMenu";
        if (index == 0)
        {
            gameObject.SetActive(true);
        }
    }
    
    // 设置空槽位
    public void SetupEmptySlot(int index)
    {
        slotIndex = index;
        isEmpty = true;

        
        dateText.text = "";
        sceneNameText.text = "";
        
        // 只启用保存按钮
        saveButton.interactable = true;
        loadButton.interactable = false;
        
        if (slotIndex == 0)
        {
            slotNameText.text = "自动保存";
            gameObject.SetActive(false);
        }
        else
        {
            slotNameText.text = "空存档";
        }
    }
    
    private void OnSaveButtonClicked()
    {
        // 如果槽不为空，请确认覆盖
        if (!isEmpty)
        {
            // 显示确认对话框（需要 UI 管理器实现）
            UIManager.Instance.ShowConfirmDialog(
                "覆盖存档",
                "此操作将覆盖现有存档，是否继续?",
                null, 
                () => _ = AsyncSaveLoadSystem.SaveGameAsync(slotIndex),
                () => { /* 取消操作 */ });
        }
        else
        {
            _ = AsyncSaveLoadSystem.SaveGameAsync(slotIndex);
        }
    }
    
    private void OnLoadButtonClicked()
    {
        if (!isEmpty)
        {
            _ = AsyncSaveLoadSystem.LoadGameAsync(slotIndex);
            MenuManager.Instance.CloseAllPanels();
        }
    }
}