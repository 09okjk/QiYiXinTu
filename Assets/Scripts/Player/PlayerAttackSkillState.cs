using Audio;
using Manager;

public class PlayerAttackSkillState: PlayerGroundState
{
    public PlayerAttackSkillState(Player player, PlayerStateMachine stateMachine, string animBoolName) : base(player, stateMachine, animBoolName)
    {
    }

    public override void Enter()
    {
        base.Enter();
        StateTimer = 0.1f;
        AudioManager.Instance.PlayEffectAudio("skill_attack_audio", false);
        SkillManager.Instance.attackSkill.UseSkill();
    }

    public override void Update()
    {
        base.Update();
        
        if (TriggerCalled && StateTimer < 0)
        {
            StateMachine.ChangeState(Player.IdleState);
        }
    }
}