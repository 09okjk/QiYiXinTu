using System;
using System.Collections;
using System.Collections.Generic;
using Manager;
using UnityEngine;

namespace UI
{
    public class HealthBarManager : MonoBehaviour
    {
        public SpriteRenderer healthBar;
        public Animator HealthBarAnimator;
        public Animator HealthBarFXAnimator;
        public List<Sprite> healthBarSprites = new List<Sprite>();
        
        public HealthBarStateMachine StateMachine;
        public HealthBarAddState HealthBarAddState;
        public HealthBarLoseState HealthBarLoseState;
        public HealthBarNormalState HealthBarNormalState;

        protected void Awake()
        {
            StateMachine = new HealthBarStateMachine();
            
            HealthBarAddState = new HealthBarAddState(PlayerManager.Instance.player,this, StateMachine, "Add");
            HealthBarLoseState = new HealthBarLoseState(PlayerManager.Instance.player, this, StateMachine, "Lose");
            HealthBarNormalState = new HealthBarNormalState(PlayerManager.Instance.player, this, StateMachine, "Normal");
        }

        protected void Start()
        {
            StateMachine.Initialize(HealthBarNormalState);
            HealthBarFXAnimator.gameObject.SetActive(false);
        }

        protected void Update()
        {
            StateMachine.CurrentState.Update();
        }

        public void ChangeHealthBar(int currentHealth,bool isHit)
        {
            healthBar.sprite = null; // 清除当前血条精灵
            if (isHit)
            {
                StateMachine.ChangeState(HealthBarLoseState);
                HealthBarFXAnimator.gameObject.SetActive(true);
                HealthBarFXAnimator.SetInteger("Health", currentHealth);
            }else
            {
                StateMachine.ChangeState(HealthBarAddState);
                HealthBarFXAnimator.gameObject.SetActive(false);
            }
        }
        
        public void SetHealthBarSprite()
        {
            var currentHealth = PlayerManager.Instance.player.playerData.CurrentHealth;
            HealthBarAnimator.SetInteger("Health", currentHealth);
            int spriteIndex = 5 - currentHealth;
            Debug.LogWarning("spriteIndex: " + spriteIndex);
            healthBar.sprite = healthBarSprites[spriteIndex];
        }
        //
        // [Header("调试设置")]
        // public bool enableDebugLogs = true;
        //
        // private bool isPlayingAnimation = false;
        // private int lastKnownHealth;
        //
        // public int currentHealth => PlayerManager.Instance.player.playerData.CurrentHealth;
        //
        // private void Start()
        // {
        //     lastKnownHealth = currentHealth;
        //     SetHealthBarSprite();
        //     HealthBarAnimator.gameObject.SetActive(false);
        //     HealthBarFXAnimator.gameObject.SetActive(false);
        //     
        //     if (enableDebugLogs)
        //     {
        //         Debug.Log($"[血条管理] 初始化完成，当前血量: {currentHealth}");
        //     }
        // }
        //
        // private void Update()
        // {
        //     if (!isPlayingAnimation && currentHealth != lastKnownHealth)
        //     {
        //         int newHealth = currentHealth;
        //         bool isHit = newHealth < lastKnownHealth;
        //         
        //         if (enableDebugLogs)
        //         {
        //             Debug.Log($"[血条管理] 检测到血量变化: {lastKnownHealth} -> {newHealth}, 是否受伤: {isHit}");
        //         }
        //         
        //         ChangeHealthBar(newHealth, isHit);
        //         lastKnownHealth = newHealth;
        //     }
        // }
        //
        // public void ChangeHealthBar(int health, bool isHit)
        // {
        //     if (health < 0 || health >= 5) 
        //     {
        //         if (enableDebugLogs)
        //             Debug.LogWarning($"[血条管理] 血量值超出范围: {health}");
        //         return;
        //     }
        //     
        //     PlayHealthAnimation(health, isHit);
        // }
        //
        // private void PlayHealthAnimation(int targetHealth, bool isHit)
        // {
        //     if (enableDebugLogs)
        //     {
        //         Debug.Log($"[血条管理] 开始播放动画 - 目标血量: {targetHealth}, 是否受伤: {isHit}");
        //     }
        //     
        //     isPlayingAnimation = true;
        //     healthBar.sprite = null;
        //     
        //     if (isHit)
        //     {
        //         HealthBarAnimator.gameObject.SetActive(true);
        //         HealthBarFXAnimator.gameObject.SetActive(true);
        //         
        //         if (enableDebugLogs)
        //         {
        //             Debug.Log($"[血条管理] 受伤动画 - 设置Health参数为: {targetHealth}");
        //             Debug.Log($"[血条管理] 动画器状态 - 主动画器激活: {HealthBarAnimator.gameObject.activeInHierarchy}, FX动画器激活: {HealthBarFXAnimator.gameObject.activeInHierarchy}");
        //         }
        //         
        //         HealthBarAnimator.SetInteger("Health", targetHealth);
        //         HealthBarFXAnimator.SetInteger("Health", targetHealth);
        //         
        //         // 立即检查动画器状态
        //         StartCoroutine(CheckAnimatorStateAfterTrigger(HealthBarAnimator, "主动画器"));
        //         StartCoroutine(CheckAnimatorStateAfterTrigger(HealthBarFXAnimator, "FX动画器"));
        //     }
        //     else
        //     {
        //         HealthBarAnimator.gameObject.SetActive(true);
        //         HealthBarFXAnimator.gameObject.SetActive(false);
        //         
        //         int animationValue = 0 - targetHealth;
        //         
        //         if (enableDebugLogs)
        //         {
        //             Debug.Log($"[血条管理] 回血动画 - 设置Health参数为: {animationValue}");
        //         }
        //         
        //         HealthBarAnimator.SetInteger("Health", animationValue);
        //         StartCoroutine(CheckAnimatorStateAfterTrigger(HealthBarAnimator, "主动画器"));
        //     }
        // }
        //
        // private IEnumerator CheckAnimatorStateAfterTrigger(Animator animator, string name)
        // {
        //     yield return null; // 等待一帧
        //     
        //     if (enableDebugLogs)
        //     {
        //         AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        //         Debug.Log($"[血条管理] {name} 触发后状态:");
        //         Debug.Log($"  - 状态哈希: {stateInfo.fullPathHash}");
        //         Debug.Log($"  - 状态长度: {stateInfo.length}");
        //         Debug.Log($"  - 播放进度: {stateInfo.normalizedTime}");
        //         Debug.Log($"  - Health参数值: {animator.GetInteger("Health")}");
        //         
        //         if (stateInfo.length <= 0)
        //         {
        //             Debug.LogError($"[血条管理] {name} 当前动画状态长度为0！");
        //         }
        //     }
        // }
        //
        //
        // private void OnAnimationComplete()
        // {
        //     if (enableDebugLogs)
        //     {
        //         Debug.Log("[血条管理] 动画完成，开始清理");
        //     }
        //     
        //     HealthBarAnimator.gameObject.SetActive(false);
        //     HealthBarFXAnimator.gameObject.SetActive(false);
        //     isPlayingAnimation = false;
        //     
        //     if (enableDebugLogs)
        //     {
        //         Debug.Log($"[血条管理] 动画完成，最终血量: {currentHealth}");
        //     }
        // }

    }
}
