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
    }
    
    
    protected void OnEnable()
    {
        DialogueManager.Instance.OnDialogueEnd += OnDialogueEnd;
    }
    
    
    protected void OnDisable()
    {
        DialogueManager.Instance.OnDialogueEnd -= OnDialogueEnd;
    }
    
    protected override void FixedUpdate()
    {
        base.FixedUpdate();
        
        if (isFollowing)
          stateMachine.ChangeState(MoveState);
    }

    protected override void OnDialogueEnd(string dialogueID)
    {
        base.OnDialogueEnd(dialogueID);
        
        // 处理对话结束后的逻辑
        if (dialogueID == "lu_first_dialogue")
        {
            Anxious();
        }
    }

    public override void ActivateNpc()
    {
        base.ActivateNpc();
        
        stateMachine.Initialize(IdleState);
    }

	// 焦虑 Anxious
	public void Anxious()
    {
        stateMachine.ChangeState(AnxiousState);
	}
}