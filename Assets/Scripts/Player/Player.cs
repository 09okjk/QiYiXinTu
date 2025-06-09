using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Core;
using Manager;
using News;
using Skills;
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
    [SerializeField] private InputActionReference newsBookAction;
    
    public bool isBusy {get; private set;}
    // public SkillManager skillManager { get; private set; }
    
    private bool _isMenuOpen = false;
    private bool _isInventoryOpen = false;
    private bool _isNewsBookOpen = false;
    private bool _isPopWindowOpen = false;
    
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

    #region Events

    public event Action<int, bool> OnHealthChanged;
    public event Action<float, float> OnManaChanged;

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
        // skillManager = SkillManager.Instance;
    }
    
    private void OnEnable()
    {
        inventoryAction.action.Enable();
        menuAction.action.Enable();
        
        // MenuManager.Instance.OnMenuStateChanged += HandleMenuStateChanged;
        InventoryManager.Instance.OnInventoryStateChanged += HandleInventoryStateChanged;
        inventoryAction.action.performed += OnInventoryToggle;
        menuAction.action.performed += OnMenuToggle;
        newsBookAction.action.performed += OnNewsBookToggle;
    }
    
    private void OnDisable()
    {
        inventoryAction.action.Disable();
        menuAction.action.Disable();
        
        // MenuManager.Instance.OnMenuStateChanged -= HandleMenuStateChanged;
        InventoryManager.Instance.OnInventoryStateChanged -= HandleInventoryStateChanged;
        // NewsManager.Instance.OnNewsBookStateChanged -= HandleNewsBookStateChanged;
        inventoryAction.action.performed -= OnInventoryToggle;
        menuAction.action.performed -= OnMenuToggle;
        newsBookAction.action.performed -= OnNewsBookToggle;
    }

    protected override void Update()
    {
        base.Update();
        
        // 如果菜单、背包、新闻库打开，不响应输入
        if (_isMenuOpen || _isInventoryOpen || _isNewsBookOpen || _isPopWindowOpen)
        {
            // Debug.LogWarning($"菜单、背包或新闻库打开，输入被禁用: Menu: {_isMenuOpen}, Inventory: {_isInventoryOpen}, NewsBook: {_isNewsBookOpen}, PopWindow: {_isPopWindowOpen}");
            return;
        }
        
        // 如果正在与UI交互，不响应输入
        if (GameUIManager.Instance && GameUIManager.Instance.IsInteractingWithUI)
        {
            // Debug.LogWarning("正在与UI交互，输入被禁用");
            return;
        }
        
        // 如果正在对话中，不响应输入
        if (DialogueManager.Instance.IsDialogueActive())
        {
            // Debug.LogWarning("正在对话中，输入被禁用");
            return;
        }
        
        // if (MenuManager.Instance.IsAnyPanelOpen())
        //     return;
        
        stateMachine.CurrentState.Update();
        
        CheckForDashInput();
        StartCoroutine(nameof(BusyFor), 0.1f);
    }
    
    public void HandleMenuStateChanged(bool isOpen)
    {
        _isMenuOpen = isOpen;
    }  
    private void HandleInventoryStateChanged(bool isOpen)
    {
        _isInventoryOpen = isOpen;
    }
    
    // public void RegisterNewsBookEvent()
    // {
    //     if (NewsManager.Instance != null)
    //     {
    //         NewsManager.Instance.OnNewsBookStateChanged += HandleNewsBookStateChanged;
    //     }
    // }
    
    public void RegisterPopWindowEvent()
    {
        if (UIManager.Instance)
        {
            UIManager.Instance.OnPopWindowEvent += HandlePopWindowEvent;
        }
    }

    private void HandlePopWindowEvent(bool isOpen)
    {
        _isPopWindowOpen = isOpen;
    }

    public void HandleNewsBookStateChanged(bool isOpen)
    {
        _isNewsBookOpen = isOpen;
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
        
        if (Input.GetKeyDown(KeyCode.LeftShift) && SkillManager.Instance.dashSkill.CanUseSkill())
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
    
    public override void Damage(int damage)
    {
        base.Damage(damage);

        if (baseData.CurrentHealth <= 0)
        {
            baseData.CurrentHealth = 0;
            OnHealthChanged?.Invoke(baseData.CurrentHealth, true);
            stateMachine.ChangeState(DeathState);
            return;
        }
        OnHealthChanged?.Invoke(baseData.CurrentHealth, true);
        if (stateMachine.CurrentState == HurtState)
        {
            return;
        }

        stateMachine.ChangeState(HurtState);
    }

    public void SpendMana(float amount)
    {
        if (playerData.CurrentMana >= amount)
        {
            playerData.CurrentMana -= amount;
            OnManaChanged?.Invoke(playerData.CurrentMana, playerData.MaxMana);
        }
    }
    public override void AddHealth(float amount)
    {
        base.AddHealth(amount);
        float maxHealth = playerData.MaxHealth;
        playerData.CurrentHealth = (int)Math.Min(playerData.CurrentHealth + amount, maxHealth);
        OnHealthChanged?.Invoke(playerData.CurrentHealth, false);
    }

    public override void AddMana(float amount)
    {
        base.AddMana(amount);
        float maxMana = playerData.MaxMana;
        playerData.CurrentMana = Mathf.Min(playerData.CurrentMana + amount, maxMana);
        OnManaChanged?.Invoke(playerData.CurrentMana, playerData.MaxMana);
    }

    public override Task Die()
    {
        _ = base.Die();
        // 触发游戏事件
        GameManager.Instance.OnGameEvent("PlayerDied");
        Destroy(gameObject);
        return Task.CompletedTask;
    }

    #region Input Actions

    private bool CanToggleUI()
    {
        // 只要对话激活或有其他UI打开，就不允许切换
        return !DialogueManager.Instance.IsDialogueActive() && !_isMenuOpen && !_isInventoryOpen && !_isNewsBookOpen;
    }

    private void OnInventoryToggle(InputAction.CallbackContext context)
    {
        if (!CanToggleUI() || _isNewsBookOpen || _isPopWindowOpen) return;
        InventoryManager.Instance.ToggleInventory();
    }

    private void OnNewsBookToggle(InputAction.CallbackContext context)
    {
        if (!CanToggleUI() || _isInventoryOpen || _isPopWindowOpen) return;
        NewsManager.Instance.ToggleNewsInfoBook();
    }
    
    private void OnMenuToggle(InputAction.CallbackContext context)
    {
        if (!CanToggleUI() || _isInventoryOpen || _isNewsBookOpen || _isPopWindowOpen) return;
        MenuManager.Instance.ToggleMenu();
    }

    #endregion
}
