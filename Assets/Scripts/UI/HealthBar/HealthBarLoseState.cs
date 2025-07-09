namespace UI
{
    public class HealthBarLoseState: HealthBarState
    {
        private int currentHealth;

        public HealthBarLoseState(Player player, HealthBarManager manager, HealthBarStateMachine stateMachine, string animBoolName) : base(player, manager, stateMachine, animBoolName)
        {
        }
        
        public override void Enter()
        {
            base.Enter();
            currentHealth = Player.playerData.CurrentHealth;
            
            manager.HealthBarAnimator.SetInteger("AfterLose", currentHealth);

            StateTimer = .1f;
        }

        public override void Update()
        {
            base.Update();
            if (TriggerCalled)
            {
                StateMachine.ChangeState(manager.HealthBarNormalState);
            }
        }

        public override void Exit()
        {
            base.Exit();
        }
    }
}