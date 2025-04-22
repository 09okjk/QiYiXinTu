using System.Collections;
using System.Collections.Generic;
using Manager;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UI;
using UnityEngine.SceneManagement;

public class GameUIManager : MonoBehaviour
{
    public static GameUIManager Instance { get; private set; }
    
    [SerializeField] GameObject gameUIPanel;

    [Header("Player Status")]
    [SerializeField] private Image playerPortrait; // 玩家头像
    [SerializeField] private Slider healthSlider; // 生命值滑块
    [SerializeField] private Slider manaSlider; // 魔法值滑块
    [SerializeField] private TextMeshProUGUI healthText; // 生命值文本
    [SerializeField] private TextMeshProUGUI manaText; // 魔法值文本
    
    [Header("Skill Bar")]
    [SerializeField] private Transform skillBarContainer;
    [SerializeField] private GameObject skillSlotPrefab;
    [SerializeField] private int maxSkillSlots = 6;
    
    [Header("Scene Info")]
    [SerializeField] private TextMeshProUGUI sceneNameText;
    [SerializeField] private Button menuButton;
    
    // private PlayerHealth playerHealth; 
    // private PlayerCombat playerCombat;
    private List<SkillSlotUI> skillSlotList = new List<SkillSlotUI>();

    private Player player;
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // 显示游戏UI
        gameUIPanel.SetActive(true);
        
        // 初始化技能栏
        InitializeSkillBar();
        
        // 设置菜单按钮监听
        menuButton.onClick.AddListener(OpenMenu);
        
        // 更新场景名称
        UpdateSceneName();
        
        // 场景加载事件注册
        SceneManager.sceneLoaded += OnSceneLoaded;
        
        // 查找并连接玩家
        FindAndConnectPlayer();
        
        // 显示任务日志面板
        QuestManager.Instance.ToggleQuestLog();
        
        player = PlayerManager.Instance.player;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        
        player.OnHealthChanged -= UpdateHealth;
        player.OnManaChanged -= UpdateMana;
    }
    
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
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
        if (player != null)
        {
            // 注册事件
            player.OnHealthChanged += UpdateHealth;
            player.OnManaChanged += UpdateMana;
            
            // 初始化数值
            UpdateHealth(player.GetHealthPercentage() * 100, 100);
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
        sceneNameText.text = SceneManager.GetActiveScene().name;
    }
    
    private void UpdateHealth(float current, float max)
    {
        healthSlider.value = current / max;
        healthText.text = $"{Mathf.CeilToInt(current)}/{Mathf.CeilToInt(max)}";
    }
    
    private void UpdateMana(float current, float max)
    {
        manaSlider.value = current / max;
        manaText.text = $"{Mathf.CeilToInt(current)}/{Mathf.CeilToInt(max)}";
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
    
    // 公共方法，允许其他脚本更新技能栏
    public void RefreshSkillBar()
    {
        UpdateSkillBar();
    }
}