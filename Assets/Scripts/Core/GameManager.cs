using System.Collections;
using System.Collections.Generic;
using UI;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    
    [Header("References")]
    [SerializeField] private GameObject loadingScreen;
    [SerializeField] private GameObject pressAnyKeyPrompt;
    [SerializeField] private UnityEngine.UI.Slider loadingBar;
    [SerializeField] private TMPro.TextMeshProUGUI loadingText; // 新增：加载文本显示
    
    [Header("Settings")]
    [SerializeField] private float minimumLoadingTime = 0.5f;
    
    public bool canSwitchScenes; // 是否允许切换场景
    
    private bool gameStarted = false;
    private bool isLoadingScreenActive = false;
    
    // 加载进度事件
    public static event Action<float> OnLoadingProgress;
    public static event Action<string> OnLoadingStatusChanged;
    public static event Action<string> OnBeforeLevelChange;
    
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
        // 添加订阅场景加载事件
        SceneManager.sceneLoaded += OnSceneLoaded;
        
        // 显示"按任意键开始"提示
        pressAnyKeyPrompt.SetActive(true);
        loadingBar.gameObject.SetActive(false);
        gameStarted = false;
        
        // 初始时隐藏加载屏幕
        // if (loadingScreen != null)
        // {
        //     loadingScreen.SetActive(false);
        // }
    }
    
    private void Update()
    {
        if (!canSwitchScenes) return;
        // 如果游戏尚未开始且检测到任意按键或点击
        if (!gameStarted && (Input.anyKeyDown || Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1)))
        {
            // 隐藏提示并开始加载场景
            gameStarted = true;
            pressAnyKeyPrompt.SetActive(false);
            loadingBar.gameObject.SetActive(true);
            LoadScene("MainMenu");
        }
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
    
    #region 加载屏幕控制接口
    
    /// <summary>
    /// 显示加载屏幕
    /// </summary>
    /// <param name="title">加载标题</param>
    /// <param name="showProgressBar">是否显示进度条</param>
    public void ShowLoadingScreen(string title = "加载中...", bool showProgressBar = true)
    {
        if (loadingScreen != null)
        {
            loadingScreen.SetActive(true);
            isLoadingScreenActive = true;
            
            if (loadingText != null)
            {
                loadingText.text = title;
            }
            
            if (loadingBar != null)
            {
                loadingBar.gameObject.SetActive(showProgressBar);
                loadingBar.value = 0f;
            }
            
            Debug.Log($"显示加载屏幕: {title}");
        }
    }
    
    /// <summary>
    /// 更新加载进度
    /// </summary>
    /// <param name="progress">进度值 (0-1)</param>
    /// <param name="statusText">状态文本 (可选)</param>
    public void UpdateLoadingProgress(float progress, string statusText = null)
    {
        if (!isLoadingScreenActive) return;
        
        progress = Mathf.Clamp01(progress);
        
        if (loadingBar != null)
        {
            loadingBar.value = progress;
        }
        
        if (!string.IsNullOrEmpty(statusText) && loadingText != null)
        {
            loadingText.text = statusText;
        }
        
        // 触发全局进度事件
        OnLoadingProgress?.Invoke(progress);
        OnLoadingStatusChanged?.Invoke(statusText);
        
        Debug.Log($"更新加载进度: {progress:P1} - {statusText}");
    }
    
    /// <summary>
    /// 隐藏加载屏幕
    /// </summary>
    public void HideLoadingScreen()
    {
        if (loadingScreen != null)
        {
            loadingScreen.SetActive(false);
            isLoadingScreenActive = false;
            
            Debug.Log("隐藏加载屏幕");
        }
    }
    
    /// <summary>
    /// 检查加载屏幕是否激活
    /// </summary>
    public bool IsLoadingScreenActive()
    {
        return isLoadingScreenActive;
    }
    
    #endregion

    public void ResetAllData()
    {
        DialogueManager.Instance.ResetData();
    }
    public void LoadScene(string sceneName)
    {
        StartCoroutine(LoadSceneAsync(sceneName));
    }
    
    private IEnumerator LoadSceneAsync(string sceneName)
    {
        // 显示加载界面
        ShowLoadingScreen($"正在加载场景: {sceneName}");
        
        // 异步加载场景
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
        asyncLoad.allowSceneActivation = false;
        
        float startTime = Time.time;
        
        // 更新加载进度
        while (!asyncLoad.isDone)
        {
            float progress = Mathf.Clamp01(asyncLoad.progress / 0.9f);
            
            UpdateLoadingProgress(progress, $"正在加载场景: {sceneName} ({progress:P0})");
            
            // 等待直到接近完成并且最小时间已过
            if (asyncLoad.progress >= 0.9f && Time.time - startTime >= minimumLoadingTime)
            {
                UpdateLoadingProgress(1f, "加载完成");
                asyncLoad.allowSceneActivation = true;
            }
            yield return null;
        }
    }
    
    // 处理场景加载完成事件
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // 隐藏加载界面
        if (scene.name == "MainMenu")
            HideLoadingScreen();
        InitializeScene(scene.name);
    }
    
    // 初始化场景
    private void InitializeScene(string sceneName)
    {
        // 建立场景初始化逻辑
        switch (sceneName)
        {
            case "MainMenu":
                Time.timeScale = 1f; // 确保游戏没有暂停
                break;
            case "女生宿舍":
                break;
            case "outside1":
                // 触发开场对话
                break;
            case "outside1_1":
                break;
            case "In_LiDe":
                break;
            case "Scene 4":
                break;
            // 根据需要添加更多场景
        }
    }
    
    // 处理进度
    public void OnGameEvent(string eventName)
    {
        // 处理游戏事件的示例
        switch (eventName)
        {
            case "GameStarted":
                // 初始化游戏状态
                GameStateManager.Instance.ClearAllFlags();
                break;
                
            case "PlayerDied":
                // 显示游戏结束界面
                UIManager.Instance.ShowConfirmDialog(
                    "你死了",
                    "是否重新加载最近的保存点？",
                    null,
                    () => LoadScene("MainMenu"), () => LoadLastSave());
                break;
                
            // 根据需要添加更多事件
        }
    }
    
    public void TriggerSceneChangeEvent(string sceneName)
    {
        // 在切换场景前触发事件
        OnBeforeLevelChange?.Invoke(sceneName);
        
        // 这里可以添加其他需要在场景切换前执行的逻辑
        Debug.Log($"触发场景切换事件: {sceneName}");
    }
    
    // 加载最近的保存
    private void LoadLastSave()
    {
        // 实现最近存档加载逻辑
    }
}