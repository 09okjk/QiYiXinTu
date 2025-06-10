using System.Collections;
using System.Collections.Generic;
using UI;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    
    [Header("References")]
    [SerializeField] private GameObject loadingScreen;
    [SerializeField] private GameObject pressAnyKeyPrompt;
    [SerializeField] private UnityEngine.UI.Slider loadingBar;
    
    [Header("Settings")]
    [SerializeField] private float minimumLoadingTime = 0.5f;
    
    public bool canSwitchScenes; // 是否允许切换场景
    
    private bool gameStarted = false;
    
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
        /*if (loadingScreen)
        {
            loadingScreen.SetActive(false);
        }*/
        // 添加订阅场景加载事件
        SceneManager.sceneLoaded += OnSceneLoaded;
        
        // 显示"按任意键开始"提示
        pressAnyKeyPrompt.SetActive(true);
        loadingBar.gameObject.SetActive(false);
        gameStarted = false;
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
    
    public void LoadScene(string sceneName)
    {
        StartCoroutine(LoadSceneAsync(sceneName));
    }
    
    private IEnumerator LoadSceneAsync(string sceneName)
    {
        // 显示加载界面
        if (loadingScreen != null)
        {
            loadingScreen.SetActive(true);
        }
        
        // 异步加载场景
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
        asyncLoad.allowSceneActivation = false;
        
        float startTime = Time.time;
        
        // 更新加载进度
        while (!asyncLoad.isDone)
        {
            float progress = Mathf.Clamp01(asyncLoad.progress / 0.9f);
            
            if (loadingBar != null)
            {
                loadingBar.value = progress;
            }
            
            // 等待直到接近完成并且最小时间已过
            if (asyncLoad.progress >= 0.9f && Time.time - startTime >= minimumLoadingTime)
            {
                asyncLoad.allowSceneActivation = true;
            }
            yield return null;
        }
    }
    
    // 处理场景加载完成事件
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // 隐藏加载界面
        if (loadingScreen != null)
        {
            loadingScreen.SetActive(false);
        }
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
                // 播放开场动画
                if(!GameStateManager.Instance.GetFlag("StartAnimationFinished"))
                    StartAnimationCotroller.Instance.PlayVideo(0);
                break;
            case "outside1":
                // 触发开场对话
                DialogueManager.Instance.StartDialogueByID("lide_dialogue");
                break;
            case "outside1_1":
                break;
            case "In_LiDe":
                DialogueManager.Instance.StartDialogueByID("lide_inside1_instruction_dialogue");
                break;
            case "Scene 4":
                // 找到玩家初始位置并放置玩家
                // GameObject playerStart = GameObject.FindGameObjectWithTag("PlayerStart");
                // GameObject player = GameObject.FindGameObjectWithTag("Player");
                //
                // if (playerStart != null && player != null)
                // {
                //     player.transform.position = playerStart.transform.position;
                // }
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
                // 开始初始任务
                // QuestManager.Instance.StartQuest("quest_001");
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
    
    // 加载最近的保存
    private void LoadLastSave()
    {
        // 找到最近的存档
        SaveDataInfo[] saves = SaveLoadSystem.GetSaveDataInfos();
        
        if (saves.Length > 0)
        {
            // 按日期排序（最新的在前）
            System.Array.Sort(saves, (a, b) => b.saveDate.CompareTo(a.saveDate));
            
            // 加载最新的存档
            SaveLoadSystem.LoadGame(saves[0].slotIndex);
        }
        else
        {
            // 没有找到存档，返回主菜单
            LoadScene("MainMenu");
        }
    }
}