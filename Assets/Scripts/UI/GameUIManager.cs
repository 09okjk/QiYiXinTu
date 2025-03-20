using System.Collections;
using System.Collections.Generic;
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
    [SerializeField] private Image playerPortrait;
    [SerializeField] private Slider healthSlider;
    [SerializeField] private Slider manaSlider;
    [SerializeField] private TextMeshProUGUI healthText;
    [SerializeField] private TextMeshProUGUI manaText;
    
    [Header("Skill Bar")]
    [SerializeField] private Transform skillBarContainer;
    [SerializeField] private GameObject skillSlotPrefab;
    [SerializeField] private int maxSkillSlots = 6;
    
    [Header("Scene Info")]
    [SerializeField] private TextMeshProUGUI sceneNameText;
    [SerializeField] private Button menuButton;
    
    private PlayerHealth playerHealth;
    private PlayerCombat playerCombat;
    private List<SkillSlotUI> skillSlotList = new List<SkillSlotUI>();

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
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        
        if (playerHealth != null)
        {
            playerHealth.OnHealthChanged -= UpdateHealth;
            playerHealth.OnManaChanged -= UpdateMana;
        }
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
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        
        if (player != null)
        {
            playerHealth = player.GetComponent<PlayerHealth>();
            playerCombat = player.GetComponent<PlayerCombat>();
            
            if (playerHealth != null)
            {
                // 注册事件
                playerHealth.OnHealthChanged += UpdateHealth;
                playerHealth.OnManaChanged += UpdateMana;
                
                // 初始化数值
                UpdateHealth(playerHealth.GetHealthPercentage() * 100, 100);
                UpdateMana(playerHealth.GetManaPercentage() * 100, 100);
            }
            
            // 更新技能栏
            if (playerCombat != null)
            {
                UpdateSkillBar();
            }
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
        if (playerCombat == null) return;
        
        Dictionary<ItemData, int> skills = playerCombat.GetSkills();
        int index = 0;
        
        // 重置所有技能槽
        foreach (var slot in skillSlotList)
        {
            slot.SkillIcon.gameObject.SetActive(false);
            slot.CountText.text = "0";
        }
        
        // 填充技能槽
        foreach (var skillPair in skills)
        {
            if (index >= skillSlotList.Count) break;
            
            SkillSlotUI slot = skillSlotList[index];
            slot.SkillIcon.sprite = skillPair.Key.icon;
            slot.SkillIcon.gameObject.SetActive(true);
            slot.CountText.text = skillPair.Value.ToString();
            
            index++;
        }
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