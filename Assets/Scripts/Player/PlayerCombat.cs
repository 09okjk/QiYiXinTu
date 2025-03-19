using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

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
    
    [Header("Input")]
    [SerializeField] private InputActionReference attackAction;  // 添加攻击输入
    [SerializeField] private InputActionReference defendAction;  // 添加防御输入

    private int attackCounter = 0;
    private float lastAttackTime;
    private bool canAttack = true;
    private bool isDefending = false;
    
    private Dictionary<ItemData,int> PlayerSkills = new Dictionary<ItemData, int>();
    
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
    
    private void OnEnable()
    {
        // 启用输入操作
        attackAction.action.Enable();
        defendAction.action.Enable();
        
        // 注册输入回调
        attackAction.action.performed += OnAttack;
        defendAction.action.performed += OnDefend;
    }
    
    private void OnDisable()
    {
        // 禁用输入操作
        attackAction.action.Disable();
        defendAction.action.Disable();
        
        // 取消注册回调
        attackAction.action.performed -= OnAttack;
        defendAction.action.performed -= OnDefend;
    }
    
    private void Update()
    {
        // 重置连击窗口
        if (Time.time - lastAttackTime > comboTimeWindow && attackCounter > 0)
        {
            attackCounter = 0;
            animator.SetInteger(attackCounterHash, attackCounter);
        }
    }
    
    // 攻击回调函数
    private void OnAttack(InputAction.CallbackContext context)
    {
        if (canAttack && !isDefending)
        {
            Attack();
        }
    }
    
    // 防御回调函数
    private void OnDefend(InputAction.CallbackContext context)
    {
        if (!isDefending && canAttack)
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
    
    // 添加技能
    public void AddSkill(ItemData skill)
    {
        if (!PlayerSkills.TryAdd(skill, 1))
        {
            PlayerSkills[skill]++;
        }
    }
    
    // 使用技能
    public void UseSkill(ItemData skill)
    {
        if (PlayerSkills.ContainsKey(skill))
        {
            // 使用技能
            Debug.Log("Using skill: " + skill.itemName);
            
            // 减少技能数量
            PlayerSkills[skill]--;
            
            // 如果技能数量为0，从字典中删除
            if (PlayerSkills[skill] <= 0)
            {
                PlayerSkills.Remove(skill);
            }
        }
        else
        {
            Debug.LogWarning("Player does not have skill: " + skill.itemName);
        }
        
        // 刷新技能栏
        GameUIManager.Instance.RefreshSkillBar();
    }
    
    // 获取所有技能
    public Dictionary<ItemData,int> GetSkills()
    {
        return PlayerSkills;
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