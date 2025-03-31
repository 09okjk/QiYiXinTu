public class SkeletonGroundState: EnemyState
{
    protected Enemy_Skeleton skeleton;
    public SkeletonGroundState(Enemy enemyBase, EnemyStateMachine stateMachine, string animBoolName,Enemy_Skeleton skeleton) : base(enemyBase, stateMachine, animBoolName)
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