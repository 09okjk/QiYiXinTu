public class LuXinshengAnxiousState : LuXinshengGroundState
{
    public LuXinshengAnxiousState(NPC npc, NPCStateMachine stateMachine, string animBoolName, LuXinsheng luXinsheng) : base(npc, stateMachine, animBoolName, luXinsheng)
    {
    }
    
    public override void Enter()
    {
        base.Enter();
        
        LuXinsheng.SetZeroVelocity();
        StateTimer = 0.1f;
    }
    
    public override void Update()
    {
        base.Update();
        
        if (StateTimer <= 0 && TriggerCalled)
        {
            // 触发动画结束事件
            Npc.Anim.SetTrigger("anxiousEnd");
        }
    }
    
    public override void Exit()
    {
        base.Exit();
    }
}
