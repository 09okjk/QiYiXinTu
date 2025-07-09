using System;
using UnityEngine;

namespace UI
{
    public class HealthBarAnimationTriggers : MonoBehaviour
    {
        private HealthBarManager healthBarManager;

        private void Awake()
        {
            healthBarManager = GetComponentInParent<HealthBarManager>();
            
            if (healthBarManager == null)
            {
                Debug.LogError("HealthBarManager not found in parent objects! Make sure this component is a child of HealthBarManager.");
            }
        }

        public void SetHealthBarSprite()
        {
            healthBarManager.StateMachine.CurrentState.AnimationFinishTrigger();
        }
    }
}