using UnityEngine;

public class SkeletonBattleState: EnemyState
{
    private Transform player;
    private Skeleton skeleton;
    private int moveDir = -1;
    public SkeletonBattleState(Enemy enemyBase, EnemyStateMachine stateMachine, string animBoolName,Skeleton skeleton) 
        : base(enemyBase, stateMachine, animBoolName)
    {
        this.skeleton = skeleton;
    }


    public override void Enter()
    {
        base.Enter();
        player = GameObject.Find("Player").transform;
    }

    public override void Update()
    {
        base.Update();

        if (player.position.x < skeleton.transform.position.x)
        {
            moveDir = -1;
        }
        else if (player.position.x > skeleton.transform.position.x)
        {
            moveDir = 1;
        }
        
        if (skeleton.IsPlayerDetected())
        {
            stateTimer = skeleton.battleTime;
            
            if (skeleton.IsPlayerInAttackRange())
            {
                if (CanAttack())
                    stateMachine.ChangeState(skeleton.AttackState);
            }
            else
            {
                skeleton.SetVelocity(skeleton.moveSpeed * moveDir, rb.linearVelocity.y);
            }
        }
        else
        {
            if (stateTimer < 0)
            {
                stateMachine.ChangeState(skeleton.IdleState);
            }
            skeleton.SetVelocity(skeleton.moveSpeed * moveDir, rb.linearVelocity.y);
        }
        
    }

    public override void Exit()
    {
        base.Exit();
    }

    private bool CanAttack()
    {
        if (Time.time >= skeleton.lastAttackTime + skeleton.attackCoolDown)
        {
            skeleton.lastAttackTime = Time.time;
            return true;
        }
        return false;
    }
}