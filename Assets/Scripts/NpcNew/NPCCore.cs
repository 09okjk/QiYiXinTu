using System;
using System.Collections;
using System.Collections.Generic;
using Manager;
using UnityEngine;

namespace NpcNew
{
    /// <summary>
    /// NPC核心系统 - 重构后的主要NPC类
    /// </summary>
    public class NPCCore : Entity, INPCBehavior, INPCInteractable, INPCDialogue, INPCStateController
    {
        [Header("NPC配置")]
        [SerializeField] private NPCConfiguration config;
        
        [Header("组件引用")]
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private GameObject interactionIndicator;
        [SerializeField] private NPCStateMachine stateMachine;
        
        // 核心数据
        private NPCDataNew npcData;// NPC数据
        private NPCRuntimeData runtimeData; // NPC运行时数据
        
        // 系统模块
        private NPCInteractionSystem interactionSystem; // NPC交互系统
        private NPCDialogueSystem dialogueSystem; // NPC对话系统
        private NPCFollowSystem followSystem; // NPC跟随系统
        private NPCStateSystem stateSystem; // NPC状态系统
        private NPCAnimationSystem animationSystem; // NPC动画系统
        
        // 缓存引用
        private Transform playerTransform; 
        private Camera mainCamera;
        
        // 事件系统
        public event Action<string> OnNPCStateChanged; // NPC状态变化事件
        public event Action<bool> OnInteractionAvailabilityChanged; // 交互可用性变化事件

        #region 属性实现
        public string NPCID => npcData?.npcID ?? string.Empty;
        public bool CanInteract 
        { 
            get => runtimeData.canInteract; 
            set => SetCanInteract(value); 
        }
        public bool IsFollowing 
        { 
            get => runtimeData.isFollowing; 
            set => SetFollowing(value); 
        }
        public bool IsActive 
        { 
            get => runtimeData.isActive; 
            set => SetActive(value); 
        }
        public NPCStateType CurrentStateType => stateSystem?.CurrentStateType ?? NPCStateType.Idle;
        #endregion

        #region Unity生命周期
        protected override void Awake()
        {
            base.Awake();
            InitializeSystems();
        }

        protected override void Start()
        {
            base.Start();
            InitializeNPC();
            CacheReferences();
            RegisterEvents();
        }

        protected override void Update()
        {
            base.Update();
            UpdateSystems();
        }

        private void FixedUpdate()
        {
            followSystem?.FixedUpdate();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            UnregisterEvents();
            DisposeSystems();
        }
        #endregion

        #region 初始化
        private void InitializeSystems()
        {
            runtimeData = new NPCRuntimeData();
            
            interactionSystem = new NPCInteractionSystem(this);
            dialogueSystem = new NPCDialogueSystem(this);
            followSystem = new NPCFollowSystem(this);
            stateSystem = new NPCStateSystem(this);
            animationSystem = new NPCAnimationSystem(this);

            stateMachine = new NPCStateMachine();
        }

        private void InitializeNPC()
        {
            if (config != null)
            {
                npcData = config.npcData;
                ApplyConfiguration();
            }
            
            SetupVisual();
            LoadDialogueData();
        }

        private void ApplyConfiguration()
        {
            runtimeData.followDistance = config.followDistance;
            runtimeData.followSpeed = config.followSpeed;
            runtimeData.interactionDistance = config.interactionDistance;
            runtimeData.canInteract = config.canInteractByDefault;
        }

        /// <summary>
        /// 缓存玩家和摄像机引用
        /// </summary>
        private void CacheReferences()
        {
            StartCoroutine(CacheReferencesCoroutine());
        }

        /// <summary>
        /// 协程：等待玩家和摄像机引用
        /// </summary>
        /// <returns></returns>
        private IEnumerator CacheReferencesCoroutine()
        {
            // 等待玩家加载
            while (playerTransform == null)
            {
                // var player = GameObject.FindGameObjectWithTag("Player");
                var player = PlayerManager.Instance?.player;
                if (player != null)
                {
                    playerTransform = player.gameObject.transform;
                    break;
                }
                yield return new WaitForSeconds(0.1f);
            }
            
            mainCamera = Camera.main;
            
            // 通知系统玩家已缓存
            interactionSystem?.SetPlayerTransform(playerTransform);
            followSystem?.SetPlayerTransform(playerTransform);
        }
        #endregion

        #region 系统更新
        private void UpdateSystems()
        {
            interactionSystem?.Update();
            stateSystem?.Update();
            animationSystem?.Update();
        }

        /// <summary>
        /// 注册NPC相关事件
        /// </summary>
        private void RegisterEvents()
        {
            NPCEventBus.Instance.RegisterNPC(this);
            
            if (DialogueManager.Instance != null)
            {
                DialogueManager.Instance.OnDialogueEnd += OnDialogueComplete;
            }
        }

        /// <summary>
        /// 取消注册NPC相关事件
        /// </summary>
        private void UnregisterEvents()
        {
            NPCEventBus.Instance?.UnregisterNPC(this);
            
            if (DialogueManager.Instance != null)
            {
                DialogueManager.Instance.OnDialogueEnd -= OnDialogueComplete;
            }
        }

        private void DisposeSystems()
        {
            interactionSystem?.Dispose();
            dialogueSystem?.Dispose();
            followSystem?.Dispose();
            stateSystem?.Dispose();
            animationSystem?.Dispose();
        }
        #endregion

