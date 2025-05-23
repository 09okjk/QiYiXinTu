public class LuXinshengIdleState : LuXinshengGroundState
{
    public LuXinshengIdleState(NPC npc, NPCStateMachine stateMachine, string animBoolName, LuXinsheng luXinsheng) : base(npc, stateMachine, animBoolName, luXinsheng)
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