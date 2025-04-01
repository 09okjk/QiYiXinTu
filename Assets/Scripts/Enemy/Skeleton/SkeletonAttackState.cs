using UnityEngine;

public class SkeletonAttackState: EnemyState
{
    Skeleton skeleton;
    public SkeletonAttackState(Enemy enemyBase, EnemyStateMachine stateMachine, string animBoolName,Skeleton skeleton) : base(enemyBase, stateMachine, animBoolName)
    {
        this.skeleton = skeleton;
    }


    public override void Enter()
    {
        base.Enter();
    }

    public override void Update()
    {
        base.Update();
        
        skeleton.SetZeroVelocity();

        if (triggerCalled)
        {
            stateMachine.ChangeState(skeleton.BattleState);
        }
    }

    public override void Exit()
    {
        base.Exit();
        
        skeleton.lastAttackTime = Time.time;
    }
}