using System;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class NPC : Entity
{
    [Header("NPC Data")] internal NPCData npcData;
    [SerializeField] protected float followDistance = 1.5f; // 跟随距离
    [SerializeField] protected internal float followSpeed = 2f; // 跟随速度
    public SpriteRenderer spriteRenderer;
    public bool isFollowing = false; // 是否跟随玩家
    
    [Header("交互设置")]
    [SerializeField] private float interactionDistance = 2f; // 交互距离
    [SerializeField] private GameObject interactionIndicator;// 交互提示UI
    
    [Header("对话数据")]
    [SerializeField] private List<DialogueData> dialogueDataList; // 对话数据列表
    
    private bool canInteract = true; // 是否可以交互
    private DialogueData cachedDialogue; // 缓存对话数据
    private GameObject player; // 玩家引用
    private float defaultSpeed; // 当前速度
    #region State

    public NPCStateMachine stateMachine { get; set; }

    #endregion

    protected override void Awake()
    {
        base.Awake();
        
        stateMachine = new NPCStateMachine();
        
        if (npcData == null)
            npcData = baseData as NPCData;
    }
    
    protected override void Start()
    {
        base.Start();
        
        // 初始设置
        if (interactionIndicator)
            interactionIndicator.SetActive(false);
        
        // 初始化Npc
        SetupNpc();
        
        defaultSpeed = followSpeed;
    }

    protected override void Update()
    {
        base.Update();
        
        // 检查玩家是否在交互距离内
        player = GameObject.FindGameObjectWithTag("Player");
        if (player)
        {
            float distance = Vector2.Distance(transform.position, player.transform.position);
            // 检查是否在交互距离内
            bool inInteractionDistance = distance <= interactionDistance;
            
            // 更新交互提示
            if (interactionIndicator && inInteractionDistance && canInteract)
            {
                interactionIndicator.SetActive(true);
                // 处理交互输入
                if (Input.GetKeyDown(KeyCode.E))
                {
                    Debug.Log($"与{npcData.npcName}交互");
                    Interact();
                }
            }
            else
            {
                interactionIndicator.SetActive(false);
            }
        }
    }
    
    protected virtual void FixedUpdate()
    {
        // 如果NPC处于跟随状态，则更新位置
        if (PlayerPrefs.GetString("FollowingNpcID") == npcData.npcID)
        {
            FollowPlayer();
        }
    }

    public void AnimationTrigger()=> stateMachine.CurrentState.AnimationFinishTrigger();
    
    private void SetupNpc()
    {
        if (npcData)
        {
            Sprite avatar = Resources.Load<Sprite>($"Art/NPCs/{npcData.spriteID}");
            if (avatar == null)
            {
                Debug.LogWarning($"NPC {npcData.npcName} 的头像未找到，使用默认头像");
                avatar = Resources.Load<Sprite>("Art/NPCs/default_avatar");
            }
            spriteRenderer.sprite = avatar;
            
            foreach (string dialogueID in npcData.dialogueIDs)
            {
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
            StopFollowing();
        }
    }
    protected virtual void OnDialogueEnd(string obj)
    {
        int finishedCount = 0;
        // 检查对话数据列表是否全部完成了，如果是，则进行标志记录
        foreach (DialogueData dialogueData in dialogueDataList)
        {
            if (dialogueData.state == DialogueState.Finished)
            {
                finishedCount++;
            }
        }

        if (finishedCount == dialogueDataList.Count)
        {
            GameStateManager.Instance.SetFlag("FinishAllDialogue_"+ npcData.npcID, true);
            SetCanInteract(false);
        }
    }
    
    // 交互方法
    private void Interact()
    {
        // 对话交互
        TriggerDialogue();
    }
    
    private void TriggerDialogue(string dialogueID = null)
    {
        if (cachedDialogue)
        {
            _ = DialogueManager.Instance.StartDialogue(cachedDialogue);
            return;
        }
        
        if (dialogueDataList == null || dialogueDataList.Count == 0)
        {
            Debug.LogWarning("对话数据列表为空");
            return;
        }
        
        if (dialogueID != null)
        {
            DialogueData dialogue = dialogueDataList.Find(d => d.dialogueID == dialogueID);
            if (dialogue)
            {
                cachedDialogue = dialogue;
                DialogueManager.Instance.StartDialogue(cachedDialogue, OnCurrentDialogueEnd);
            }
            else
            {
                Debug.LogWarning($"对话数据 {dialogueID} 未找到");
            }
        }
        else
        {
            // 遍历对话数据列表，查找未完成的对话
            foreach (DialogueData dialogueData in dialogueDataList)
            {
                if (dialogueData.state != DialogueState.Finished)
                {
                    cachedDialogue = dialogueData;
                    _ = DialogueManager.Instance.StartDialogue(cachedDialogue, OnCurrentDialogueEnd);
                    break;
                }
            }
        }
    }
    
    // 根据对话结束的不同方式，进行不同的处理
    private void OnCurrentDialogueEnd(bool isFinished)
    {
        if (isFinished)
        {
            cachedDialogue = null;
            return;
        }
        
        // 未满足对话条件
        Debug.Log("对话条件不满足");
    }
    
    #region Follow Player
    
    public void FollowTargetPlayer()
    {
        // 设置目标玩家
        player = GameObject.FindGameObjectWithTag("Player");
        // 使NPC跟随玩家
        isFollowing = true;
        
        // 使用PlayerPrefs记录跟随的NPCID
        PlayerPrefs.SetString("FollowingNpcID", npcData.npcID);
        PlayerPrefs.Save();

        GameStateManager.Instance.SetFlag("Following_" + npcData.npcID, isFollowing);
        
        // 初始化朝向
        UpdateFacingDirection();
    }

    public void FollowPlayer()
    {
        followSpeed = defaultSpeed;
        if (player != null)
        {
            // 更新面朝方向
            UpdateFacingDirection();
            
            // 计算NPC与玩家之间的距离
            float distance = Vector2.Distance(transform.position, player.transform.position);
            
            // 如果距离小于跟随距离，则停止跟随
            if (distance < followDistance)
            {
                followSpeed = 0;
                return;
            }
            
            // NPC跟随玩家
            Vector2 direction = (player.transform.position - transform.position).normalized;
            Rb.MovePosition(Rb.position + direction * followSpeed * Time.deltaTime);
        }
    }
    
    private void UpdateFacingDirection()
    {
        if (player != null && spriteRenderer != null)
        {
            // 根据玩家位置设置朝向
            float xDirection = player.transform.position.x - transform.position.x;
        
            // 大于0表示玩家在NPC右侧，需要面朝右；小于0表示玩家在NPC左侧，需要面朝左
            spriteRenderer.flipX = xDirection < 0;
        }
    }
    
    public virtual void StopFollowing()
    {
        // 停止跟随玩家
        isFollowing = false;
        // 清空PlayerPrefs记录跟随的NPCID
        PlayerPrefs.SetString("FollowingNpcID", string.Empty);
        PlayerPrefs.Save();
        // followSpeed = 0;
        
        // 重置朝向
        if (spriteRenderer != null)
            spriteRenderer.flipX = false;
        
        // 清除跟随状态标志
        GameStateManager.Instance.SetFlag("Following_" + npcData.npcID, false);
    }

    #endregion


    public virtual void ActivateNpc()
    {
        if (GameStateManager.Instance.GetFlag("Following_" + npcData.npcID))
            FollowTargetPlayer();
        gameObject.SetActive(true);
    }
    
    public virtual void DeactivateNpc()
    {
        gameObject.SetActive(false);
    }
    
    // 设置NPC是否可以交互
    public void SetCanInteract(bool canInteract)
    {
        this.canInteract = canInteract;
    }
    private void OnDrawGizmosSelected()
    {
        // 编辑器中的可视化辅助，用于显示交互范围
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionDistance);
    }
}