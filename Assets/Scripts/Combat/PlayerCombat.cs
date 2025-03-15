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
    
    // Animation parameter hashes
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
        // Reset combo if time window passed
        if (Time.time - lastAttackTime > comboTimeWindow && attackCounter > 0)
        {
            attackCounter = 0;
            animator.SetInteger(attackCounterHash, attackCounter);
        }
        
        // Attack input
        if (Input.GetButtonDown("Fire1") && canAttack && !isDefending)
        {
            Attack();
        }
        
        // Defense input
        if (Input.GetButtonDown("Fire2") && !isDefending && canAttack)
        {
            StartCoroutine(Defend());
        }
    }
    
    private void Attack()
    {
        // Increment attack counter (cycles 1-2-3)
        attackCounter = (attackCounter % 3) + 1;
        
        // Update last attack time
        lastAttackTime = Time.time;
        
        // Set animator parameters
        animator.SetInteger(attackCounterHash, attackCounter);
        animator.SetTrigger(attackTriggerHash);
        
        // Actual damage is applied via animation event
    }
    
    // Called by animation event
    public void ApplyDamage()
    {
        // Get all enemies in range
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, enemyLayers);
        
        // Apply damage to each enemy
        foreach (Collider2D enemy in hitEnemies)
        {
            enemy.GetComponent<EnemyHealth>()?.TakeDamage(attackDamage);
        }
    }
    
    private IEnumerator Defend()
    {
        isDefending = true;
        canAttack = false;
        
        // Set animator
        animator.SetBool(defendHash, true);
        
        // Apply brief invincibility
        playerHealth.SetInvincible(true);
        
        // Wait for invincibility period
        yield return new WaitForSeconds(defenseInvincibilityTime);
        
        // Remove invincibility but keep defending animation
        playerHealth.SetInvincible(false);
        
        // Wait for the rest of defense duration
        yield return new WaitForSeconds(defenseDuration - defenseInvincibilityTime);
        
        // End defense
        isDefending = false;
        canAttack = true;
        animator.SetBool(defendHash, false);
    }
    
    private void OnDrawGizmosSelected()
    {
        if (attackPoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(attackPoint.position, attackRange);
        }
    }
}