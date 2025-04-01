using System;
using UnityEngine;

public class Enemy : Entity
{
    [SerializeField] public LayerMask whatIsPlayer;
    [SerializeField] protected BoxCollider2D playerCheck;
    
    [Header("Stunned Info")]
    public float stunnedDuration = 1f;
    public Vector2 stunnedDirection;
    protected bool canBeStunned;
    [SerializeField] protected GameObject counterImage; 
    
    [Header("Move Info")]
    public float moveSpeed = 2f;
    public float idleTime = 1f;
    public float battleTime = 1f;
    
    [Header("Attack Info")]
    public float attackDistance = 1f;
    public float attackCoolDown = 1f;
    [HideInInspector] public float lastAttackTime;
    
    public EnemyStateMachine stateMachine { get; private set; }

    protected override void Awake()
    {
        base.Awake();
        stateMachine = new EnemyStateMachine();
    }

    protected override void Update()
    {
        base.Update();
        stateMachine.currentState.Update();
    }
    
    public virtual void AnimationFinishTrigger() => stateMachine.currentState.AnimationFinishTrigger();
    
    # region Check Functions 检查函数
    /// <summary>
    /// 检测是否有Player在检测范围内
    /// </summary>
    /// <returns>是否检测到Player</returns>
    public virtual bool IsPlayerDetected()
    {
        return Physics2D.OverlapBox(
            playerCheck.bounds.center, 
            playerCheck.bounds.size, 
            0f, 
            whatIsPlayer);
    }
    
    /// <summary>
    ///  检测是否有Player在攻击范围内
    /// </summary>
    /// <returns>是否检测到Player</returns>
    public virtual bool IsPlayerInAttackRange()
    {
        return Physics2D.OverlapBox(
            transform.position+attackDistance/2*transform.right, 
            new Vector2(attackDistance, 2), 
            0f, 
            whatIsPlayer);
    }
    
    # endregion
    
    public virtual void OpenCounterAttackWindow()
    {
        canBeStunned = true;
        counterImage.SetActive(true);
    }
    
    public virtual void CloseCounterAttackWindow()
    {
        canBeStunned = false;
        counterImage.SetActive(false);
    }
    
    protected override void OnDrawGizmos()
    {
        base.OnDrawGizmos();
        
        // 绘制Player检测区域
        if (playerCheck != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireCube(playerCheck.bounds.center, playerCheck.bounds.size);
        }
        
        // 绘制攻击范围
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(transform.position+attackDistance/2*transform.right, new Vector3(attackDistance, 2, 0));
    }
}
