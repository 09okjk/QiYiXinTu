using Manager;
using UnityEngine;

public class SkeletonGroundState: EnemyState
{
    protected Skeleton skeleton;
    protected Transform player;
    public SkeletonGroundState(Enemy enemyBase, EnemyStateMachine stateMachine, string animBoolName,Skeleton skeleton) : base(enemyBase, stateMachine, animBoolName)
    {
        this.skeleton = skeleton;
    }


    public override void Enter()
    {
        base.Enter();
        player = PlayerManager.Instance.player.transform;
    }

    public override void Update()
    {
        base.Update();
        
        if (skeleton.IsPlayerDetected())
        {
            stateMachine.ChangeState(skeleton.BattleState);
        }
    }

    public override void Exit()
    {
        base.Exit();
    }
}