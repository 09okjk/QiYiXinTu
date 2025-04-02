using Manager;

public class PlayerThrowSwordState: PlayerState
{
    public PlayerThrowSwordState(Player player, PlayerStateMachine stateMachine, string animBoolName) : base(player, stateMachine, animBoolName)
    {
    }

    public override void Enter()
    {
        base.Enter();
    }

    public override void Update()
    {
        base.Update();
        
        
        if (TriggerCalled)
        {
            SkillManager.Instance.swordSkill.CreateSword();
            Player.skillManager.swordSkill.DotsActive(false);
            StateMachine.ChangeState(Player.IdleState);
        }
    }

    public override void Exit()
    {
        base.Exit();
    }
}