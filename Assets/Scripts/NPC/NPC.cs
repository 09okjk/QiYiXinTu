using System;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using System.Collections;

public class NPC : Entity
{
    [Header("NPC Data")] 
    internal NPCData npcData;
    
    [Header("跟随设置")]
    [SerializeField] protected float followDistance = 1.5f;
    [SerializeField] protected internal float followSpeed = 2f;
    
    [Header("渲染设置")]
    public SpriteRenderer spriteRenderer;
    
    [Header("状态设置")]
    public bool isFollowing = false;
    public bool isActive = true;
    
    [Header("交互设置")]
    [SerializeField] private float interactionDistance = 2f;
    [SerializeField] private GameObject interactionIndicator;
    public bool canInteract = true;
    
    [Header("对话数据")]
    public List<string> dialogueIDs;
    
    // 私有字段
    private List<DialogueData> dialogueDataList = new List<DialogueData>();
    private DialogueData cachedDialogue;
    private Transform playerTransform; // 缓存玩家Transform
    private GameObject playerGameObject; // 缓存玩家GameObject
    private float defaultSpeed;
    private bool isPlayerInRange = false;
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
        
        if (npcData == null)
            npcData = baseData as NPCData;
            
        // 从数据初始化状态
        if (npcData != null)
        {
            InitializeFromData();
        }
    }
    
    protected override void Start()
    {
        base.Start();
        
        // 初始设置
        InitializeInteractionUI();
        
        // 设置NPC
        SetupNPC();
        
        defaultSpeed = followSpeed;
        
        // 缓存玩家引用
        CachePlayerReferences();
        
        // 延迟订阅事件
        StartCoroutine(DelayedEventSubscription());
    }

    protected override void Update()
    {
        base.Update();
        
        // 优化的玩家检查
        if (Time.time - lastPlayerCheckTime >= playerCheckInterval)
        {
            CheckPlayerInteraction();
            lastPlayerCheckTime = Time.time;
        }
        
        // 处理交互输入
        HandleInteractionInput();
    }
    
    protected virtual void FixedUpdate()
    {
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

    private void InitializeFromData()
    {
        if (npcData != null)
        {
            isFollowing = npcData.isFollowing;
            canInteract = npcData.canInteract;
            dialogueIDs = new List<string>(npcData.dialogueIDs);
        }
    }

    private void InitializeInteractionUI()
    {
        if (interactionIndicator != null)
        {
            interactionIndicator.SetActive(false);
        }
    }

    private void CachePlayerReferences()
    {
        try
        {
            playerGameObject = GameObject.FindGameObjectWithTag("Player");
            if (playerGameObject != null)
            {
                playerTransform = playerGameObject.transform;
                Debug.Log($"NPC {npcData?.npcID} 成功缓存玩家引用");
            }
            else
            {
                Debug.LogWarning($"NPC {npcData?.npcID} 未找到玩家对象");
                // 设置重试机制
                StartCoroutine(RetryPlayerCaching());
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
                Debug.Log($"NPC {npcData?.npcID} 延迟缓存玩家引用成功");
                yield break;
            }
            
            retryCount++;
        }
        
        if (playerGameObject == null)
        {
            Debug.LogError($"NPC {npcData?.npcID} 无法找到玩家对象");
        }
    }

    private IEnumerator DelayedEventSubscription()
    {
        yield return new WaitForSeconds(0.1f);
        SubscribeToEvents();
    }

    #endregion

    #region 事件管理

    private void SubscribeToEvents()
    {
        if (hasSubscribedToEvents) return;

        try
        {
            if (DialogueManager.Instance != null)
            {
                DialogueManager.Instance.OnDialogueEnd += OnDialogueEnd;
                hasSubscribedToEvents = true;
                Debug.Log($"NPC {npcData?.npcID} 成功订阅对话事件");
            }
            else
            {
                Debug.LogWarning($"NPC {npcData?.npcID} DialogueManager.Instance为空，延迟重试");
                StartCoroutine(RetryEventSubscription());
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"订阅事件时发生错误: {e.Message}");
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
                Debug.Log($"NPC {npcData?.npcID} 延迟订阅对话事件成功");
                yield break;
            }
            
            retryCount++;
        }
    }

    private void UnsubscribeFromEvents()
    {
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

    private void SetupNPC()
    {
        if (npcData == null) return;

        try
        {
            // 设置精灵
            SetupSprite();
            
            // 加载对话数据
            LoadDialogueData();
            
            // 初始化跟随状态
            if (!isFollowing)
            {
                StopFollowing();
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"设置NPC时发生错误: {e.Message}");
        }
    }

    private void SetupSprite()
    {
        if (spriteRenderer == null || string.IsNullOrEmpty(npcData.spriteID)) return;

        Sprite avatar = Resources.Load<Sprite>($"Art/NPCs/{npcData.spriteID}");
        if (avatar == null)
        {
            Debug.LogWarning($"NPC {npcData.npcName} 的头像未找到，使用默认头像");
            avatar = Resources.Load<Sprite>("Art/NPCs/default_avatar");
        }
        
        if (avatar != null)
        {
            spriteRenderer.sprite = avatar;
        }
    }

    private void LoadDialogueData()
    {
        dialogueDataList.Clear();
        
        if (dialogueIDs == null || dialogueIDs.Count == 0) return;

        foreach (string dialogueID in dialogueIDs)
        {
            if (string.IsNullOrEmpty(dialogueID)) continue;

            DialogueData dialogue = Resources.Load<DialogueData>($"ScriptableObjects/Dialogues/{dialogueID}");
            if (dialogue != null)
            {
                dialogueDataList.Add(dialogue);
            }
            else
            {
                Debug.LogWarning($"无法找到对话数据: {dialogueID}");
            }
        }
    }

    #endregion

    #region 交互系统

    private void CheckPlayerInteraction()
    {
        if (playerTransform == null || !canInteract) 
        {
            UpdateInteractionUI(false);
            return;
        }

        float distance = Vector2.Distance(transform.position, playerTransform.position);
        bool inRange = distance <= interactionDistance;
        
        if (inRange != isPlayerInRange)
        {
            isPlayerInRange = inRange;
            UpdateInteractionUI(inRange);
        }
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
        if (isPlayerInRange && canInteract && Input.GetKeyDown(KeyCode.E))
        {
            Debug.Log($"与{npcData?.npcName}交互");
            TriggerDialogue();
        }
    }

    #endregion

    #region 对话系统

    private void TriggerDialogue(string dialogueID = null)
    {
        try
        {
            if (cachedDialogue != null)
            {
                StartCachedDialogue();
                return;
            }
            
            if (dialogueDataList == null || dialogueDataList.Count == 0)
            {
                Debug.LogWarning($"NPC {npcData?.npcID} 对话数据列表为空");
                return;
            }
            
            DialogueData targetDialogue = FindTargetDialogue(dialogueID);
            if (targetDialogue != null)
            {
                StartDialogue(targetDialogue);
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"触发对话时发生错误: {e.Message}");
        }
    }

    private void StartCachedDialogue()
    {
        if (DialogueManager.Instance != null)
        {
            _ = DialogueManager.Instance.StartDialogue(cachedDialogue);
        }
    }

    private DialogueData FindTargetDialogue(string dialogueID)
    {
        if (!string.IsNullOrEmpty(dialogueID))
        {
            return dialogueDataList.Find(d => d.dialogueID == dialogueID);
        }
        
        // 找到第一个未完成的对话
        return dialogueDataList.Find(d => d.state != DialogueState.Finished);
    }

    private void StartDialogue(DialogueData dialogue)
    {
        if (DialogueManager.Instance != null)
        {
            cachedDialogue = dialogue;
            _ = DialogueManager.Instance.StartDialogue(dialogue, OnCurrentDialogueEnd);
        }
    }

    protected virtual void OnDialogueEnd(string dialogueID)
    {
        try
        {
            CheckAllDialoguesCompleted();
        }
        catch (Exception e)
        {
            Debug.LogError($"处理对话结束时发生错误: {e.Message}");
        }
    }

    private void CheckAllDialoguesCompleted()
    {
        int finishedCount = 0;
        
        foreach (DialogueData dialogueData in dialogueDataList)
        {
            if (dialogueData.state == DialogueState.Finished)
            {
                finishedCount++;
            }
        }

        if (finishedCount == dialogueDataList.Count)
        {
            OnAllDialoguesCompleted();
        }
        else
        {
            SetCanInteract(true);
        }
    }

    private void OnAllDialoguesCompleted()
    {
        if (GameStateManager.Instance != null)
        {
            GameStateManager.Instance.SetFlag("FinishAllDialogue_" + npcData?.npcID, true);
        }
        SetCanInteract(false);
    }
    
    private void OnCurrentDialogueEnd(bool isFinished)
    {
        if (isFinished)
        {
            cachedDialogue = null;
        }
        else
        {
            Debug.Log("对话条件不满足");
        }
    }

    #endregion

    #region 跟随系统

    private bool ShouldFollowPlayer()
    {
        if (!isFollowing || playerTransform == null) return false;
        
        // 检查GameStateManager中的跟随状态
        if (GameStateManager.Instance != null)
        {
            return GameStateManager.Instance.GetFlag("Following_" + npcData?.npcID);
        }
        
        return false;
    }

    public void FollowTargetPlayer()
    {
        if (playerGameObject == null)
        {
            Debug.LogError("玩家对象引用为空，无法开始跟随");
            return;
        }

        isFollowing = true;
        
        // 设置游戏状态标志
        if (GameStateManager.Instance != null)
        {
            GameStateManager.Instance.SetFlag("Following_" + npcData?.npcID, true);
        }
        
        UpdateFacingDirection();
        
        Debug.Log($"NPC {npcData?.npcID} 开始跟随玩家");
    }

    public void FollowPlayer()
    {
        if (playerTransform == null) return;

        followSpeed = defaultSpeed;
        
        UpdateFacingDirection();
        
        float distance = Vector2.Distance(transform.position, playerTransform.position);
        
        if (distance < followDistance)
        {
            followSpeed = 0;
            return;
        }
        
        Vector2 direction = (playerTransform.position - transform.position).normalized;
        if (Rb != null)
        {
            Rb.MovePosition(Rb.position + direction * followSpeed * Time.fixedDeltaTime);
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
        isFollowing = false;
        
        // 清除游戏状态标志
        if (GameStateManager.Instance != null)
        {
            GameStateManager.Instance.SetFlag("Following_" + npcData?.npcID, false);
        }
        
        // 重置朝向
        if (spriteRenderer != null)
        {
            spriteRenderer.flipX = false;
        }
        
        Debug.Log($"NPC {npcData?.npcID} 停止跟随玩家");
    }

    #endregion

    #region 激活/禁用

    public virtual void ActivateNpc()
    {
        try
        {
            if (isFollowing)
            {
                FollowTargetPlayer();
            }
            
            // 设置交互UI的相机引用
            SetupInteractionUICamera();
            
            gameObject.SetActive(true);
            
            Debug.Log($"NPC {npcData?.npcID} 已激活");
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
        Debug.Log($"NPC {npcData?.npcID} 已禁用");
    }

    #endregion

    #region 公共方法

    public void SetCanInteract(bool canInteract)
    {
        this.canInteract = canInteract;
        
        if (!canInteract)
        {
            UpdateInteractionUI(false);
        }
    }

    public void AnimationTrigger() => stateMachine.CurrentState.AnimationFinishTrigger();

    /// <summary>
    /// 重置NPC状态（用于对象池）
    /// </summary>
    public virtual void ResetNPC()
    {
        StopFollowing();
        SetCanInteract(true);
        cachedDialogue = null;
        isPlayerInRange = false;
        UpdateInteractionUI(false);
    }

    /// <summary>
    /// 获取NPC当前状态信息
    /// </summary>
    public NPCStatusInfo GetStatusInfo()
    {
        return new NPCStatusInfo
        {
            npcID = npcData?.npcID,
            isFollowing = this.isFollowing,
            canInteract = this.canInteract,
            isActive = this.isActive,
            position = transform.position
        };
    }

    #endregion

    #region 调试

    private void OnDrawGizmosSelected()
    {
        // 交互范围
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionDistance);
        
        // 跟随范围
        if (isFollowing)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, followDistance);
        }
    }

    #endregion
}

/// <summary>
/// NPC状态信息结构体
/// </summary>
[System.Serializable]
public struct NPCStatusInfo
{
    public string npcID;
    public bool isFollowing;
    public bool canInteract;
    public bool isActive;
    public Vector3 position;
}