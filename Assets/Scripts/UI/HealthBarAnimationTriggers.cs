using System;
using UnityEngine;

namespace UI
{
    public class HealthBarAnimationTriggers:MonoBehaviour
    {
        private HealthBarManager healthBarManager;

        private void Awake()
        {
            healthBarManager = GetComponentInParent<HealthBarManager>();
        }

        public void SetHealthBarSprite()
        {
            if (healthBarManager != null)
            {
                healthBarManager.SetHealthBarSprite();
            }
            else
            {
                Debug.LogWarning("HealthBarManager not found in parent.");
            }
        }
    }
}