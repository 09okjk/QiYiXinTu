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
    
    #region Keyboards
    private void OnInventoryToggle(InputAction.CallbackContext context)
    {
        InventoryManager.Instance.ToggleInventory();
    }
    
    private void OnMenuToggle(InputAction.CallbackContext context)
    {
        MenuManager.Instance.ToggleMenu();
    }
    #endregion


    public IEnumerator BusyFor(float seconds)
    {
        isBusy = true;
        
        yield return new WaitForSeconds(seconds);
        
        isBusy = false;
    }
    
    public void AnimationTrigger() => StateMachine.CurrentState.AnimationFinishTrigger();

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
    
    #region Velocity
    public void ZeroVelocity() => Rb.linearVelocity = Vector2.zero;
    public void SetVelocity(float xVelocity,float yVelocity)
    {
        Rb.linearVelocity = new Vector2(xVelocity, yVelocity);
        FlipController(xVelocity);
    }
    #endregion
    
    #region Collision Checks

    public bool IsGroundDetected() => Physics2D.Raycast(groundCheck.position, Vector2.down, groundCheckDistance, whatIsGround);
    public bool IsWallDetected() => Physics2D.Raycast(wallCheck.position, Vector2.right * FacingDirection, wallCheckDistance, whatIsGround);
    
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(groundCheck.position, new Vector3(groundCheck.position.x, groundCheck.position.y - groundCheckDistance, groundCheck.position.z));
        Gizmos.DrawLine(wallCheck.position, new Vector3(wallCheck.position.x + wallCheckDistance, wallCheck.position.y, wallCheck.position.z));
    }
    #endregion
    
    #region Flip
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
