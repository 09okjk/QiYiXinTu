using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class EnemyHealth : MonoBehaviour
{
    [Header("生命值设置")]
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float currentHealth;
    
    [Header("受伤设置")]
    [SerializeField] private float invincibilityTime = 0.5f;  // 受伤后的无敌时间
    [SerializeField] private GameObject hitEffectPrefab;      // 受伤特效
    [SerializeField] private SpriteRenderer spriteRenderer;   // 用于受伤闪烁效果
    
    [Header("死亡设置")]
    [SerializeField] private GameObject deathEffectPrefab;    // 死亡特效
    [SerializeField] private float destroyDelay = 1f;         // 死亡后销毁延迟

    [Header("事件")]
    public UnityEvent OnDeath;
    public UnityEvent<float, float> OnHealthChanged;  // 参数：当前生命值, 最大生命值

    private bool isInvincible = false;
    private Animator animator;
    private int deathHash;
    private int hitHash;

    private void Awake()
    {
        currentHealth = maxHealth;
        animator = GetComponent<Animator>();
        
        if (animator != null)
        {
            deathHash = Animator.StringToHash("Death");
            hitHash = Animator.StringToHash("Hit");
        }
        
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }
    }

    public void TakeDamage(float damage)
    {
        if (isInvincible || currentHealth <= 0) return;

        // 应用伤害
        currentHealth = Mathf.Max(0, currentHealth - damage);
        
        // 触发事件
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
        
        // 播放受伤动画和特效
        if (animator != null && hitHash != 0)
        {
            animator.SetTrigger(hitHash);
        }
        
        if (hitEffectPrefab != null)
        {
            Instantiate(hitEffectPrefab, transform.position, Quaternion.identity);
        }
        
        StartCoroutine(FlashEffect());
        
        // 检查是否死亡
        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            // 短暂无敌时间
            StartCoroutine(InvincibilityCoroutine());
        }
    }

    private IEnumerator InvincibilityCoroutine()
    {
        isInvincible = true;
        yield return new WaitForSeconds(invincibilityTime);
        isInvincible = false;
    }

    private IEnumerator FlashEffect()
    {
        if (spriteRenderer != null)
        {
            Color originalColor = spriteRenderer.color;
            spriteRenderer.color = Color.red;
            yield return new WaitForSeconds(0.1f);
            spriteRenderer.color = originalColor;
        }
        else
        {
            yield return null;
        }
    }

    private void Die()
    {
        // 防止多次调用
        if (enabled == false) return;
        
        // 播放死亡动画
        if (animator != null && deathHash != 0)
        {
            animator.SetTrigger(deathHash);
        }
        
        // 播放死亡特效
        if (deathEffectPrefab != null)
        {
            Instantiate(deathEffectPrefab, transform.position, Quaternion.identity);
        }
        
        // 触发死亡事件
        OnDeath?.Invoke();
        
        // 禁用碰撞体
        foreach (Collider2D col in GetComponents<Collider2D>())
        {
            col.enabled = false;
        }
        
        // 禁用此脚本以防止进一步伤害
        enabled = false;
        
        // 延迟销毁游戏对象
        Destroy(gameObject, destroyDelay);
    }

    // 公共方法，用于恢复生命值
    public void Heal(float amount)
    {
        currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }
    
    // 获取当前生命值百分比
    public float GetHealthPercentage()
    {
        return currentHealth / maxHealth;
    }
}