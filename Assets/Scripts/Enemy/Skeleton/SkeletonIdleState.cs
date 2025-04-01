public class SkeletonIdleState: SkeletonGroundState
{
    public SkeletonIdleState(Enemy enemyBase, EnemyStateMachine stateMachine, string animBoolName, Skeleton skeleton) : base(enemyBase, stateMachine, animBoolName, skeleton)
    {
    }

    public override void Enter()
    {
        base.Enter();

        stateTimer = skeleton.idleTime;
    }

    public override void Update()
    {
        base.Update();
        
        if (stateTimer <= 0)
        {
            skeleton.Flip();
            stateMachine.ChangeState(skeleton.MoveState);
        }
    }

    public override void Exit()
    {
        base.Exit();
    }
}