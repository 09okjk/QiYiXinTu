using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    
    [Header("References")]
    [SerializeField] private GameObject loadingScreen;
    [SerializeField] private UnityEngine.UI.Slider loadingBar;
    
    [Header("Settings")]
    [SerializeField] private float minimumLoadingTime = 0.5f;
    
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
        if (loadingScreen != null)
        {
            loadingScreen.SetActive(false);
        }
        // Subscribe to scene loading events
        SceneManager.sceneLoaded += OnSceneLoaded;
    }
    
    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
    
    public void LoadScene(string sceneName)
    {
        StartCoroutine(LoadSceneAsync(sceneName));
    }
    
    private IEnumerator LoadSceneAsync(string sceneName)
    {
        // Show loading screen
        if (loadingScreen != null)
        {
            loadingScreen.SetActive(true);
        }
        
        // Start async loading
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
        asyncLoad.allowSceneActivation = false;
        
        float startTime = Time.time;
        
        // Update loading progress
        while (!asyncLoad.isDone)
        {
            float progress = Mathf.Clamp01(asyncLoad.progress / 0.9f);
            
            if (loadingBar != null)
            {
                loadingBar.value = progress;
            }
            
            // Wait until we're close to done and minimum time has passed
            if (asyncLoad.progress >= 0.9f && Time.time - startTime >= minimumLoadingTime)
            {
                asyncLoad.allowSceneActivation = true;
            }
            
            yield return null;
        }
    }
    
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Hide loading screen
        if (loadingScreen != null)
        {
            loadingScreen.SetActive(false);
        }
        
        // Initialize scene
        InitializeScene(scene.name);
    }
    
    private void InitializeScene(string sceneName)
    {
        // Setup scene-specific logic
        switch (sceneName)
        {
            case "MainMenu":
                Time.timeScale = 1f; // Ensure game is not paused
                break;
                
            case "GameScene":
                // Find the player start position and place player there
                GameObject playerStart = GameObject.FindGameObjectWithTag("PlayerStart");
                GameObject player = GameObject.FindGameObjectWithTag("Player");
                
                if (playerStart != null && player != null)
                {
                    player.transform.position = playerStart.transform.position;
                }
                break;
                
            // Add more scenes as needed
        }
    }
    
    // For handling game progression
    public void OnGameEvent(string eventName)
    {
        // Example of handling game events
        switch (eventName)
        {
            case "GameStarted":
                // Initialize game state
                GameStateManager.Instance.ClearAllFlags();
                // Start initial quests
                QuestManager.Instance.StartQuest("quest_intro");
                break;
                
            case "PlayerDied":
                // Show game over screen
                UIManager.Instance.ShowConfirmDialog(
                    "你死了",
                    "是否重新加载最近的保存点？",
                    () => LoadLastSave(),
                    () => LoadScene("MainMenu")
                );
                break;
                
            // Add more events as needed
        }
    }
    
    private void LoadLastSave()
    {
        // Find the most recent save
        SaveDataInfo[] saves = SaveLoadSystem.GetSaveDataInfos();
        
        if (saves.Length > 0)
        {
            // Sort by date (newest first)
            System.Array.Sort(saves, (a, b) => b.saveDate.CompareTo(a.saveDate));
            
            // Load the newest save
            SaveLoadSystem.LoadGame(saves[0].slotIndex);
        }
        else
        {
            // No saves found, go to main menu
            LoadScene("MainMenu");
        }
    }
}