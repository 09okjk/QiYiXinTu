﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;
using TMPro;
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
    [SerializeField] private AudioMixer audioMixer;
    [SerializeField] private Slider masterVolumeSlider;
    [SerializeField] private Slider musicVolumeSlider;
    [SerializeField] private Slider sfxVolumeSlider;
    
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
    }
    
    private void Start()
    {
        // 设置音量滑块
        if (masterVolumeSlider != null)
            masterVolumeSlider.onValueChanged.AddListener(SetMasterVolume);
            
        if (musicVolumeSlider != null)
            musicVolumeSlider.onValueChanged.AddListener(SetMusicVolume);
            
        if (sfxVolumeSlider != null)
            sfxVolumeSlider.onValueChanged.AddListener(SetSFXVolume);
        
        // 初始化保存的值
        LoadAudioSettings();
        
        // 初始时隐藏所有面板
        CloseAllPanels();
        
        // 检测当前场景是否为主菜单，如果是则显示主菜单面板
        if (SceneManager.GetActiveScene().name == "MainMenu")
        {
            mainMenuPanel.SetActive(true);
        }
    }
    
    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Debug.Log($"场景加载：{scene.name}，当前 MenuManager 是否为实例：{this == Instance}");
        // Debug.Log($"当前 TimeScale: {Time.timeScale}");
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
        CloseAllPanels();
        savePanel.SetActive(true);
        OnMenuStateChanged?.Invoke(true);
        PopulateSaveSlots();
    }
    
    public void CloseSavePanel()
    {
        savePanel.SetActive(false);
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

    internal void CloseAllPanels()
    {
        mainMenuPanel.SetActive(false);
        settingsPanel.SetActive(false);
        controlsPanel.SetActive(false);
        savePanel.SetActive(false);
    }
    
    // 保存和加载游戏的插槽
    private void PopulateSaveSlots()
    {
        // 清除现有的插槽
        foreach (Transform child in saveSlotContainer)
        {
            Destroy(child.gameObject);
        }
        
        // 获取保存的数据信息
        SaveDataInfo[] saveDataInfos = SaveLoadSystem.GetSaveDataInfos();
        
        // 创建现有保存的插槽
        for (int i = 0; i < saveDataInfos.Length; i++)
        {
            CreateSaveSlot(i, saveDataInfos[i]);
        }
        
        // 创建空插槽到最大
        for (int i = saveDataInfos.Length; i < maxSaveSlots; i++)
        {
            CreateSaveSlot(i, null);
        }
    }
    
    // 用于创建保存插槽
    private void CreateSaveSlot(int slotIndex, SaveDataInfo info)
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
    
    // 音频设置
    // 设置音量
    public void SetMasterVolume(float volume)
    {
        SetAudioMixerVolume("MasterVolume", volume);
        PlayerPrefs.SetFloat("MasterVolume", volume);
        PlayerPrefs.Save();
    }
    // 设置音乐音量
    public void SetMusicVolume(float volume)
    {
        SetAudioMixerVolume("MusicVolume", volume);
        PlayerPrefs.SetFloat("MusicVolume", volume);
        PlayerPrefs.Save();
    }
    // 设置音效音量
    public void SetSFXVolume(float volume)
    {
        SetAudioMixerVolume("SFXVolume", volume);
        PlayerPrefs.SetFloat("SFXVolume", volume);
        PlayerPrefs.Save();
    }
    // 设置音频混合器音量
    private void SetAudioMixerVolume(string parameterName, float normalizedValue)
    {
        // 将归一化值（0-1）转换为混音器值（对数，-80db到0db）
        float mixerValue = normalizedValue > 0.001f ? Mathf.Log10(normalizedValue) * 20 : -80f;
        audioMixer.SetFloat(parameterName, mixerValue);
    }
    // 加载音频设置
    private void LoadAudioSettings()
    {
        float masterVolume = PlayerPrefs.GetFloat("MasterVolume", 0.75f);
        float musicVolume = PlayerPrefs.GetFloat("MusicVolume", 0.75f);
        float sfxVolume = PlayerPrefs.GetFloat("SFXVolume", 0.75f);
        
        // Set slider values
        if (masterVolumeSlider != null) masterVolumeSlider.value = masterVolume;
        if (musicVolumeSlider != null) musicVolumeSlider.value = musicVolume;
        if (sfxVolumeSlider != null) sfxVolumeSlider.value = sfxVolume;
        
        // 应用到音频混合器
        SetAudioMixerVolume("MasterVolume", masterVolume);
        SetAudioMixerVolume("MusicVolume", musicVolume);
        SetAudioMixerVolume("SFXVolume", sfxVolume);
    }
    
    // 返回主菜单
    public void ReturnToMainMenu()
    {
        CloseAllPanels();
        mainMenuPanel.SetActive(true);
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
    
    // 开始新游戏
    public async void StartNewGame()
    {
        try
        {
            // 重置游戏数据
            await SceneManager.LoadSceneAsync("女生宿舍");
            GameManager.Instance.OnGameEvent("GameStarted");
        }
        catch (Exception e)
        {
            throw; 
        }
    }
    
    // 检测是否有UI面板打开
    public bool IsAnyPanelOpen()
    {
        return mainMenuPanel.activeSelf || settingsPanel.activeSelf || controlsPanel.activeSelf || savePanel.activeSelf;
    }
}