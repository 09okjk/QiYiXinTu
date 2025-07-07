namespace UI
{
    public class HealthBarNormalState:HealthBarState
    {
        private int currentHealth;
        public HealthBarNormalState(Player player, HealthBarManager manager, HealthBarStateMachine stateMachine, string animBoolName) : base(player, manager, stateMachine, animBoolName)
        {
        }

        public override void Enter()
        {
            base.Enter();
            currentHealth = Player.playerData.CurrentHealth;
        }
        
        public override void Update()
        {
            base.Update();
            manager.SetHealthBarSprite();
        }
        
        public override void Exit()
        {
            base.Exit();
        }
    }
}