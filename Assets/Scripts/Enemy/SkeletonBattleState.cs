using UnityEngine;

public class SkeletonBattleState: EnemyState
{
    private Transform player;
    private Enemy_Skeleton skeleton;
    private int moveDir = -1;
    public SkeletonBattleState(Enemy enemyBase, EnemyStateMachine stateMachine, string animBoolName,Enemy_Skeleton skeleton) 
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

        if (skeleton.IsPlayerDetected())
        {
            stateTimer = skeleton.battleTime;
            
            if (skeleton.IsPlayerInAttackRange() && CanAttack())
            {
                stateMachine.ChangeState(skeleton.AttackState);
            }
        }
        else
        {
            if (stateTimer < 0)
            {
                stateMachine.ChangeState(skeleton.IdleState);
            }
        }
        
        if (player.position.x < skeleton.transform.position.x)
        {
            moveDir = -1;
        }
        else if (player.position.x > skeleton.transform.position.x)
        {
            moveDir = 1;
        }
        skeleton.SetVelocity(skeleton.moveSpeed * moveDir, rb.linearVelocity.y);
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
        Debug.Log("attack cool down");
        return false;
    }
}