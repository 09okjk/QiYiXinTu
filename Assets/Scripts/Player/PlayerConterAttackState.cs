using UnityEngine;

public class PlayerConterAttackState: PlayerState
{
    public PlayerConterAttackState(Player player, PlayerStateMachine stateMachine, string animBoolName) : base(player, stateMachine, animBoolName)
    {
    }

    public override void Enter()
    {
        base.Enter();

        StateTimer = Player.counterAttackDuration;
        Player.Anim.SetBool("SuccessfulCounterAttack", false);
    }

    public override void Update()
    {
        base.Update();
        
        Player.SetZeroVelocity();
        
        Collider2D[] colliders = Physics2D.OverlapCircleAll(Player.attackCheck.position, Player.attackCheckRadius, Player.whatIsEnemy);
        foreach (Collider2D collider in colliders)
        {
            if (collider.TryGetComponent<Enemy>(out Enemy enemy))
            {
                if(enemy.CanBeStunned())
                {
                    StateTimer = 10;// 只是为了让动画播放完
                    Player.Anim.SetBool("SuccessfulCounterAttack", true);
                }
            }
        }

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