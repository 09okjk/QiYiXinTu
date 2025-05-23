public class LuXinsheng:NPC
{
    # region States
    internal LuXinshengIdleState IdleState { get; set; }
    internal LuXinshengMoveState MoveState { get; set; }
    internal LuXinshengHomeworkState HomeworkState { get; set; }
    internal LuXinshengSleepState SleepState { get; set; }
    internal LuXinshengWeekUpState WeekUpState { get; set; }
    internal LuXinshengShockedState ShockedState { get; set; }
    
    # endregion
    
    protected override void Awake()
    {
        base.Awake();

        IdleState = new LuXinshengIdleState(this, stateMachine, "Idle", this);
        MoveState = new LuXinshengMoveState(this, stateMachine, "Move", this);
        HomeworkState = new LuXinshengHomeworkState(this, stateMachine, "Homework", this);
        SleepState = new LuXinshengSleepState(this, stateMachine, "Sleep", this);
        WeekUpState = new LuXinshengWeekUpState(this, stateMachine, "WeekUp", this);
        ShockedState = new LuXinshengShockedState(this, stateMachine, "Shocked", this);
    }

    protected override void Start()
    {
        base.Start();
        stateMachine.Initialize(IdleState);
    }
    
    protected override void FixedUpdate()
    {
        base.FixedUpdate();
        
        if (isFollowing)
          stateMachine.ChangeState(MoveState);
    }
    
    public void Homework()
    {
        stateMachine.ChangeState(HomeworkState);
    }
    
    public void Sleep()
    {
        stateMachine.ChangeState(SleepState);
    }
    
    public void WeekUp()
    {
        stateMachine.ChangeState(WeekUpState);
    }
    
    public void Shocked()
    {
        stateMachine.ChangeState(ShockedState);
    }
}