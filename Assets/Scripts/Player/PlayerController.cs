using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.InputSystem;
    
    public class PlayerController : MonoBehaviour
    {
        [Header("Movement")]
        [SerializeField] private float moveSpeed = 5f;
        [SerializeField] private float jumpForce = 12f;
        [SerializeField] private Transform groundCheck;
        [SerializeField] private LayerMask groundLayer;
        [SerializeField] private float groundCheckRadius = 0.2f;
        
        [Header("Animation")]
        [SerializeField] private Animator animator;
        [SerializeField] private SpriteRenderer spriteRenderer;
        
        [Header("Input")]
        [SerializeField] private InputActionReference moveAction;
        [SerializeField] private InputActionReference jumpAction;
        [SerializeField] private InputActionReference inventoryAction;
        [SerializeField] private InputActionReference menuAction;
        
        private Rigidbody2D rb;
        private bool isGrounded;
        private bool facingRight = true;
        private Vector2 moveInput;
        private bool jumpPressed;
        
        // 为了效率，动画参数哈希值
        private int moveSpeedHash;
        private int isGroundedHash;
        private int jumpHash;
        
        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            
            // 缓存动画参数哈希值
            moveSpeedHash = Animator.StringToHash("MoveSpeed");
            isGroundedHash = Animator.StringToHash("IsGrounded");
            jumpHash = Animator.StringToHash("Jump");
        }
        
        private void OnEnable()
        {
            // 启用输入操作
            moveAction.action.Enable();
            jumpAction.action.Enable();
            inventoryAction.action.Enable();
            menuAction.action.Enable();
            
            // 注册回调
            jumpAction.action.performed += OnJump;
            inventoryAction.action.performed += OnInventoryToggle;
            menuAction.action.performed += OnMenuToggle;
        }
        
        private void OnDisable()
        {
            // 禁用输入操作
            moveAction.action.Disable();
            jumpAction.action.Disable();
            inventoryAction.action.Disable();
            menuAction.action.Disable();
            
            // 取消注册回调
            jumpAction.action.performed -= OnJump;
            inventoryAction.action.performed -= OnInventoryToggle;
            menuAction.action.performed -= OnMenuToggle;
        }
        
        private void Update()
        {
            // Check if player is grounded
            isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
            animator.SetBool(isGroundedHash, isGrounded);
            
            // Get movement input
            moveInput = moveAction.action.ReadValue<Vector2>();
            
            // 如果按下并在地面上则处理跳跃
            if (jumpPressed && isGrounded)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
                animator.SetTrigger(jumpHash);
                jumpPressed = false;
            }
            
            // 更新动画
            animator.SetFloat(moveSpeedHash, Mathf.Abs(moveInput.x));
            
            // 根据移动方向翻转角色
            if (moveInput.x > 0 && !facingRight)
            {
                Flip();
            }
            else if (moveInput.x < 0 && facingRight)
            {
                Flip();
            }
        }
        
        private void FixedUpdate()
        {
            // 在 FixedUpdate 中移动玩家以保持一致性
            rb.linearVelocity = new Vector2(moveInput.x * moveSpeed, rb.linearVelocity.y);
        }
        
        private void OnJump(InputAction.CallbackContext context)
        {
            jumpPressed = true;
        }
        
        private void OnInventoryToggle(InputAction.CallbackContext context)
        {
            InventoryManager.Instance.ToggleInventory();
        }
        
        private void OnMenuToggle(InputAction.CallbackContext context)
        {
            MenuManager.Instance.ToggleMenu();
        }
        
        // 翻转角色
        private void Flip()
        {
            facingRight = !facingRight;
            spriteRenderer.flipX = !facingRight;
        }
        
        private void OnDrawGizmosSelected()
        {
            if (groundCheck != null)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
            }
        }
    }