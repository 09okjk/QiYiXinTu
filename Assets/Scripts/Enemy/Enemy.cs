using System;
using UnityEngine;

public class Enemy : Entity
{
    [SerializeField] protected LayerMask whatIsPlayer;
    [SerializeField] protected BoxCollider2D playerCheck;
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
            transform.position, 
            new Vector2(attackDistance, 2), 
            0f, 
            whatIsPlayer);
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
        Gizmos.DrawWireCube(transform.position, new Vector3(attackDistance, 2, 0));
    }
}
