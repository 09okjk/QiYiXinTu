using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Audio;
using Manager;
using Save;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;
using TMPro;
using UI;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    public static MenuManager Instance { get; private set; }
    
    [Header("Panels")]
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private GameObject controlsPanel;
    [SerializeField] private GameObject savePanel;
    
    [Header("Audio Settings")]
    [SerializeField] private Slider masterVolumeSlider; // 主音量滑动条
    [SerializeField] private Slider musicVolumeSlider; // 音乐音量滑动条
    [SerializeField] private Slider sfxVolumeSlider;// 音效音量滑动条
    
    [Header("Save/Load")]
    [SerializeField] private Transform saveSlotContainer;
    [SerializeField] private GameObject saveSlotPrefab;
    [SerializeField] private int maxSaveSlots = 6;
    
    private bool isMenuActive = false;
    
    public event Action<bool> OnMenuStateChanged; // 事件，用于通知其他脚本菜单状态的变化
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            //DontDestroyOnLoad(gameObject);
        }
        else
        {
            Debug.Log($"发现重复的 MenuManager 实例：{gameObject.name}，当前实例：{Instance.gameObject.name}");
            Destroy(gameObject);
        }
        // 初始化音量滑块
        masterVolumeSlider.value = 1f;
        musicVolumeSlider.value = 1f;
        sfxVolumeSlider.value = 1f;
    }
    
    private void Start()
    {
        // 设置音量滑块
        if (masterVolumeSlider != null)
            masterVolumeSlider.onValueChanged.AddListener(SetMasterVolume);
            
        if (musicVolumeSlider != null)
            musicVolumeSlider.onValueChanged.AddListener(SetBackgroundVolume);
            
        if (sfxVolumeSlider != null)
            sfxVolumeSlider.onValueChanged.AddListener(SetEffectVolume);
        
        // 初始时隐藏所有面板
        CloseAllPanels();
        
        // 检测当前场景是否为主菜单，如果是则显示主菜单面板
        if (SceneManager.GetActiveScene().name == "MainMenu")
        {
            mainMenuPanel.SetActive(true);
        }
    }

    private void SetMasterVolume(float volume)
    {
        AudioManager.Instance.SetMainVolume(volume);
    }
    
    private void SetBackgroundVolume(float volume)
    {
        AudioManager.Instance.SetBackgroundAudioVolume(volume);
    }
    
    private void SetEffectVolume(float volume)
    {
        AudioManager.Instance.SetEffectAudioVolume(volume);
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        OnMenuStateChanged += OnMenuStateChangedHandler;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        OnMenuStateChanged -= OnMenuStateChangedHandler;
    }

    private void OnMenuStateChangedHandler(bool isOpen)
    {
        
        try
        {
            PlayerManager.Instance.player.HandleMenuStateChanged(isOpen);
        }
        catch (Exception e)
        {
            Debug.Log("player 不存在或未初始化，无法处理菜单状态变化: " + e.Message);
        }
    }


    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Debug.Log($"场景加载：{scene.name}，当前 MenuManager 是否为实例：{this == Instance}");
        // Debug.Log($"当前 TimeScale: {Time.timeScale}");
    }
    public void OnDataSave()
    {
        Debug.Log($"数据保存完成");
        if (savePanel.activeSelf)
        {
            PopulateSaveSlots();
        }
    }
    
    // 切换菜单的显示状态
    public void ToggleMenu()
    {
        if (isMenuActive)
        {
            CloseMenu();
        }
        else
        {
            OpenMenu();
        }
    }
    
    public void OpenMenu()
    {
        isMenuActive = true;
        mainMenuPanel.SetActive(true);
        OnMenuStateChanged?.Invoke(true);
        Time.timeScale = 0; // 直接设置时间缩放为0

        //GameStateManager.Instance.PauseGame();
        Debug.Log("Menu Opened, TimeScale: " + Time.timeScale);
        
    }
    
    public void CloseMenu()
    {
        isMenuActive = false;
        CloseAllPanels();
        OnMenuStateChanged?.Invoke(false);
        Time.timeScale = 1; // 直接设置时间缩放为0

        //GameStateManager.Instance.ResumeGame();
        Debug.Log("Menu Closed, TimeScale: " + Time.timeScale);    
    }
    
    public void OpenSettings()
    {
        CloseAllPanels();
        OnMenuStateChanged?.Invoke(true);
        // 设置音量滑块的当前值
        var audioData = AudioManager.Instance.GetAudioGameData();
        if (audioData != null)
        {
            masterVolumeSlider.value = audioData.mainVolume;
            musicVolumeSlider.value = audioData.backgroundVolume;
            sfxVolumeSlider.value = audioData.effectVolume;
        }
        else
        {
            Debug.LogWarning("AudioGameData is null, using default values.");
            masterVolumeSlider.value = 1f;
            musicVolumeSlider.value = 1f;
            sfxVolumeSlider.value = 1f;
        }
        settingsPanel.SetActive(true);
    }
    
    public void CloseSettings()
    {
        settingsPanel.SetActive(false);
        // 如果当前场景是主菜单，则显示主菜单面板
        if (SceneManager.GetActiveScene().name == "MainMenu")
        {
            mainMenuPanel.SetActive(true);
        }
        else
        {
            CloseMenu();
        }
    }
    
    public void OpenControls()
    {
        CloseAllPanels();
        OnMenuStateChanged?.Invoke(true);
        controlsPanel.SetActive(true);
    }
    
    public void CloseControls()
    {
        controlsPanel.SetActive(false);       
        // 如果当前场景是主菜单，则显示主菜单面板
        if (SceneManager.GetActiveScene().name == "MainMenu")
        {
            mainMenuPanel.SetActive(true);
        }
        else
        {
            CloseMenu();
        }
    }
    
    public void OpenSavePanel()
    {
        try
        {
            CloseAllPanels();
            savePanel.SetActive(true);
            OnMenuStateChanged?.Invoke(true);
            PopulateSaveSlots();
        }
        catch (Exception e)
        {
            Debug.LogError("打开保存面板时发生错误: " + e.Message);
            throw;
        }
    }
    
    public void CloseSavePanel()
    {
        savePanel.SetActive(false);
        // 如果当前场景是主菜单，则显示主菜单面板
        if (SceneManager.GetActiveScene().name == "MainMenu")
        {
            MainMenuManager.Instance.ShowMainMenuUI();
        }
        else
        {
            CloseMenu();
        }
    }

    internal void CloseAllPanels()
    {
        mainMenuPanel.SetActive(false);
        settingsPanel.SetActive(false);
        controlsPanel.SetActive(false);
        savePanel.SetActive(false);
        OnMenuStateChanged?.Invoke(false);
    }
    
    // 保存和加载游戏的插槽
    private async void PopulateSaveSlots()
    {
        try
        {
            // 清除现有的插槽
            foreach (Transform child in saveSlotContainer)
            {
                Destroy(child.gameObject);
            }
            
            // 显示加载中提示
            Debug.Log("正在加载存档信息...");
            
            // 使用await等待异步操作完成
            List<SaveData> saveDataInfos = await SaveLoadAsyncSystem.GetAllSaves();

            // 创建一个与maxSaveSlots大小相同的数组，默认值为null
            SaveData[] sortedSaveData = new SaveData[maxSaveSlots];

            // 将现有存档信息放入对应的索引位置
            for (int i = 0; i < saveDataInfos.Count; i++)
            {
                if (saveDataInfos[i].saveSlotIndex >= 0 && saveDataInfos[i].saveSlotIndex < maxSaveSlots)
                {
                    sortedSaveData[saveDataInfos[i].saveSlotIndex] = saveDataInfos[i];
                }
                else
                {
                    Debug.LogWarning($"存档槽索引超出范围: {saveDataInfos[i].saveSlotIndex}");
                }
            }

            // 按顺序创建所有存档槽
            for (int i = 0; i < maxSaveSlots; i++)
            {
                CreateSaveSlot(i, sortedSaveData[i]);
            }
            
            Debug.Log("存档插槽加载完成");
        }
        catch (Exception e)
        {
            Debug.LogError($"加载存档信息失败: {e.Message}\n{e.StackTrace}");
        }
    }
    
    // 用于创建保存插槽
    private void CreateSaveSlot(int slotIndex, SaveData info)
    {
        GameObject slotGO = Instantiate(saveSlotPrefab, saveSlotContainer);
        SaveSlotUI slotUI = slotGO.GetComponent<SaveSlotUI>();
        
        if (info != null)
        {
            // 已存在的存档
            slotUI.SetupExistingSlot(slotIndex, info);
        }
        else
        {
            // 空插槽
            slotUI.SetupEmptySlot(slotIndex);
        }
    }
    
    // 返回主菜单
    public void ReturnToMainMenu()
    {
        CloseAllPanels();
        //GameManager.Instance.LoadScene("MainMenu");
        // 直接退出游戏
        QuitToDesktop();
    }
    // 退出到桌面
    public void QuitToDesktop()
    {
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }
    
    // 检测是否有UI面板打开
    public bool IsAnyPanelOpen()
    {
        return mainMenuPanel.activeSelf || settingsPanel.activeSelf || controlsPanel.activeSelf || savePanel.activeSelf;
    }
}