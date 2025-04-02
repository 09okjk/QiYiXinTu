public class SkeletonHurtState : EnemyState
{
    private Skeleton skeleton;
    public SkeletonHurtState(Enemy enemyBase, EnemyStateMachine stateMachine, string animBoolName,Skeleton skeleton) : base(enemyBase, stateMachine, animBoolName)
    {
        this.skeleton = skeleton;
    }

    public override void Enter()
    {
        base.Enter();
        stateTimer = skeleton.InvincibleTime;
    }

    public override void Update()
    {
        base.Update();
        
        if (stateTimer < 0 && triggerCalled)
        {
            stateMachine.ChangeState(skeleton.IdleState);
        }
        
    }

    public override void Exit()
    {
        base.Exit();
    }
}