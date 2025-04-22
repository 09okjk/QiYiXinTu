using System.Collections;
using Core;
using UnityEngine;

public class Entity:MonoBehaviour
{
    [Header("Basic Info")]
    public EntityData baseData;

    protected internal float InvincibleTime => baseData.InvincibleTime;
    private Vector2 knockbackDirection => baseData.knockbackDirection;
    private float knockbackDuration => baseData.KnockbackDuration;

    protected bool isKnocked;
    
    [Header("Collision Info")]
    public Transform attackCheck;
    public float attackCheckRadius = 0.5f;
    [SerializeField] protected Transform groundCheck;
    [SerializeField] protected float groundCheckDistance = 0.3f;
    [SerializeField] protected Transform wallCheck;
    [SerializeField] protected float wallCheckDistance = 0.3f;
    [SerializeField] protected LayerMask whatIsGround;
    
    #region Components

    public Animator Anim { get; protected set; }
    public Rigidbody2D Rb { get; protected set; }
    public EntityFX EntityFX { get; protected set; }
    
    #endregion
    
    public int FacingDirection { get; private set; } = 1;
    protected bool _facingRight = true;

    protected virtual void Awake()
    {
        
    }
    
    protected virtual void Start()
    {
        Anim = GetComponentInChildren<Animator>();
        Rb = GetComponent<Rigidbody2D>();
        EntityFX = GetComponent<EntityFX>();
    }
    
    protected virtual void Update()
    {
        
    }

    public virtual void Damage(float damage)
    {
        //EntityFX.StartCoroutine("FlashFX");
        StartCoroutine(nameof(HitKnockback));
        Debug.Log(gameObject.name + " was damage");
        baseData.CurrentHealth -= damage;
    }

    public virtual void AddHealth(float amount)
    {
        
    }

    public virtual void AddMana(float amount)
    {
        
    }

    public virtual float GetHealthPercentage()
    {
        return baseData.CurrentHealth / baseData.MaxHealth;
    }
    
    public virtual float GetManaPercentage()
    {
        return baseData.CurrentMana / baseData.MaxMana;
    }
    
    public virtual void Die()
    {
        //EntityFX.StartCoroutine("DieFX");
        Debug.Log(gameObject.name + " was die");
        Destroy(gameObject);
    }

    protected virtual IEnumerator HitKnockback()
    {
        isKnocked = true;
        
        Rb.linearVelocity = new Vector2(knockbackDirection.x * -FacingDirection, knockbackDirection.y);
        
        yield return new WaitForSeconds(knockbackDuration);
        
        isKnocked = false;
    }
    
    
    #region Velocity Control 速度控制
    
    /// <summary>
    /// 设置速度 0
    /// </summary>
    public void SetZeroVelocity()
    {
        if (isKnocked)
            return;
        Rb.linearVelocity = Vector2.zero;
    }
    
    /// <summary>
    /// 设置速度 x y
    /// </summary>
    /// <param name="xVelocity"> x 速度 </param>
    /// <param name="yVelocity"> y 速度</param>
    public void SetVelocity(float xVelocity,float yVelocity)
    { 
        if (isKnocked)
            return;
        
        Rb.linearVelocity = new Vector2(xVelocity, yVelocity);
        FlipController(xVelocity);
    }
    #endregion
    
    #region Flip Control 翻转控制
    public virtual void Flip()
    {
        FacingDirection *= -1;
        _facingRight = !_facingRight;
        transform.Rotate(0.0f, 180.0f, 0.0f);
    }
    
    public virtual void FlipController(float x)
    {
        if (x > 0 && !_facingRight)
        {
            Flip();
        }
        else if (x < 0 && _facingRight)
        {
            Flip();
        }
    }
    #endregion
    
    #region Collision Checks 碰撞检测

    /// <summary>
    /// 是否检测到地面
    /// </summary>
    /// <returns> 是否检测到地面 </returns>
    public virtual bool IsGroundDetected() => Physics2D.Raycast(groundCheck.position, Vector2.down, groundCheckDistance, whatIsGround);
    
    /// <summary>
    /// 是否检测到墙壁
    /// </summary>
    /// <returns> 是否检测到墙壁 </returns>
    public virtual bool IsWallDetected() => Physics2D.Raycast(wallCheck.position, Vector2.right * FacingDirection, wallCheckDistance, whatIsGround);
    
    /// <summary>
    /// 绘制碰撞检测线
    /// </summary>
    protected virtual void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(groundCheck.position, new Vector3(groundCheck.position.x, groundCheck.position.y - groundCheckDistance, groundCheck.position.z));
        Gizmos.DrawLine(wallCheck.position, new Vector3(wallCheck.position.x + wallCheckDistance * FacingDirection, wallCheck.position.y, wallCheck.position.z));
        Gizmos.color = Color.white;
        Gizmos.DrawWireSphere(attackCheck.position, attackCheckRadius);
    }
    #endregion
}