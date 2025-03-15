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
        
        // Animation parameter hashes for efficiency
        private int moveSpeedHash;
        private int isGroundedHash;
        private int jumpHash;
        
        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            
            // Cache animation parameter hashes
            moveSpeedHash = Animator.StringToHash("MoveSpeed");
            isGroundedHash = Animator.StringToHash("IsGrounded");
            jumpHash = Animator.StringToHash("Jump");
        }
        
        private void OnEnable()
        {
            // Enable input actions
            moveAction.action.Enable();
            jumpAction.action.Enable();
            inventoryAction.action.Enable();
            menuAction.action.Enable();
            
            // Register callbacks
            jumpAction.action.performed += OnJump;
            inventoryAction.action.performed += OnInventoryToggle;
            menuAction.action.performed += OnMenuToggle;
        }
        
        private void OnDisable()
        {
            // Disable input actions
            moveAction.action.Disable();
            jumpAction.action.Disable();
            inventoryAction.action.Disable();
            menuAction.action.Disable();
            
            // Unregister callbacks
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
            
            // Handle jump if pressed and grounded
            if (jumpPressed && isGrounded)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
                animator.SetTrigger(jumpHash);
                jumpPressed = false;
            }
            
            // Update animator
            animator.SetFloat(moveSpeedHash, Mathf.Abs(moveInput.x));
            
            // Flip character based on movement direction
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
            // Move player in FixedUpdate for consistency
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