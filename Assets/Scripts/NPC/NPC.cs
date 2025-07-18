using System;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Linq;

[Serializable]
public class NpcGameData
{
    // basic NPC data
    public string npcID;
    public string npcName;
    public string spriteID;
    public string sceneName; // NPC所在场景名称
    public bool canInteract = false; // 是否可以交互
    public bool isActive = true; // 是否激活NPC
    public Vector3 position; // NPC位置
    // follow settings
    public float followSpeed = 2f; // 跟随速度
    public float followDistance = 1.5f; // 跟随距离
    public float interactionDistance = 2f; // 交互距离
    public bool isFollowing = false; // 是否跟随玩家
    // dialogue settings
    public List<string> dialogueIDs = new List<string>(); // 对话ID，用于动态加载对话数据
    public string currentDialogueID; // 当前对话ID
    // activation rules
    // public List<NPCActivationRule> activationRules = new List<NPCActivationRule>(); // 激活规则
    // additional properties
    public List<NPCProperty> properties = new List<NPCProperty>(); // 扩展属性
}
public class NPC : Entity
{
    [Header("NPC Data")] 
    [SerializeField] private float defaultSpeed = 2f; // 默认速度
    private NpcGameData npcGameData;
    
    [Header("渲染设置")]
    public SpriteRenderer spriteRenderer;
    
    [Header("交互设置")]
    [SerializeField] private float interactionDistance = 2f;
    [SerializeField] private GameObject interactionIndicator;
    
    // 私有字段
    private Transform playerTransform; // 缓存玩家Transform
    private GameObject playerGameObject; // 缓存玩家GameObject
    private bool hasSubscribedToEvents = false;
    
    // 性能优化相关
    private float playerCheckInterval = 0.1f; // 玩家检查间隔
    private float lastPlayerCheckTime = 0f;

    #region State
    public NPCStateMachine stateMachine { get; set; }
    #endregion

    #region Unity生命周期

    protected override void Awake()
    {
        base.Awake();
        
        stateMachine = new NPCStateMachine();
    }
    
    protected override void Start()
    {
        base.Start();
        
        // 初始化交互UI
        interactionIndicator.SetActive(false);
        
        // 设置NPC
        // SetupNPC();
        
        // 缓存玩家引用
        CachePlayerReferences();
        
        // 延迟订阅事件
        // StartCoroutine(DelayedEventSubscription());
    }

    protected override void Update()
    {
        base.Update();
        
        if (CheckPlayerInRange())
        {
            // 处理交互输入
            HandleInteractionInput();
        }
        else
        {
            interactionIndicator.SetActive(false);
        }
    }
    
    protected virtual void FixedUpdate()
    {
        // Debug.LogWarning("should follow player:"+ShouldFollowPlayer());
        // 优化的跟随逻辑
        if (ShouldFollowPlayer())
        {
            FollowPlayer();
        }
    }

    protected virtual void OnDestroy()
    {
        UnsubscribeFromEvents();
    }

    #endregion

    #region 初始化

