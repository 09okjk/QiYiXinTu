using System.Collections;
using System.Collections.Generic;
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
    [SerializeField] private int maxSaveSlots = 5;
    
    private bool isMenuActive = false;
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    private void Start()
    {
        // Setup volume sliders
        if (masterVolumeSlider != null)
            masterVolumeSlider.onValueChanged.AddListener(SetMasterVolume);
            
        if (musicVolumeSlider != null)
            musicVolumeSlider.onValueChanged.AddListener(SetMusicVolume);
            
        if (sfxVolumeSlider != null)
            sfxVolumeSlider.onValueChanged.AddListener(SetSFXVolume);
        
        // Initialize with saved values
        LoadAudioSettings();
        
        // Hide all panels initially
        CloseAllPanels();
    }
    
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
        Time.timeScale = 0; // Pause game
    }
    
    public void CloseMenu()
    {
        isMenuActive = false;
        CloseAllPanels();
        Time.timeScale = 1; // Resume game
    }
    
    public void OpenSettings()
    {
        CloseAllPanels();
        settingsPanel.SetActive(true);
    }
    
    public void OpenControls()
    {
        CloseAllPanels();
        controlsPanel.SetActive(true);
    }
    
    public void OpenSavePanel()
    {
        CloseAllPanels();
        savePanel.SetActive(true);
        PopulateSaveSlots();
    }
    
    private void CloseAllPanels()
    {
        mainMenuPanel.SetActive(false);
        settingsPanel.SetActive(false);
        controlsPanel.SetActive(false);
        savePanel.SetActive(false);
    }
    
    private void PopulateSaveSlots()
    {
        // Clear existing slots
        foreach (Transform child in saveSlotContainer)
        {
            Destroy(child.gameObject);
        }
        
        // Get save data info
        SaveDataInfo[] saveDataInfos = SaveLoadSystem.GetSaveDataInfos();
        
        // Create slots for existing saves
        for (int i = 0; i < saveDataInfos.Length; i++)
        {
            CreateSaveSlot(i, saveDataInfos[i]);
        }
        
        // Create empty slots up to max
        for (int i = saveDataInfos.Length; i < maxSaveSlots; i++)
        {
            CreateSaveSlot(i, null);
        }
    }
    
    private void CreateSaveSlot(int slotIndex, SaveDataInfo info)
    {
        GameObject slotGO = Instantiate(saveSlotPrefab, saveSlotContainer);
        SaveSlotUI slotUI = slotGO.GetComponent<SaveSlotUI>();
        
        if (info != null)
        {
            // Existing save
            slotUI.SetupExistingSlot(slotIndex, info);
        }
        else
        {
            // Empty slot
            slotUI.SetupEmptySlot(slotIndex);
        }
    }
    
    // Audio settings
    public void SetMasterVolume(float volume)
    {
        SetAudioMixerVolume("MasterVolume", volume);
        PlayerPrefs.SetFloat("MasterVolume", volume);
        PlayerPrefs.Save();
    }
    
    public void SetMusicVolume(float volume)
    {
        SetAudioMixerVolume("MusicVolume", volume);
        PlayerPrefs.SetFloat("MusicVolume", volume);
        PlayerPrefs.Save();
    }
    
    public void SetSFXVolume(float volume)
    {
        SetAudioMixerVolume("SFXVolume", volume);
        PlayerPrefs.SetFloat("SFXVolume", volume);
        PlayerPrefs.Save();
    }
    
    private void SetAudioMixerVolume(string parameterName, float normalizedValue)
    {
        // Convert normalized value (0-1) to mixer value (logarithmic, -80db to 0db)
        float mixerValue = normalizedValue > 0.001f ? Mathf.Log10(normalizedValue) * 20 : -80f;
        audioMixer.SetFloat(parameterName, mixerValue);
    }
    
    private void LoadAudioSettings()
    {
        float masterVolume = PlayerPrefs.GetFloat("MasterVolume", 0.75f);
        float musicVolume = PlayerPrefs.GetFloat("MusicVolume", 0.75f);
        float sfxVolume = PlayerPrefs.GetFloat("SFXVolume", 0.75f);
        
        // Set slider values
        if (masterVolumeSlider != null) masterVolumeSlider.value = masterVolume;
        if (musicVolumeSlider != null) musicVolumeSlider.value = musicVolume;
        if (sfxVolumeSlider != null) sfxVolumeSlider.value = sfxVolume;
        
        // Apply to audio mixer
        SetAudioMixerVolume("MasterVolume", masterVolume);
        SetAudioMixerVolume("MusicVolume", musicVolume);
        SetAudioMixerVolume("SFXVolume", sfxVolume);
    }
    
    // Save/Load functions
    public void SaveGame(int slotIndex)
    {
        SaveLoadSystem.SaveGame(slotIndex);
        PopulateSaveSlots();
    }
    
    public void LoadGame(int slotIndex)
    {
        SaveLoadSystem.LoadGame(slotIndex);
        CloseMenu();
    }
    
    public void ReturnToMainMenu()
    {
        CloseAllPanels();
        mainMenuPanel.SetActive(true);
    }
    
    public void QuitToDesktop()
    {
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }
}