using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LuXinsheng : NPC
{
    [Header("LuXinsheng特殊设置")]
    [SerializeField] private string[] specialScenes = { "outside1" }; // 特殊场景列表
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
            if (DialogueManager.Instance != null)
            {
                DialogueManager.Instance.StartDialogueByID("fight_dialogue");
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
        if (!GetNpcGameData().isFollowing) return;

        try
        {
            if (GetNpcGameData().followSpeed == 0)
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
        if (dialogueID == "lu_first_dialogue")
        {
            HandleFirstDialogueEnd();
        }
        else if (dialogueID == "fight_dialogue")
        {
            HandleFightDialogueEnd();
        }
        else if (dialogueID == "lide_dialogue")
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

