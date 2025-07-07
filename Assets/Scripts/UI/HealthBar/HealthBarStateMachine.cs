using UnityEngine;

namespace UI
{
    public class HealthBarStateMachine
    {
        public HealthBarState CurrentState;

        public void Initialize(HealthBarState state)
        {
            CurrentState = state;

            CurrentState.Enter();
        }
        
        public void ChangeState(HealthBarState newState)
        {
            Debug.LogWarning("Changing HealthBarState from " + CurrentState.GetType().Name + " to " + newState.GetType().Name);
            CurrentState.Exit();
            CurrentState = newState;
            CurrentState.Enter();
        }
    }
}