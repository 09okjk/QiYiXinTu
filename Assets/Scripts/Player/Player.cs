using System;
using System.Collections;
using System.Collections.Generic;
using Core;
using Manager;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : Entity
{
    public Transform swordPoint;
    
    public PlayerData playerData => (PlayerData)baseData;
    public float moveSpeed => playerData.moveSpeed;
    public float jumpForce => playerData.jumpForce;
    public float wallJumpForce => playerData.wallJumpForce;
    public float idleToMoveTransitionTime => playerData.idleToMoveTransitionTime;

    public float DashDir { get; private set; }  
    
    [Header("Attack Info")]
    public float comboTimeWindow => playerData.comboTimeWindow;
    public float counterAttackDuration => playerData.counterAttackDuration;
    public LayerMask whatIsEnemy;
    public Vector2[] attackMovements;
    
    [Header("Input Actions")]
    [SerializeField] private InputActionReference inventoryAction;
    [SerializeField] private InputActionReference menuAction;
    
    public bool isBusy {get; private set;}
    public SkillManager skillManager { get; private set; }
    
    #region States
    
    public PlayerStateMachine stateMachine { get; private set; }
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
    public PlayerAimSwordState AimSwordState { get; private set; }
    public PlayerThrowSwordState ThrowSwordState { get; private set; }
    public PlayerCatchSwordState CatchSwordState { get; private set; }
    public PlayerHurtState HurtState { get; private set; }
    public PlayerDeathState DeathState { get; private set; }
    
    #endregion
    protected override void Awake()
    {
        base.Awake();
        stateMachine = new PlayerStateMachine();
        
        IdleState = new PlayerIdleState(this, stateMachine, "Idle");
        MoveState = new PlayerMoveState(this, stateMachine, "Move");
        IdleToMoveTransitionState = new PlayerIdleToMoveTransitionState(this, stateMachine, "IdleToMove");
        JumpState = new PlayerJumpState(this, stateMachine, "Jump");
        AirState = new PlayerAirState(this, stateMachine, "Jump");
        DashState = new PlayerDashState(this, stateMachine, "Dash");
        WallSlideState = new PlayerWallSlideState(this, stateMachine, "WallSlide");
        WallJumpState = new PlayerWallJumpState(this, stateMachine, "Jump");
        PrimaryAttackState = new PlayerPrimaryAttackState(this, stateMachine, "Attack");
        CounterAttackState = new PlayerConterAttackState(this, stateMachine, "CounterAttack");
        AimSwordState = new PlayerAimSwordState(this, stateMachine, "AimSword");
        ThrowSwordState = new PlayerThrowSwordState(this, stateMachine, "ThrowSword");
        CatchSwordState = new PlayerCatchSwordState(this, stateMachine, "CatchSword");
        HurtState = new PlayerHurtState(this, stateMachine, "Hurt");
        DeathState = new PlayerDeathState(this, stateMachine, "Death");
    }

    protected override void Start()
    {
        base.Start();
        stateMachine.Initialize(IdleState);
        skillManager = SkillManager.Instance;
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
        stateMachine.CurrentState.Update();
        
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
    public void AnimationTrigger() => stateMachine.CurrentState.AnimationFinishTrigger();

    /// <summary>
    /// 检查是否可以进行冲刺
    /// </summary>
    private void CheckForDashInput()
    {
        if (IsWallDetected())
        {
            return;
        }
        
        if (Input.GetKeyDown(KeyCode.LeftShift) && skillManager.dashSkill.CanUseSkill())
        {
            SkillManager.Instance.dashSkill.UseSkill();
            DashDir = Input.GetAxisRaw("Horizontal");
            
            if (DashDir == 0)
            {
                DashDir = FacingDirection;
            }
            
            stateMachine.ChangeState(DashState);
        }
    }
    
    public override void Damage(float damage)
    {
        base.Damage(damage);

        if (baseData.CurrentHealth <= 0)
        {
            baseData.CurrentHealth = 0;
            stateMachine.ChangeState(DeathState);
            return;
        }
        
        if (stateMachine.CurrentState == HurtState)
        {
            return;
        }

        stateMachine.ChangeState(HurtState);
    }

    public override void Die()
    {
        base.Die();
        // GameManager.Instance.GameOver();
        
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
