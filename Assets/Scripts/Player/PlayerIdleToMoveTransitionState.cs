public class PlayerIdleToMoveTransitionState:PlayerGroundState
{
    public PlayerIdleToMoveTransitionState(Player player, PlayerStateMachine stateMachine, string animBoolName) 
        : base(player, stateMachine, animBoolName)
    {
    }
    
    public override void Enter()
    {
        base.Enter();
        StateTimer = Player.idleToMoveTransitionTime;
    }
    
    public override void Update()
    {
        base.Update();
        
        // 可以在这里添加过渡动画相关逻辑
        // 例如根据过渡时间调整速度
        float transitionSpeed = Player.moveSpeed * (1 - (StateTimer / 0.2f));
        Player.SetVelocity(xInput * transitionSpeed, Rb.linearVelocity.y);
        
        // 当过渡时间结束，切换到移动状态
        if (StateTimer <= 0)
        {
            StateMachine.ChangeState(Player.MoveState);
        }
        
        // 如果玩家在过渡期间停止输入，返回idle状态
        if (xInput == 0)
        {
            StateMachine.ChangeState(Player.IdleState);
        }
    }
    
    public override void Exit()
    {
        base.Exit();
    }
}