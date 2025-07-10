using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
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
    [SerializeField] private Button deleteButton; // 删除按钮（如果需要的话）
    
    private int slotIndex;
    private bool isEmpty;

    private void Start()
    {
        saveButton.onClick.AddListener(OnSaveButtonClicked);
        loadButton.onClick.AddListener(OnLoadButtonClicked);
        deleteButton.onClick.AddListener(OnDeleteButtonClicked);
    }

    // 设置已存在的槽位
    public void SetupExistingSlot(int index, SaveData info)
    {
        slotIndex = index;
        isEmpty = false;
        
        slotNameText.text = info.saveName;
        dateText.text = info.saveTime;
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
    
    private async void OnSaveButtonClicked()
    {
        if (!isEmpty)
        {
            UIManager.Instance.ShowConfirmDialog(
                "覆盖存档",
                "此操作将覆盖现有存档，是否继续?",
                null,
                async () => await SaveGame(false),
                () => { /* 取消操作 */ });
        }
        else if(GameStateManager.Instance.GetFlag("IsNewGame"))
        {
            Debug.LogWarning("保存并重置游戏数据");
            await SaveWithReset();
        }
        else
        {
            // 如果是空槽位，直接保存
            await SaveGame(false);
        }
    }
    
    private void OnLoadButtonClicked()
    {
        if (!isEmpty)
        {
            MenuManager.Instance.CloseAllPanels();
            if (sceneNameText.text == "MainMenu") sceneNameText.text = "女生宿舍";
            GameStateManager.Instance.SetCurrentSaveSlot(slotIndex);
            GameManager.Instance.LoadScene(sceneNameText.text);
        }
    }
    
    private void OnDeleteButtonClicked()
    {
        // 如果槽不为空，请确认删除
        if (!isEmpty)
        {
            // 显示确认对话框（需要 UI 管理器实现）
            UIManager.Instance.ShowConfirmDialog(
                "删除存档",
                "此操作将删除现有存档，是否继续?",
                null,
                () =>
                {
                    SaveLoadAsyncSystem.DeleteSave(slotIndex);
                    MenuManager.Instance.OpenSavePanel();
                },
                () => { /* 取消操作 */ });
        }
    }

    private Task SaveGame(bool isQuickSave)
    {
        SaveLoadAsyncSystem.SaveGame(isQuickSave,slotIndex);
        return Task.CompletedTask;
    }
    
    private Task SaveWithReset()
    {
        // 1. 先执行重置
        GameManager.Instance.ResetAllGameData();
    
        // 2. 完成后再执行保存操作
        SaveLoadAsyncSystem.SaveGame(false,slotIndex);
        return Task.CompletedTask;
    }
}