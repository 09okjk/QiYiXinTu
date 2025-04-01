using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : Entity
{
    
    [Header("Move Info")]
    public float moveSpeed = 8f;
    public float jumpForce = 12f;
    public float wallJumpForce = 5f;
    public float idleToMoveTransitionTime = 0.0001f;
    
    [Header("Dash Info")]
    [SerializeField] private float dashCoolDown = 1f;
    [SerializeField] private float dashUsageTimer;
    public float dashSpeed = 30f;
    public float dashDuration = 0.2f;
    public float DashDir { get; private set; }  
    
    [Header("Attack Info")]
    public float comboTimeWindow = .2f;
    public LayerMask whatIsEnemy;
    public Vector2[] attackMovements;
    public float counterAttackDuration = 2f;
    
    [Header("Input Actions")]
    [SerializeField] private InputActionReference inventoryAction;
    [SerializeField] private InputActionReference menuAction;
    
    public bool isBusy {get; private set;}


    
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
    public PlayerConterAttackState CounterAttackState { get; private set; }
    
    #endregion
    protected override void Awake()
    {
        base.Awake();
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
        CounterAttackState = new PlayerConterAttackState(this, StateMachine, "CounterAttack");
    }

    protected override void Start()
    {
        base.Start();
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

    protected override void Update()
    {
        base.Update();
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
        
    private void OnInventoryToggle(InputAction.CallbackContext context)
    {
        InventoryManager.Instance.ToggleInventory();
    }
    
    private void OnMenuToggle(InputAction.CallbackContext context)
    {
        MenuManager.Instance.ToggleMenu();
    }
}
