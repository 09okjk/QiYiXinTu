using Audio;

public class PlayerDeathState:PlayerState
{
    public PlayerDeathState(Player player, PlayerStateMachine stateMachine, string animBoolName) : base(player, stateMachine, animBoolName)
    {
    }

    public override void Enter()
    {
        base.Enter();
        StateTimer = 5f;
        AudioManager.Instance.PlayEffectAudio("death_audio", false);
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