        #region INPCBehavior实现
        public void Initialize(NPCDataNew data)
        {
            npcData = data;
            ApplyNPCData();
        }

        public void ActivateNPC()
        {
            try
            {
                gameObject.SetActive(true);
                runtimeData.isActive = true;
                
                stateSystem?.Initialize();
                SetupInteractionUI();
                
                OnNPCStateChanged?.Invoke("Activated");
                NPCLogger.Log($"NPC {NPCID} 已激活", this);
            }
            catch (Exception e)
            {
                NPCLogger.LogError($"激活NPC失败: {e.Message}", this);
            }
        }

        public void DeactivateNPC()
        {
            try
            {
                gameObject.SetActive(false);
                runtimeData.isActive = false;
                
                OnNPCStateChanged?.Invoke("Deactivated");
                NPCLogger.Log($"NPC {NPCID} 已禁用", this);
            }
            catch (Exception e)
            {
                NPCLogger.LogError($"禁用NPC失败: {e.Message}", this);
            }
        }

        public void StartInteraction()
        {
            interactionSystem?.StartInteraction();
        }

        public void StartFollowing()
        {
            followSystem?.StartFollowing();
        }

        public void StopFollowing()
        {
            followSystem?.StopFollowing();
        }
        #endregion

        #region INPCInteractable实现
        public bool IsInInteractionRange(Vector3 position)
        {
            return interactionSystem?.IsInRange(position) ?? false;
        }

        public void OnPlayerEnterRange()
        {
            interactionSystem?.OnPlayerEnterRange();
        }

        public void OnPlayerExitRange()
        {
            interactionSystem?.OnPlayerExitRange();
        }

        public void OnInteractionTrigger()
        {
            interactionSystem?.OnInteractionTrigger();
        }
        #endregion

        #region INPCDialogue实现
        public void StartDialogue(string dialogueID = null)
        {
            dialogueSystem?.StartDialogue(dialogueID);
        }

        public void OnDialogueComplete(string dialogueID)
        {
            dialogueSystem?.OnDialogueComplete(dialogueID);
        }

        public bool HasAvailableDialogue()
        {
            return dialogueSystem?.HasAvailableDialogue() ?? false;
        }
        #endregion

        #region INPCStateController实现
        public void ChangeState(NPCStateType stateType)
        {
            stateSystem?.ChangeState(stateType);
            animationSystem?.OnStateChanged(stateType);
        }
        #endregion

        #region 辅助方法
        
        /// <summary>
        /// 设置NPC的视觉表现(如精灵图像)
        /// </summary>
        private void SetupVisual()
        {
            if (npcData != null && spriteRenderer != null)
            {
                var sprite = NPCResourceManager.LoadSprite(npcData.spriteID);
                if (sprite != null)
                {
                    spriteRenderer.sprite = sprite;
                }
            }
        }

        private void LoadDialogueData()
        {
            dialogueSystem?.LoadDialogues(npcData?.dialogueIDs);
        }

        private void SetupInteractionUI()
        {
            if (interactionIndicator != null && mainCamera != null)
            {
                var canvas = interactionIndicator.GetComponent<Canvas>();
                if (canvas != null)
                {
                    canvas.worldCamera = mainCamera;
                }
            }
        }

        private void ApplyNPCData()
        {
            if (npcData == null) return;

            runtimeData.canInteract = npcData.canInteract;
            runtimeData.isFollowing = npcData.isFollowing;
            
            SetupVisual();
            LoadDialogueData();
        }

        private void SetCanInteract(bool value)
        {
            if (runtimeData.canInteract != value)
            {
                runtimeData.canInteract = value;
                OnInteractionAvailabilityChanged?.Invoke(value);
            }
        }

        private void SetFollowing(bool value)
        {
            runtimeData.isFollowing = value;
            if (value)
            {
                StartFollowing();
            }
            else
            {
                StopFollowing();
            }
        }

        private void SetActive(bool value)
        {
            runtimeData.isActive = value;
            if (value)
            {
                ActivateNPC();
            }
            else
            {
                DeactivateNPC();
            }
        }
        #endregion

        #region 公共访问器
        public NPCDataNew GetNPCData() => npcData;
        public NPCRuntimeData GetRuntimeData() => runtimeData;
        public Transform GetTransform() => transform;
        public SpriteRenderer GetSpriteRenderer() => spriteRenderer;
        public GameObject GetInteractionIndicator() => interactionIndicator;
        public NPCStateMachine GetStateMachine() => stateMachine;

        #region 动画相关
        public NPCAnimationSystem GetAnimationSystem() => animationSystem;
        
        // 添加动画控制方法
        public void SetAnimationState(NPCAnimationState state)
        {
            animationSystem?.SetAnimationState(state);
        }

        public void SetAnimationBool(string paramName, bool value)
        {
            animationSystem?.SetBool(paramName, value);
        }

        public void SetAnimationFloat(string paramName, float value)
        {
            animationSystem?.SetFloat(paramName, value);
        }

        public void SetAnimationTrigger(string paramName)
        {
            animationSystem?.SetTrigger(paramName);
        }

        public void PlayAnimation(string animationName)
        {
            animationSystem?.PlayAnimation(animationName);
        }

        public void LockAnimation(float duration)
        {
            animationSystem?.LockAnimation(duration);
        }

        public void UnlockAnimation()
        {
            animationSystem?.UnlockAnimation();
        }

        public bool IsAnimationLocked => animationSystem?.IsAnimationLocked ?? false;
        #endregion
        
        #endregion
    }
}