    public void SetCurrentDialogueID(string dialogueID)
    {
        if (npcGameData != null)
        {
            npcGameData.currentDialogueID = dialogueID;
        }
    }
    private void CachePlayerReferences()
    {
        try
        {
            if (playerGameObject == null)
            {
                playerGameObject = GameObject.FindGameObjectWithTag("Player");
            }
        
            if (playerGameObject != null)
            {
                playerTransform = playerGameObject.transform;
                Debug.Log($"NPC {npcGameData?.npcID} 成功缓存玩家引用");
            }
            else
            {
                Debug.LogWarning($"NPC {npcGameData?.npcID} 未找到玩家对象");
            
                // 只有在Start方法中才启动重试协程，避免重复启动
                if (Time.time > 0.1f) // 确保不是在Awake阶段
                {
                    StartCoroutine(RetryPlayerCaching());
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"缓存玩家引用时发生错误: {e.Message}");
        }
    }

    private IEnumerator RetryPlayerCaching()
    {
        int retryCount = 0;
        const int maxRetries = 10;

        while (retryCount < maxRetries && playerGameObject == null)
        {
            yield return new WaitForSeconds(0.5f);
            
            playerGameObject = GameObject.FindGameObjectWithTag("Player");
            if (playerGameObject != null)
            {
                playerTransform = playerGameObject.transform;
                Debug.Log($"NPC {npcGameData?.npcID} 延迟缓存玩家引用成功");
                yield break;
            }
            
            retryCount++;
        }
        
        if (playerGameObject == null)
        {
            Debug.LogError($"NPC {npcGameData?.npcID} 无法找到玩家对象");
        }
    }

    #endregion

    #region 事件管理

    private void SubscribeToEvents()
    {
        if (hasSubscribedToEvents) return;

        try
        {
            GameManager.Instance.OnDialogueManagerReady += OnDialogueManagerReady;
        }
        catch (Exception e)
        {
            Debug.LogError($"订阅事件时发生错误: {e.Message}");
        }
    }

    private void OnDialogueManagerReady()
    {
        if (DialogueManager.Instance != null)
        {
            DialogueManager.Instance.OnDialogueEnd += OnDialogueEnd;
            hasSubscribedToEvents = true;
            Debug.Log($"NPC {npcGameData?.npcID} 成功订阅对话事件");
        }
        else
        {
            Debug.LogWarning($"NPC {npcGameData?.npcID} DialogueManager.Instance为空，延迟重试");
            StartCoroutine(RetryEventSubscription());
        }
    }

    private IEnumerator RetryEventSubscription()
    {
        int retryCount = 0;
        const int maxRetries = 10;

        while (retryCount < maxRetries && !hasSubscribedToEvents)
        {
            yield return new WaitForSeconds(0.5f);
            
            if (DialogueManager.Instance != null)
            {
                DialogueManager.Instance.OnDialogueEnd += OnDialogueEnd;
                hasSubscribedToEvents = true;
                Debug.Log($"NPC {npcGameData?.npcID} 延迟订阅对话事件成功");
                yield break;
            }
            
            retryCount++;
        }
    }

    private void UnsubscribeFromEvents()
    {
        GameManager.Instance.OnDialogueManagerReady -= OnDialogueManagerReady;
        try
        {
            if (hasSubscribedToEvents && DialogueManager.Instance != null)
            {
                DialogueManager.Instance.OnDialogueEnd -= OnDialogueEnd;
                hasSubscribedToEvents = false;
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"取消订阅事件时发生错误: {e.Message}");
        }
    }

    #endregion

    #region NPC设置

    private void SetupNpc(NpcGameData _npcGameData = null)
    {
        if (_npcGameData != null)
        {
            npcGameData = _npcGameData;
        }
        else
        {
            NPCData npcData = baseData as NPCData;
            npcGameData = new NpcGameData
            {
                npcID = npcData.npcID,
                npcName = npcData.npcName,
                spriteID = npcData.spriteID,
                sceneName = npcData.sceneName,
                canInteract = true, // 默认可以交互
                isActive = false,
                position = Vector3.zero, // 默认位置
                followSpeed = npcData.followSpeed,
                followDistance = npcData.followDistance,
                interactionDistance = npcData.interactionDistance,
                isFollowing = false,
                dialogueIDs = new List<string>(npcData.dialogueIDs),
                currentDialogueID = null,
            };
        }

        try
        {
            Debug.Log("Start setting up NPC: " + npcGameData?.npcName+"isFollowing:"+npcGameData?.isFollowing);
            // 设置精灵
            SetupSprite();

            // defaultSpeed = GetNpcGameData().followSpeed;
            
            // 初始化跟随状态
            if (GetNpcGameData().isFollowing)
            {
                FollowTargetPlayer();
            }else
            {
                StopFollowing();
            }
            
            SubscribeToEvents();
        }
        catch (Exception e)
        {
            Debug.LogError($"设置NPC时发生错误: {e.Message}");
        }
    }

    private void SetupSprite()
    {
        if (spriteRenderer == null || string.IsNullOrEmpty(npcGameData.spriteID)) return;

        Sprite avatar = Resources.Load<Sprite>($"Art/NPCs/{npcGameData.spriteID}");
        if (avatar == null)
        {
            Debug.LogWarning($"NPC {npcGameData.npcName} 的头像未找到，使用默认头像");
            avatar = Resources.Load<Sprite>("Art/NPCs/default_avatar");
        }
        
        if (avatar != null)
        {
            spriteRenderer.sprite = avatar;
        }
    }

    #endregion

    #region 交互系统

    private bool CheckPlayerInRange()
    {
        return Vector2.Distance(transform.position, playerTransform.position) <= interactionDistance;
    }

    private void UpdateInteractionUI(bool show)
    {
        if (interactionIndicator != null)
        {
            interactionIndicator.SetActive(show);
        }
        
    }

    private void HandleInteractionInput()
    {
        if (npcGameData.canInteract)
        {
            UpdateInteractionUI(true);
        }
        else
        {
            UpdateInteractionUI(false);
            return;
        }
        
        if (Input.GetKeyDown(KeyCode.E))
        {
            Debug.Log($"与{npcGameData?.npcName}交互");
            TriggerDialogue();
        }
    }

    #endregion

    #region 对话系统

    private void TriggerDialogue()
    {
        try
        {
            if (string.IsNullOrEmpty(npcGameData.currentDialogueID))
            {
                Debug.LogWarning($"NPC {npcGameData?.npcID} 当前对话ID为空，无法触发对话");
                return;
            }
            
            if (npcGameData.dialogueIDs == null || npcGameData.dialogueIDs.Count == 0)
            {
                Debug.LogWarning($"NPC {npcGameData?.npcID} 对话数据列表为空");
                return;
            }
            
            string dialogueID = npcGameData.dialogueIDs.FirstOrDefault(id => id == npcGameData.currentDialogueID);
            if (dialogueID != null)
            {
                DialogueManager.Instance.StartDialogueByID(dialogueID);
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"触发对话时发生错误: {e.Message}");
        }
    }

    protected virtual void OnDialogueEnd(string dialogueID)
    {
        try
        {
            npcGameData.dialogueIDs.Remove(dialogueID);
            SetCanInteract(false);
        }
        catch (Exception e)
        {
            Debug.LogError($"处理对话结束时发生错误: {e.Message}");
        }
    }

    #endregion

    #region 跟随系统
    
    private bool ShouldFollowPlayer()
    {
        return npcGameData.isFollowing && playerTransform;
    }

    public void FollowTargetPlayer()
    {
        if (playerGameObject == null)
        {
            // 尝试重新缓存玩家引用
            CachePlayerReferences();
        
            if (playerGameObject == null)
            {
                Debug.LogError("玩家对象引用为空，无法开始跟随");
            
                // 启动协程重试
                StartCoroutine(RetryFollowPlayer());
                return;
            }
        }

        npcGameData.isFollowing = true;
    
        UpdateFacingDirection();
    
        Debug.Log($"NPC {npcGameData?.npcID} 开始跟随玩家");
    }

    public void FollowPlayer()
    {
        if (playerTransform == null) return;

        npcGameData.followSpeed = defaultSpeed;
        
        UpdateFacingDirection();
        
        float distance = Vector2.Distance(transform.position, playerTransform.position);
        
        if (distance < npcGameData.followDistance)
        {
            npcGameData.followSpeed = 0;
            return;
        }
        
        Vector2 direction = (playerTransform.position - transform.position).normalized;
        if (Rb != null)
        {
            Rb.MovePosition(Rb.position + direction * npcGameData.followSpeed * Time.fixedDeltaTime);
        }
    }
    
    // 添加重试跟随的协程
    private IEnumerator RetryFollowPlayer()
    {
        int retryCount = 0;
        const int maxRetries = 5;

        while (retryCount < maxRetries && playerGameObject == null)
        {
            yield return new WaitForSeconds(0.2f);
        
            CachePlayerReferences();
        
            if (playerGameObject != null)
            {
                Debug.Log($"NPC {npcGameData?.npcID} 延迟跟随玩家成功");
            
                npcGameData.isFollowing = true;
            
                UpdateFacingDirection();
                yield break;
            }
        
            retryCount++;
        }
    
        if (playerGameObject == null)
        {
            Debug.LogError($"NPC {npcGameData?.npcID} 重试后仍无法找到玩家对象");
        }
    }
    
    private void UpdateFacingDirection()
    {
        if (playerTransform == null || spriteRenderer == null) return;

        float xDirection = playerTransform.position.x - transform.position.x;
        spriteRenderer.flipX = xDirection < 0;
    }
    
    public virtual void StopFollowing()
    {
        GetNpcGameData().isFollowing = false;
        
        // 重置朝向
        if (spriteRenderer != null)
        {
            spriteRenderer.flipX = false;
        }
        
        Debug.Log($"NPC {GetNpcGameData()?.npcID} 停止跟随玩家");
    }

    #endregion

    #region 激活/禁用

    public virtual void ActivateNpc()
    {
        try
        {
            // 确保玩家引用存在
            if (playerGameObject == null)
            {
                CachePlayerReferences();
            }
            
            gameObject.SetActive(true);
        
            if (GetNpcGameData().isFollowing)
            {
                FollowTargetPlayer();
            }
        
            // 设置交互UI的相机引用
            SetupInteractionUICamera();
        
            Debug.Log($"NPC {npcGameData?.npcID} 已激活");
        }
        catch (Exception e)
        {
            Debug.LogError($"激活NPC时发生错误: {e.Message}");
        }
    }

    private void SetupInteractionUICamera()
    {
        if (interactionIndicator != null)
        {
            var canvas = interactionIndicator.GetComponent<Canvas>();
            if (canvas != null)
            {
                canvas.worldCamera = Camera.main;
            }
        }
    }
    
    public virtual void DeactivateNpc()
    {
        gameObject.SetActive(false);
        Debug.Log($"NPC {npcGameData?.npcID} 已禁用");
    }

    #endregion

    #region 公共方法

    public void SetCanInteract(bool canInteract)
    {
        npcGameData.canInteract = canInteract;
    }

    public void AnimationTrigger() => stateMachine.CurrentState.AnimationFinishTrigger();

    /// <summary>
    /// 重置NPC状态（用于对象池）
    /// </summary>
    public virtual void ResetNPC()
    {
        SetupNpc();
    }
    
    public void SetNpcGameData(NpcGameData _npcGameData = null)
    {
        SetupNpc(_npcGameData);
    }
    
    public NpcGameData GetNpcGameData()
    {
        return npcGameData;
    }
    
    public void SetPosition(Vector3 position)
    {
        transform.position = position;
        npcGameData.position = position;
    }

    #endregion

    #region 调试

    private void OnDrawGizmosSelected()
    {
        // 交互范围
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionDistance);
        
        // 跟随范围
        if (npcGameData.isFollowing)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, npcGameData.followDistance);
        }
    }

    #endregion
}