public class PlayerDeathState:PlayerState
{
    public PlayerDeathState(Player player, PlayerStateMachine stateMachine, string animBoolName) : base(player, stateMachine, animBoolName)
    {
    }

    public override void Enter()
    {
        base.Enter();
        StateTimer = 5f;
    }

    public override void Update()
    {
        base.Update();
        
        if (StateTimer < 0 || TriggerCalled)
        {
            Player.Die();
        }
    }

    public override void Exit()
    {
        base.Exit();
    }
}