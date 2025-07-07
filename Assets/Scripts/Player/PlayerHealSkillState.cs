using Manager;

public class PlayerHealSkillState:PlayerGroundState
{
    public PlayerHealSkillState(Player player, PlayerStateMachine stateMachine, string animBoolName) : base(player, stateMachine, animBoolName)
    {
    }

    public override void Enter()
    {
        base.Enter();
        
        StateTimer = 0.5f;
    }
    
    public override void Update()
    {
        base.Update();
        
        if (StateTimer < 0 && TriggerCalled)
        {
            StateMachine.ChangeState(Player.IdleState);
        }
    }
    public override void Exit()
    {
        base.Exit();
        SkillManager.Instance.healSkill.UseSkill();
    }
}