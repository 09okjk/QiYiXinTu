using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LuXinsheng:NPC
{
    # region States
    internal LuXinshengIdleState IdleState { get; set; }
    internal LuXinshengMoveState MoveState { get; set; }
    internal LuXinshengAnxiousState AnxiousState { get; set; }
    
    # endregion
    
    protected override void Awake()
    {
        base.Awake();

        IdleState = new LuXinshengIdleState(this, stateMachine, "Idle", this);
        MoveState = new LuXinshengMoveState(this, stateMachine, "Move", this);
        AnxiousState = new LuXinshengAnxiousState(this, stateMachine, "Anxious", this);
    }

    protected override void Start()
    {
        base.Start();
        EnemyManager.Instance.OnEnemyActivatedByType += OnEnemyActivatedByType;
    }

    private void OnDestroy()
    {
        EnemyManager.Instance.OnEnemyActivatedByType -= OnEnemyActivatedByType;
    }

    protected void OnEnable()
    {
        DialogueManager.Instance.OnDialogueEnd += OnDialogueEnd;
    }
    
    
    protected void OnDisable()
    {
        DialogueManager.Instance.OnDialogueEnd -= OnDialogueEnd;
    }

    private void OnEnemyActivatedByType(EnemyType obj)
    {
        Debug.Log("Enemy activated: " + obj);
        // 检查是否是 Enemy1 类型的敌人被激活
        if (obj == EnemyType.Enemy1)
        {
            // 如果是 Enemy1，则切换到 MoveState
            stateMachine.ChangeState(MoveState);
            FollowTargetPlayer();
            DialogueManager.Instance.StartDialogueByID("fight_dialogue");
        }
    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();

        if (isFollowing)
        {
            if (followSpeed == 0)
            {
                // 如果跟随速度为0，切换到 IdleState
                stateMachine.ChangeState(IdleState);
            }
            else
            {
                // 如果跟随速度不为0，切换到 MoveState
                stateMachine.ChangeState(MoveState);
            }
        }
        
        
    }

    public override void DeactivateNpc()
    {
        base.DeactivateNpc();
        if (SceneManager.GetActiveScene().name == "outside1")
        {
            ActivateNpc();
        }
    }

    protected override void OnDialogueEnd(string dialogueID)
    {
        base.OnDialogueEnd(dialogueID);
        
        // 处理对话结束后的逻辑
        if (dialogueID == "lu_first_dialogue")
        {
            Anxious();
        }

        if (dialogueID == "fight_dialogue")
        {
            FollowTargetPlayer();
        }
        
        if (dialogueID == "lide_dialogue")
        {
            // 处理开场对话结束后的逻辑
            Debug.Log("开场对话结束，LuXinsheng 进入 Idle 状态");
            stateMachine.ChangeState(IdleState);
            FollowTargetPlayer();
        }
    }

    public override void ActivateNpc()
    {
        base.ActivateNpc();
        
        // 测试用-------
        //if (SceneManager.GetActiveScene().name == "outside1")
        //    DialogueManager.Instance.StartDialogueByID("lide_dialogue");
        // 测试用-------
        
        stateMachine.Initialize(IdleState);
    }

	// 焦虑 Anxious
	public void Anxious()
    {
        Debug.Log("LuXinsheng is anxious.");
        stateMachine.ChangeState(AnxiousState);
	}
}