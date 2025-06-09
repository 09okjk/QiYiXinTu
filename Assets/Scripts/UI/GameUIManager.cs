using System.Collections;
using System.Collections.Generic;
using Manager;
using News;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class GameUIManager : MonoBehaviour
{
    public static GameUIManager Instance { get; private set; }
    public bool IsInteractingWithUI { get; private set; }
    
    [SerializeField] GameObject gameUIPanel;

    [Header("Player Status")]
    [SerializeField] private Image playerPortrait; // 玩家头像
    [SerializeField] private HealthBarManager healthBarManager;
    
    [Header("Skill Bar")]
    [SerializeField] private Transform skillBarContainer;
    [SerializeField] private GameObject skillSlotPrefab;
    [SerializeField] private int maxSkillSlots = 6;
    
    [Header("Scene Info")]
    [SerializeField] private TextMeshProUGUI sceneNameText;
    [SerializeField] private Button menuButton; // 菜单按钮
    [SerializeField] private Button instructionButton; // 新手引导按钮
    [SerializeField] private Button inventoryButton; // 背包按钮
    [SerializeField] private Button newsButton; // 新闻按钮
    [SerializeField] private Animator sceneAnimator; // 场景动画
    [SerializeField] private Animator luSleepAnimator; // 路睡觉动画
    [SerializeField] private Animator luWeekUpAnimator; // 路睡醒动画
    [SerializeField] private GameObject frontScene; // 前景场景
    private List<SkillSlotUI> skillSlotList = new List<SkillSlotUI>();

    private Player player;
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
        // 不显示游戏UI
        gameUIPanel.SetActive(false);
        luSleepAnimator.gameObject.SetActive(false);
        luWeekUpAnimator.gameObject.SetActive(false);
        
        // 初始化技能栏
        InitializeSkillBar();
        
        // 添加UI交互事件
        AddUIPointerHandlers(menuButton.gameObject);
        AddUIPointerHandlers(inventoryButton.gameObject);
        AddUIPointerHandlers(newsButton.gameObject);
        
        // 设置菜单按钮监听
        menuButton.onClick.AddListener(OpenMenu);
        
        // 设置背包按钮监听
        inventoryButton.onClick.AddListener(OpenInventory);
        
        // 设置新闻按钮监听
        newsButton.onClick.AddListener(OpenNews);
        
        // 更新场景名称
        UpdateSceneName();
        
        
        // 查找并连接玩家
        // FindAndConnectPlayer();
        
        // 显示任务日志面板
        
        player = PlayerManager.Instance.player;

        OnSceneLoaded();
    }

    private void OnDestroy()
    {
        //SceneManager.sceneLoaded -= OnSceneLoaded;
        
        player.OnHealthChanged -= UpdateHealth;
        player.OnManaChanged -= UpdateMana;
    }
    
    protected void OnEnable()
    {
        DialogueManager.Instance.OnDialogueEnd += OnDialogueEnd;
    }


    protected void OnDisable()
    {
        DialogueManager.Instance.OnDialogueEnd -= OnDialogueEnd;
    }

    private void OnDialogueEnd(string dialogueID)
    {
        // 处理对话结束后的逻辑
        if (dialogueID == "homework_over")
        {
            luSleepAnimator.gameObject.SetActive(false);
            luWeekUpAnimator.gameObject.SetActive(true);
        }

        if (dialogueID == "fight_dialogue")
        {
            gameUIPanel.SetActive(true);
        }
    }

    public void PlaySceneAnimation()
    {
        if (sceneAnimator != null)
        {
            sceneAnimator.gameObject.SetActive(true);
            if (frontScene)
            {
                frontScene.SetActive(true);
            }
        }
        else
        {
            Debug.LogWarning("Scene Animator is not assigned.");
        }
    }
    
    public void StopSceneAnimation()
    {
        if (sceneAnimator != null)
        {
            sceneAnimator.StopPlayback();
            sceneAnimator.gameObject.SetActive(false);
        }
        else
        {
            Debug.LogWarning("Scene Animator is not assigned.");
        }
    }
    
    public void PlayLuSleepAnimation()
    {
        if (luSleepAnimator != null)
        {
            luSleepAnimator.gameObject.SetActive(true);
        }
        else
        {
            Debug.LogWarning("Lu Sleep Animator is not assigned.");
        }
    }
    
    // 在UI开始交互时调用
    public void SetInteractingWithUI(bool isInteracting)
    {
        IsInteractingWithUI = isInteracting;
    }
    
    private void OnSceneLoaded()
    {
        UpdateSceneName();
        
        // 短暂延迟以确保场景中所有对象都已加载
        StartCoroutine(DelayedPlayerConnect());
    }
    
    private IEnumerator DelayedPlayerConnect()
    {
        yield return new WaitForSeconds(0.2f);
        FindAndConnectPlayer();
    }

    private void FindAndConnectPlayer()
    {
        Debug.Log("Finding Player...");
        if (player != null)
        {
            // 注册事件
            player.OnHealthChanged += UpdateHealth;
            player.OnManaChanged += UpdateMana;
            
            // 初始化数值
            // UpdateHealth(player.playerData.CurrentHealth, false);
            UpdateMana(player.GetManaPercentage() * 100, 100);
            
            // 更新技能栏
            // if (playerCombat != null)
            // {
            //     UpdateSkillBar();
            // }
        }
    }

    private void UpdateSceneName()
    {
        sceneNameText.text = $"————{SceneManager.GetActiveScene().name}————";
    }
    
    private void UpdateHealth(int current, bool isHit)
    {
        //Debug.Log("Current Health: " + current);
        if (isHit)
        {
            // 生命值减少
            healthBarManager.ChangeHealthBar(current, true);
        }
        else
        {
            // 生命值增加
            healthBarManager.ChangeHealthBar(current, false);
        }
    }
    
    private void UpdateMana(float current, float max)
    {
        // manaSlider.value = current / max;
        // manaText.text = $"{Mathf.CeilToInt(current)}/{Mathf.CeilToInt(max)}";
    }
    
    private void InitializeSkillBar()
    {
        // 清除现有技能槽
        foreach (Transform child in skillBarContainer)
        {
            Destroy(child.gameObject);
        }
        skillSlotList.Clear();
        
        // 创建技能槽
        for (int i = 0; i < maxSkillSlots; i++)
        {
            SkillSlotUI slotObj = Instantiate(skillSlotPrefab, skillBarContainer).GetComponent<SkillSlotUI>();
            
            // 默认无技能状态
            slotObj.SkillIcon.gameObject.SetActive(false);
            
            // 设置快捷键文本
            slotObj.KeyText.text = (i + 1).ToString();
            
            // 设置数量文本
            slotObj.CountText.text = "10";
            
            skillSlotList.Add(slotObj);
        }
    }
    
    public void UpdateSkillBar()
    {
        // if (playerCombat == null) return;
        
        // Dictionary<ItemData, int> skills = playerCombat.GetSkills();
        int index = 0;
        
        // 重置所有技能槽
        foreach (var slot in skillSlotList)
        {
            slot.SkillIcon.gameObject.SetActive(false);
            slot.CountText.text = "0";
        }
        
        // 填充技能槽
        // foreach (var skillPair in skills)
        // {
        //     if (index >= skillSlotList.Count) break;
        //     
        //     SkillSlotUI slot = skillSlotList[index];
        //     slot.SkillIcon.sprite = skillPair.Key.icon;
        //     slot.SkillIcon.gameObject.SetActive(true);
        //     slot.CountText.text = skillPair.Value.ToString();
        //     
        //     index++;
        // }
    }
    
    private void OpenMenu()
    {
        MenuManager.Instance.ToggleMenu();
    }
    
    private void OpenInventory()
    {
        InventoryManager.Instance.ToggleInventory();
    }

    private void OpenNews()
    {
        NewsManager.Instance.ToggleNewsInfoBook();
    }
    
    private void AddUIPointerHandlers(GameObject uiElement)
    {
        // 添加事件触发器组件（如果没有）
        if (!uiElement.GetComponent<EventTrigger>())
            uiElement.AddComponent<EventTrigger>();
            
        EventTrigger trigger = uiElement.GetComponent<EventTrigger>();
        
        // 鼠标进入事件
        EventTrigger.Entry enterEntry = new EventTrigger.Entry();
        enterEntry.eventID = EventTriggerType.PointerEnter;
        enterEntry.callback.AddListener((data) => { SetInteractingWithUI(true); });
        trigger.triggers.Add(enterEntry);
        
        // 鼠标离开事件
        EventTrigger.Entry exitEntry = new EventTrigger.Entry();
        exitEntry.eventID = EventTriggerType.PointerExit;
        exitEntry.callback.AddListener((data) => { SetInteractingWithUI(false); });
        trigger.triggers.Add(exitEntry);
    }
    
    // 公共方法，允许其他脚本更新技能栏
    public void RefreshSkillBar()
    {
        UpdateSkillBar();
    }
}