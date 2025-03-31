public class SkeletonMoveState: SkeletonGroundState
{
    public SkeletonMoveState(Enemy enemyBase, EnemyStateMachine stateMachine, string animBoolName, Enemy_Skeleton skeleton) : base(enemyBase, stateMachine, animBoolName, skeleton)
    {
    }

    public override void Enter()
    {
        base.Enter();
    }

    public override void Update()
    {
        base.Update();
        
        skeleton.SetVelocity(skeleton.moveSpeed * skeleton.FacingDirection, rb.linearVelocity.y);
        
        if (skeleton.IsWallDetected() || !skeleton.IsGroundDetected())
        {
            stateMachine.ChangeState(skeleton.IdleState);
        }
    }

    public override void Exit()
    {
        base.Exit();
    }
}