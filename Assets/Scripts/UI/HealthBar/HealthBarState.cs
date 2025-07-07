using UnityEngine;

namespace UI
{
    public class HealthBarState
    {
        protected HealthBarStateMachine StateMachine;
        protected Player Player;
        protected HealthBarManager manager;
        protected float StateTimer;
        protected bool TriggerCalled;
        
        private string _animBoolName;
        
        public HealthBarState(Player player, HealthBarManager manager,HealthBarStateMachine stateMachine, string animBoolName)
        {
            this.Player = player;
            this.manager = manager;
            this.StateMachine = stateMachine;
            this._animBoolName = animBoolName;
        }
        
        public virtual void Enter()
        {
            manager.HealthBarAnimator.SetBool(_animBoolName, true);
            TriggerCalled = false;
        }
        
        public virtual void Update()
        {
            StateTimer -= Time.deltaTime;
        }
        
        public virtual void Exit()
        {
            manager.HealthBarAnimator.SetBool(_animBoolName, false);
        }
        
        public virtual void AnimationFinishTrigger()
        {
            TriggerCalled = true;
        }
    }
}