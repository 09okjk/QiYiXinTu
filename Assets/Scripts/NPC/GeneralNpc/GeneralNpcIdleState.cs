public class GeneralNpcIdleState : GeneralNpcGroundState
{
    public GeneralNpcIdleState(NPC npc, NPCStateMachine stateMachine, string animBoolName, GeneralNpc generalNpc) : base(npc, stateMachine, animBoolName, generalNpc)
    {
    }
    
    public override void Enter()
    {
        base.Enter();

        GeneralNpc.SetZeroVelocity();
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