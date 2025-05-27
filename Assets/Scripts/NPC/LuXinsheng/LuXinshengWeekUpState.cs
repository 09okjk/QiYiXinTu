using UnityEngine;

public class LuXinshengWeekUpState: LuXinshengGroundState
{
    public LuXinshengWeekUpState(NPC npc, NPCStateMachine stateMachine, string animBoolName, LuXinsheng luXinsheng) : base(npc, stateMachine, animBoolName, luXinsheng)
    {
    }
    
    public override void Enter()
    {
        base.Enter();
        Debug.Log("LuXinshengWeekUpState Enter");
        LuXinsheng.SetZeroVelocity();
    }
    
    public override void Update()
    {
        base.Update();
        //
        // Debug.Log("LuXinshengWeekUpState Update");
        // Debug.Log("TriggerCalled: " + TriggerCalled);
        // if (TriggerCalled)
        // {
        //     // 触发对话
        //     DialogueManager.Instance.StartDialogueByID("lu_first_dialogue");
        //     // 切换到Idle状态
        //     LuXinsheng.stateMachine.ChangeState(LuXinsheng.IdleState);
        // }
    }
    
    public override void Exit()
    {
        base.Exit();
    }
}