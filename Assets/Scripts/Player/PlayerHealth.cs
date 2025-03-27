using System;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health Settings")]
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float currentHealth;
    [SerializeField] private float maxMana = 100f;
    [SerializeField] private float currentMana;
    [SerializeField] private float manaRegenRate = 5f;
    
    [Header("UI References")]
    [SerializeField] private Slider healthSlider;
    [SerializeField] private Slider manaSlider;
    
    [Header("Effects")]
    [SerializeField] private float invincibilityDuration = 1f;
    [SerializeField] private float flashInterval = 0.1f;
    [SerializeField] private SpriteRenderer spriteRenderer;
    
    private bool isInvincible = false;
    private bool isDead = false;
    
    // Events
    public event Action OnPlayerDeath;
    public event Action<float, float> OnHealthChanged;
    public event Action<float, float> OnManaChanged;
    
    private void Awake()
    {
        currentHealth = maxHealth;
        currentMana = maxMana;
    }
    
    private void Start()
    {
        UpdateHealthUI();
        UpdateManaUI();
    }
    
    private void Update()
    {
        // Regenerate mana over time // 每秒回复法力
        if (currentMana < maxMana)
        {
            currentMana += manaRegenRate * Time.deltaTime;
            currentMana = Mathf.Min(currentMana, maxMana);
            UpdateManaUI();
        }
    }
    
    public void TakeDamage(float damage)
    {
        if (isInvincible || isDead) return;
        
        currentHealth -= damage;
        currentHealth = Mathf.Max(0, currentHealth);
        
        UpdateHealthUI();
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
        
        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            StartCoroutine(InvincibilityFlash());
        }
    }
    
    public void Heal(float amount)
    {
        if (isDead) return;
        
        currentHealth += amount;
        currentHealth = Mathf.Min(currentHealth, maxHealth);
        
        UpdateHealthUI();
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }
    
    public bool UseMana(float amount)
    {
        if (currentMana >= amount)
        {
            currentMana -= amount;
            UpdateManaUI();
            OnManaChanged?.Invoke(currentMana, maxMana);
            return true;
        }
        
        return false;
    }
    
    public void RestoreMana(float amount)
    {
        currentMana += amount;
        currentMana = Mathf.Min(currentMana, maxMana);
        
        UpdateManaUI();
        OnManaChanged?.Invoke(currentMana, maxMana);
    }
    
    private void UpdateHealthUI()
    {
        if (healthSlider != null)
        {
            healthSlider.value = currentHealth / maxHealth;
        }
    }
    
    private void UpdateManaUI()
    {
        if (manaSlider != null)
        {
            manaSlider.value = currentMana / maxMana;
        }
    }
    
    private System.Collections.IEnumerator InvincibilityFlash()
    {
        isInvincible = true;
        
        // 闪烁效果
        float endTime = Time.time + invincibilityDuration;
        bool visible = false;
        
        while (Time.time < endTime)
        {
            visible = !visible;
            spriteRenderer.color = visible ? Color.white : new Color(1, 1, 1, 0.5f);
            yield return new WaitForSeconds(flashInterval);
        }
        
        spriteRenderer.color = Color.white;
        isInvincible = false;
    }
    
    private void Die()
    {
        if (isDead) return;
        
        isDead = true;
        
        // Disable player control 
        GetComponent<Player>().enabled = false;
        GetComponent<PlayerCombat>().enabled = false;
        
        // Trigger death animation
        GetComponent<Animator>().SetTrigger("Death");
        
        // Notify other systems
        OnPlayerDeath?.Invoke();
    }
    
    public void SetInvincible(bool invincible)
    {
        isInvincible = invincible;
    }
    
    public float GetHealthPercentage()
    {
        return currentHealth / maxHealth;
    }
    
    public float GetManaPercentage()
    {
        return currentMana / maxMana;
    }
    
    public void SetHealth(float healthValue) 
    {
        // 假设传入的是百分比值，如在SaveData中保存的playerHealth
        currentHealth = (healthValue / 100f) * maxHealth;
    }

    public void SetMana(float manaValue) 
    {
        // 假设传入的是百分比值，如在SaveData中保存的playerMana
        currentMana = (manaValue / 100f) * maxMana;
    }
    
}