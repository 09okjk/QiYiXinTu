using Audio;

public class PlayerHurtState : PlayerState
{
    public PlayerHurtState(Player player, PlayerStateMachine stateMachine, string animBoolName) : base(player, stateMachine, animBoolName)
    {
    }

    public override void Enter()
    {
        base.Enter();
        
        StateTimer = Player.InvincibleTime;
        AudioManager.Instance.PlayEffectAudio("hurt_audio", false);    
    }

    public override void Update()
    {
        base.Update();
        
        if (StateTimer < 0 || TriggerCalled)
        {
            StateMachine.ChangeState(Player.IdleState);
        }
    }

    public override void Exit()
    {
        base.Exit();
    }
}