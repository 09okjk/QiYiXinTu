public class LuXinshengSleepState: LuXinshengGroundState
{
    public LuXinshengSleepState(NPC npc, NPCStateMachine stateMachine, string animBoolName, LuXinsheng luXinsheng) : base(npc, stateMachine, animBoolName, luXinsheng)
    {
    }
    
    public override void Enter()
    {
        base.Enter();
        
        LuXinsheng.SetZeroVelocity();
    }
    
    public override void Update()
    {
        base.Update();
    }
    
    public override void Exit()
    {
        base.Exit();
    }
}