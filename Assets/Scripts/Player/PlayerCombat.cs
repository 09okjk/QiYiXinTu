using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCombat : MonoBehaviour
{
    [Header("Combat Settings")]
    [SerializeField] private float attackRange = 1.5f;
    [SerializeField] private Transform attackPoint;
    [SerializeField] private LayerMask enemyLayers;
    [SerializeField] private float attackDamage = 20f;
    [SerializeField] private float comboTimeWindow = 0.5f;
    [SerializeField] private float defenseDuration = 0.5f;
    [SerializeField] private float defenseInvincibilityTime = 0.2f;
    
    [Header("Animation")]
    [SerializeField] private Animator animator;
    
    private int attackCounter = 0;
    private float lastAttackTime;
    private bool canAttack = true;
    private bool isDefending = false;
    
    // 动画参数哈希值 可以提高性能
    private int attackTriggerHash;
    private int attackCounterHash;
    private int defendHash;
    
    private PlayerHealth playerHealth;
    
    private void Awake()
    {
        attackTriggerHash = Animator.StringToHash("Attack");
        attackCounterHash = Animator.StringToHash("AttackCounter");
        defendHash = Animator.StringToHash("Defend");
        
        playerHealth = GetComponent<PlayerHealth>();
    }
    
    private void Update()
    {
        // 重置连击窗口
        if (Time.time - lastAttackTime > comboTimeWindow && attackCounter > 0)
        {
            attackCounter = 0;
            animator.SetInteger(attackCounterHash, attackCounter);
        }
        
        // 攻击输入，如果可以攻击且没有在防御
        if (Input.GetButtonDown("Fire1") && canAttack && !isDefending)
        {
            Attack();
        }
        
        // 防御输入，如果没有在防御且可以攻击
        if (Input.GetButtonDown("Fire2") && !isDefending && canAttack)
        {
            StartCoroutine(Defend());
        }
    }
    
    private void Attack()
    {
        // 增加攻击计数器（循环1-2-3）
        attackCounter = (attackCounter % 3) + 1;
        
        // 更新上次攻击时间
        lastAttackTime = Time.time;
        
        // 设置动画参数
        animator.SetInteger(attackCounterHash, attackCounter);
        animator.SetTrigger(attackTriggerHash);
        
        // 实际伤害通过动画事件应用
    }
    
    // 由动画事件调用
    /// <summary>
    /// 应用伤害
    /// </summary>
    public void ApplyDamage()
    {
        // 获取范围内的所有敌人
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, enemyLayers);
        
        // 对每个敌人应用伤害
        foreach (Collider2D enemy in hitEnemies)
        {
            enemy.GetComponent<EnemyHealth>()?.TakeDamage(attackDamage);
        }
    }
    
    /// <summary>
    /// 防御
    /// </summary>
    /// <returns></returns>
    private IEnumerator Defend()
    {
        isDefending = true;
        canAttack = false;
        
        // 设置动画
        animator.SetBool(defendHash, true);
        
        // 应用短暂的无敌
        playerHealth.SetInvincible(true);
        
        // 等待无敌时间
        yield return new WaitForSeconds(defenseInvincibilityTime);
        
        // 移除无敌但保持防御动画
        playerHealth.SetInvincible(false);
        
        // 等待剩余的防御时间
        yield return new WaitForSeconds(defenseDuration - defenseInvincibilityTime);
        
        // 结束防御
        isDefending = false;
        canAttack = true;
        animator.SetBool(defendHash, false);
    }
    
    
    /// <summary>
    /// 绘制攻击范围
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        if (attackPoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(attackPoint.position, attackRange);
        }
    }
}