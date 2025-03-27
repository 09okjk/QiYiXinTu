using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    
    [Header("Move Info")]
    public float moveSpeed = 8f;
    public float jumpForce = 12f;
    public float wallJumpForce = 5f;
    public float idleToMoveTransitionTime = 0.1f;
    
    [Header("Dash Info")]
    [SerializeField] private float dashCoolDown = 1f;
    [SerializeField] private float dashUsageTimer;
    public float dashSpeed = 30f;
    public float dashDuration = 0.2f;
    public float DashDir { get; private set; }  
    
    [Header("Collision Info")]
    public Transform groundCheck;
    public float groundCheckDistance = 0.3f;
    public Transform wallCheck;
    public float wallCheckDistance = 0.3f;
    public LayerMask whatIsGround;
    
    [Header("Attack Info")]
    public float comboTimeWindow = .2f;

    public Vector2[] attackMovements;
    
    [Header("Input Actions")]
    [SerializeField] private InputActionReference inventoryAction;
    [SerializeField] private InputActionReference menuAction;
    
    public bool isBusy {get; private set;}
    public int FacingDirection { get; private set; } = 1;
    private bool _facingRight = true;

    #region Components

    public Animator Anim { get; private set; }
    public Rigidbody2D Rb { get; private set; }
    
    #endregion
    
    #region States
    
    public PlayerStateMachine StateMachine { get; private set; }
    public PlayerIdleState IdleState { get; private set; }
    public PlayerMoveState MoveState { get; private set; }
    public PlayerIdleToMoveTransitionState IdleToMoveTransitionState { get; private set; }
    public PlayerJumpState JumpState { get; private set; }
    public PlayerAirState AirState { get; private set; }
    public PlayerDashState DashState { get; private set; }
    public PlayerWallSlideState WallSlideState { get; private set; }
    public PlayerWallJumpState WallJumpState { get; private set; }
    public PlayerPrimaryAttackState PrimaryAttackState { get; private set; }
    
    #endregion
    private void Awake()
    {
        StateMachine = new PlayerStateMachine();
        
        IdleState = new PlayerIdleState(this, StateMachine, "Idle");
        MoveState = new PlayerMoveState(this, StateMachine, "Move");
        IdleToMoveTransitionState = new PlayerIdleToMoveTransitionState(this, StateMachine, "IdleToMove");
        JumpState = new PlayerJumpState(this, StateMachine, "Jump");
        AirState = new PlayerAirState(this, StateMachine, "Jump");
        DashState = new PlayerDashState(this, StateMachine, "Dash");
        WallSlideState = new PlayerWallSlideState(this, StateMachine, "WallSlide");
        WallJumpState = new PlayerWallJumpState(this, StateMachine, "Jump");
        PrimaryAttackState = new PlayerPrimaryAttackState(this, StateMachine, "Attack");
    }

    private void Start()
    {
        Anim = GetComponentInChildren<Animator>();
        Rb = GetComponent<Rigidbody2D>();
        
        StateMachine.Initialize(IdleState);
    }
    
    private void OnEnable()
    {
        inventoryAction.action.Enable();
        menuAction.action.Enable();
        
        inventoryAction.action.performed += OnInventoryToggle;
        menuAction.action.performed += OnMenuToggle;
    }
    
    private void OnDisable()
    {
        inventoryAction.action.Disable();
        menuAction.action.Disable();
        
        inventoryAction.action.performed -= OnInventoryToggle;
        menuAction.action.performed -= OnMenuToggle;
    }

    private void Update()
    {
        StateMachine.CurrentState.Update();
        
        CheckForDashInput();
        StartCoroutine(nameof(BusyFor), 0.1f);
    }

    /// <summary>
    /// 忙碌状态
    /// </summary>
    /// <param name="seconds"> 忙碌时间 </param>
    /// <returns> 忙碌协程 </returns>
    public IEnumerator BusyFor(float seconds)
    {
        isBusy = true;
        
        yield return new WaitForSeconds(seconds);
        
        isBusy = false;
    }
    
    /// <summary>
    /// 动画触发，用于状态机，通知状态机动画播放完毕，可以进行下一步操作
    /// </summary>
    public void AnimationTrigger() => StateMachine.CurrentState.AnimationFinishTrigger();

    /// <summary>
    /// 检查是否可以进行冲刺
    /// </summary>
    private void CheckForDashInput()
    {
        dashUsageTimer -= Time.deltaTime;

        if (IsWallDetected())
        {
            return;
        }
        
        if (Input.GetKeyDown(KeyCode.LeftShift) && dashUsageTimer < 0)
        {
            dashUsageTimer = dashCoolDown;
            DashDir = Input.GetAxisRaw("Horizontal");
            
            if (DashDir == 0)
            {
                DashDir = FacingDirection;
            }
            
            StateMachine.ChangeState(DashState);
        }
    }
        
    #region Keyboards Input 键盘输入
    private void OnInventoryToggle(InputAction.CallbackContext context)
    {
        InventoryManager.Instance.ToggleInventory();
    }
    
    private void OnMenuToggle(InputAction.CallbackContext context)
    {
        MenuManager.Instance.ToggleMenu();
    }
    #endregion
    
    #region Velocity Control 速度控制
    
    /// <summary>
    /// 设置速度 0
    /// </summary>
    public void ZeroVelocity() => Rb.linearVelocity = Vector2.zero;
    
    /// <summary>
    /// 设置速度 x y
    /// </summary>
    /// <param name="xVelocity"> x 速度 </param>
    /// <param name="yVelocity"> y 速度</param>
    public void SetVelocity(float xVelocity,float yVelocity)
    {
        Rb.linearVelocity = new Vector2(xVelocity, yVelocity);
        FlipController(xVelocity);
    }
    #endregion
    
    #region Collision Checks 碰撞检测

    /// <summary>
    /// 是否检测到地面
    /// </summary>
    /// <returns> 是否检测到地面 </returns>
    public bool IsGroundDetected() => Physics2D.Raycast(groundCheck.position, Vector2.down, groundCheckDistance, whatIsGround);
    
    /// <summary>
    /// 是否检测到墙壁
    /// </summary>
    /// <returns> 是否检测到墙壁 </returns>
    public bool IsWallDetected() => Physics2D.Raycast(wallCheck.position, Vector2.right * FacingDirection, wallCheckDistance, whatIsGround);
    
    /// <summary>
    /// 绘制碰撞检测线
    /// </summary>
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(groundCheck.position, new Vector3(groundCheck.position.x, groundCheck.position.y - groundCheckDistance, groundCheck.position.z));
        Gizmos.DrawLine(wallCheck.position, new Vector3(wallCheck.position.x + wallCheckDistance, wallCheck.position.y, wallCheck.position.z));
    }
    #endregion
    
    #region Flip Control 翻转控制
    public void Flip()
    {
        FacingDirection *= -1;
        _facingRight = !_facingRight;
        transform.Rotate(0.0f, 180.0f, 0.0f);
    }
    
    public void FlipController(float x)
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
}
