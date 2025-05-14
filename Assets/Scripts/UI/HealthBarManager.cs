using System;
using System.Collections.Generic;
using Manager;
using UnityEngine;

namespace UI
{
    public class HealthBarManager:MonoBehaviour
    {
        public SpriteRenderer healthBar;
        public int currentHealth;
        public Animator HealthBarAnimator;
        public Animator HealthBarFXAnimator;
        public List<Sprite> healthBarSprites = new List<Sprite>();

        private void Start()
        {
            currentHealth = PlayerManager.Instance.player.playerData.CurrentHealth;
            SetHealthBarSprite();
            HealthBarAnimator.gameObject.SetActive(false);
        }
        
        private void Update()
        {
            if (currentHealth != PlayerManager.Instance.player.playerData.CurrentHealth)
            {
                currentHealth = PlayerManager.Instance.player.playerData.CurrentHealth;
                SetHealthBarSprite();
            }
        }

        public void ChangeHealthBar (int health, bool isHit)
        {
            if (isHit && health >= 0)
            {
                healthBar.sprite = null;
                HealthBarAnimator.gameObject.SetActive(true);
                HealthBarFXAnimator.gameObject.SetActive(true);
                Debug.Log("Current Health: " + health);
                currentHealth = health;
                HealthBarAnimator.SetInteger("Health", health);
                HealthBarFXAnimator.SetInteger("Health", health);
            }
            else if (!isHit && health is <= 5 and > 1)
            {
                healthBar.sprite = null;
                HealthBarAnimator.gameObject.SetActive(true);
                HealthBarFXAnimator.gameObject.SetActive(false);  
                currentHealth = health;
                health = 0 - health;
                HealthBarAnimator.SetInteger("Health", health);
            }
        }

        public void SetHealthBarSprite()
        {
            if( currentHealth > 0)
                healthBar.sprite = healthBarSprites[5-currentHealth];
            // HealthBarAnimator.gameObject.SetActive(false);
        }
    }
}