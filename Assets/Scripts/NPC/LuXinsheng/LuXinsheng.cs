using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LuXinsheng : NPC
{
    [Header("LuXinsheng特殊设置")]
    [SerializeField] private string[] specialScenes = { "outside1" }; // 特殊场景列表
    [SerializeField] private LuXinshengDialogueConfig dialogueConfig; // 对话配置
    
    #region States
    internal LuXinshengIdleState IdleState { get; set; }
    internal LuXinshengMoveState MoveState { get; set; }
    internal LuXinshengAnxiousState AnxiousState { get; set; }
    #endregion
    
    private bool hasSubscribedToEnemyEvents = false;

    #region Unity生命周期

    protected override void Awake()
    {
        base.Awake();

        InitializeStates();
        LoadDialogueConfig();
    }

    protected override void Start()
    {
        base.Start();
        SubscribeToEnemyEvents();
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        UnsubscribeFromEnemyEvents();
    }

    #endregion

    #region 初始化

    private void InitializeStates()
    {
        IdleState = new LuXinshengIdleState(this, stateMachine, "Idle", this);
        MoveState = new LuXinshengMoveState(this, stateMachine, "Move", this);
        AnxiousState = new LuXinshengAnxiousState(this, stateMachine, "Anxious", this);
    }

    private void LoadDialogueConfig()
    {
        if (dialogueConfig == null)
        {
            // 尝试从Resources加载配置
            dialogueConfig = Resources.Load<LuXinshengDialogueConfig>("ScriptableObjects/NPCs/LuXinshengDialogueConfig");
            
            if (dialogueConfig == null)
            {
                Debug.LogWarning("未找到LuXinsheng对话配置，使用默认设置");
                CreateDefaultDialogueConfig();
            }
        }
    }

    private void CreateDefaultDialogueConfig()
    {
        // 创建默认配置以避免硬编码
        dialogueConfig = ScriptableObject.CreateInstance<LuXinshengDialogueConfig>();
        dialogueConfig.firstDialogueID = "lu_first_dialogue";
        dialogueConfig.fightDialogueID = "fight_dialogue";
        dialogueConfig.lideDialogueID = "lide_dialogue";
    }

    #endregion

    #region 事件管理

    private void SubscribeToEnemyEvents()
    {
        try
        {
            if (EnemyManager.Instance != null)
            {
                EnemyManager.Instance.OnEnemyActivatedByType += OnEnemyActivatedByType;
                hasSubscribedToEnemyEvents = true;
                Debug.Log("LuXinsheng 成功订阅敌人事件");
            }
            else
            {
                Debug.LogWarning("EnemyManager.Instance 为空，无法订阅敌人事件");
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"订阅敌人事件时发生错误: {e.Message}");
        }
    }

    private void UnsubscribeFromEnemyEvents()
    {
        try
        {
            if (hasSubscribedToEnemyEvents && EnemyManager.Instance != null)
            {
                EnemyManager.Instance.OnEnemyActivatedByType -= OnEnemyActivatedByType;
                hasSubscribedToEnemyEvents = false;
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"取消订阅敌人事件时发生错误: {e.Message}");
        }
    }

    #endregion

    #region 敌人事件处理

    private void OnEnemyActivatedByType(EnemyType enemyType)
    {
        Debug.Log($"敌人激活: {enemyType}");
        
        if (enemyType == EnemyType.Enemy1)
        {
            HandleEnemy1Activation();
        }
    }

    private void HandleEnemy1Activation()
    {
        try
        {
            // 切换到移动状态
            stateMachine.ChangeState(MoveState);
            
            // 开始跟随玩家
            FollowTargetPlayer();
            
            // 触发战斗对话
            if (DialogueManager.Instance != null && dialogueConfig != null)
            {
                DialogueManager.Instance.StartDialogueByID(dialogueConfig.fightDialogueID);
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"处理Enemy1激活时发生错误: {e.Message}");
        }
    }

    #endregion

    #region 状态更新

    protected override void FixedUpdate()
    {
        base.FixedUpdate();

        UpdateMovementState();
    }

    private void UpdateMovementState()
    {
        if (!isFollowing) return;

        try
        {
            if (followSpeed == 0)
            {
                // 跟随速度为0时，切换到空闲状态
                if (stateMachine.CurrentState != IdleState)
                {
                    stateMachine.ChangeState(IdleState);
                }
            }
            else
            {
                // 跟随速度不为0时，切换到移动状态
                if (stateMachine.CurrentState != MoveState)
                {
                    stateMachine.ChangeState(MoveState);
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"更新移动状态时发生错误: {e.Message}");
        }
    }

    #endregion

    #region 激活/禁用逻辑

    public override void DeactivateNpc()
    {
        base.DeactivateNpc();
        
        // 检查是否在特殊场景中需要重新激活
        if (ShouldReactivateInCurrentScene())
        {
            ActivateNpc();
        }
    }

    private bool ShouldReactivateInCurrentScene()
    {
        string currentScene = SceneManager.GetActiveScene().name;
        
        foreach (string specialScene in specialScenes)
        {
            if (currentScene == specialScene)
            {
                return true;
            }
        }
        
        return false;
    }

    public override void ActivateNpc()
    {
        base.ActivateNpc();
        
        // 初始化状态机
        stateMachine.Initialize(IdleState);
        
        Debug.Log("LuXinsheng 激活并初始化为空闲状态");
    }

    #endregion

    #region 对话处理

    protected override void OnDialogueEnd(string dialogueID)
    {
        base.OnDialogueEnd(dialogueID);
        
        if (dialogueConfig == null)
        {
            Debug.LogWarning("对话配置未加载，无法处理对话结束");
            return;
        }

        try
        {
            HandleSpecificDialogue(dialogueID);
        }
        catch (Exception e)
        {
            Debug.LogError($"处理对话结束时发生错误: {e.Message}");
        }
    }

    private void HandleSpecificDialogue(string dialogueID)
    {
        if (dialogueID == dialogueConfig.firstDialogueID)
        {
            HandleFirstDialogueEnd();
        }
        else if (dialogueID == dialogueConfig.fightDialogueID)
        {
            HandleFightDialogueEnd();
        }
        else if (dialogueID == dialogueConfig.lideDialogueID)
        {
            HandleLideDialogueEnd();
        }
    }

    private void HandleFirstDialogueEnd()
    {
        Debug.Log("首次对话结束，LuXinsheng 进入焦虑状态");
        Anxious();
    }

    private void HandleFightDialogueEnd()
    {
        Debug.Log("战斗对话结束，LuXinsheng 开始跟随玩家");
        FollowTargetPlayer();
    }

    private void HandleLideDialogueEnd()
    {
        Debug.Log("开场对话结束，LuXinsheng 进入空闲状态并开始跟随");
        stateMachine.ChangeState(IdleState);
        FollowTargetPlayer();
    }

    #endregion

    #region 特殊行为

    public void Anxious()
    {
        try
        {
            Debug.Log("LuXinsheng 进入焦虑状态");
            stateMachine.ChangeState(AnxiousState);
        }
        catch (Exception e)
        {
            Debug.LogError($"切换到焦虑状态时发生错误: {e.Message}");
        }
    }

    #endregion

    #region 调试

    [ContextMenu("强制进入焦虑状态")]
    private void DebugAnxious()
    {
        Anxious();
    }

    [ContextMenu("强制开始跟随")]
    private void DebugStartFollowing()
    {
        FollowTargetPlayer();
    }

    #endregion
}

/// <summary>
/// LuXinsheng对话配置
/// </summary>
[CreateAssetMenu(fileName = "LuXinshengDialogueConfig", menuName = "Characters/LuXinsheng Dialogue Config")]
public class LuXinshengDialogueConfig : ScriptableObject
{
    [Header("对话ID配置")]
    public string firstDialogueID = "lu_first_dialogue";
    public string fightDialogueID = "fight_dialogue";
    public string lideDialogueID = "lide_dialogue";
    
    [Header("特殊场景配置")]
    public string[] specialScenes = { "outside1" };
}