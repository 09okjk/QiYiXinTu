using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NpcNew
{
    /// <summary>
    /// NPC运行时数据
    /// </summary>
    [System.Serializable]
    public class NPCRuntimeData
    {
        public bool canInteract = true;
        public bool isFollowing = false;
        public bool isActive = false;
        public float followDistance = 1.5f;
        public float followSpeed = 2f;
        public float interactionDistance = 2f;
        public Vector3 lastPosition;
        public float lastInteractionTime;
    }

    /// <summary>
    /// NPC配置
    /// </summary>
    [CreateAssetMenu(fileName = "NPCConfiguration", menuName = "NPC/Configuration")]
    public class NPCConfiguration : ScriptableObject
    {
        [Header("基础数据")]
        public NPCDataNew npcData;
        
        [Header("交互设置")]
        public float interactionDistance = 2f;
        public bool canInteractByDefault = true;
        
        [Header("跟随设置")]
        public float followDistance = 1.5f;
        public float followSpeed = 2f;
        
        [Header("性能设置")]
        public float updateInterval = 0.1f; // 更新间隔时间
        public bool enableOptimizations = true; // 是否启用性能优化
    }

    /// <summary>
    /// NPC交互系统
    /// </summary>
    public class NPCInteractionSystem : IDisposable
    {
        private NPCCore npcCore;
        private Transform playerTransform;
        private bool isPlayerInRange = false;
        private float lastCheckTime = 0f;
        private const float CHECK_INTERVAL = 0.1f;

        public NPCInteractionSystem(NPCCore core)
        {
            npcCore = core;
        }

        public void SetPlayerTransform(Transform player)
        {
            playerTransform = player;
        }

        public void Update()
        {
            if (Time.time - lastCheckTime >= CHECK_INTERVAL)
            {
                CheckPlayerDistance();
                lastCheckTime = Time.time;
            }

            HandleInput();
        }

        /// <summary>
        /// 检查玩家与NPC的距离
        /// </summary>
        private void CheckPlayerDistance()
        {
            if (playerTransform == null || !npcCore.CanInteract) return;

            bool inRange = IsInRange(playerTransform.position);
            
            if (inRange != isPlayerInRange)
            {
                isPlayerInRange = inRange;
                
                if (inRange)
                {
                    OnPlayerEnterRange();
                }
                else
                {
                    OnPlayerExitRange();
                }
            }
        }

        /// <summary>
        /// 检查玩家位置是否在NPC的交互范围内
        /// </summary>
        /// <param name="position">玩家位置</param>
        public bool IsInRange(Vector3 position)
        {
            float distance = Vector3.Distance(npcCore.GetTransform().position, position);
            return distance <= npcCore.GetRuntimeData().interactionDistance;
        }

        /// <summary>
        /// 玩家进入NPC交互范围时调用
        /// </summary>
        public void OnPlayerEnterRange()
        {
            var indicator = npcCore.GetInteractionIndicator();
            if (indicator != null)
            {
                indicator.SetActive(true);
            }
        }

        /// <summary>
        /// 玩家离开NPC交互范围时调用
        /// </summary>
        public void OnPlayerExitRange()
        {
            var indicator = npcCore.GetInteractionIndicator();
            if (indicator != null)
            {
                indicator.SetActive(false);
            }
        }

        /// <summary>
        /// 处理玩家输入
        /// </summary>
        private void HandleInput()
        {
            if (isPlayerInRange && Input.GetKeyDown(KeyCode.E))
            {
                OnInteractionTrigger();
            }
        }

        public void OnInteractionTrigger()
        {
            NPCLogger.Log($"与NPC {npcCore.NPCID} 开始交互", npcCore);
            npcCore.StartDialogue();
        }

        public void StartInteraction()
        {
            OnInteractionTrigger();
        }

        public void Dispose()
        {
            playerTransform = null;
            npcCore = null;
        }
    }

    /// <summary>
    /// NPC对话系统
    /// </summary>
    public class NPCDialogueSystem : IDisposable
    {
        private NPCCore npcCore;
        private List<DialogueData> dialogueDataList = new List<DialogueData>();
        private DialogueData cachedDialogue;

        public NPCDialogueSystem(NPCCore core)
        {
            npcCore = core;
        }

        public void LoadDialogues(List<string> dialogueIDs)
        {
            if (dialogueIDs == null) return;

            dialogueDataList.Clear();
            
            foreach (string dialogueID in dialogueIDs)
            {
                var dialogue = NPCResourceManager.LoadDialogue(dialogueID);
                if (dialogue != null)
                {
                    dialogueDataList.Add(dialogue);
                }
            }
        }

        public void StartDialogue(string dialogueID = null)
        {
            try
            {
                DialogueData targetDialogue = FindTargetDialogue(dialogueID);
                if (targetDialogue != null)
                {
                    cachedDialogue = targetDialogue;
                    DialogueManager.Instance?.StartDialogue(targetDialogue, OnDialogueEnd);
                }
            }
            catch (Exception e)
            {
                NPCLogger.LogError($"启动对话失败: {e.Message}", npcCore);
            }
        }

        private DialogueData FindTargetDialogue(string dialogueID)
        {
            if (!string.IsNullOrEmpty(dialogueID))
            {
                return dialogueDataList.Find(d => d.dialogueID == dialogueID);
            }
            
            return dialogueDataList.Find(d => d.state != DialogueState.Finished);
        }

        private void OnDialogueEnd(bool isFinished)
        {
            if (isFinished)
            {
                cachedDialogue = null;
            }
        }

        public void OnDialogueComplete(string dialogueID)
        {
            // 检查所有对话是否完成
            bool allCompleted = true;
            foreach (var dialogue in dialogueDataList)
            {
                if (dialogue.state != DialogueState.Finished)
                {
                    allCompleted = false;
                    break;
                }
            }

            if (allCompleted)
            {
                OnAllDialoguesCompleted();
            }
        }

        private void OnAllDialoguesCompleted()
        {
            GameStateManager.Instance?.SetFlag($"FinishAllDialogue_{npcCore.NPCID}", true);
            npcCore.CanInteract = false;
        }

        public bool HasAvailableDialogue()
        {
            return dialogueDataList.Exists(d => d.state != DialogueState.Finished);
        }

        public void Dispose()
        {
            dialogueDataList.Clear();
            cachedDialogue = null;
            npcCore = null;
        }
    }

    /// <summary>
    /// NPC跟随系统
    /// </summary>
    public class NPCFollowSystem : IDisposable
    {
        private NPCCore npcCore;
        private Transform playerTransform;
        private bool isFollowing = false;

        public NPCFollowSystem(NPCCore core)
        {
            npcCore = core;
        }

        public void SetPlayerTransform(Transform player)
        {
            playerTransform = player;
        }

        public void StartFollowing()
        {
            if (playerTransform == null)
            {
                NPCLogger.LogWarning("无法开始跟随：玩家引用为空", npcCore);
                return;
            }

            isFollowing = true;
            GameStateManager.Instance?.SetFlag($"Following_{npcCore.NPCID}", true);
            
            UpdateFacingDirection();
            NPCLogger.Log($"NPC {npcCore.NPCID} 开始跟随玩家", npcCore);
        }

        public void StopFollowing()
        {
            isFollowing = false;
            GameStateManager.Instance?.SetFlag($"Following_{npcCore.NPCID}", false);
            
            ResetFacing();
            NPCLogger.Log($"NPC {npcCore.NPCID} 停止跟随玩家", npcCore);
        }

        public void FixedUpdate()
        {
            if (!isFollowing || playerTransform == null) return;

            UpdateFollowMovement();
            UpdateFacingDirection();
        }

        private void UpdateFollowMovement()
        {
            var runtimeData = npcCore.GetRuntimeData();
            var npcTransform = npcCore.GetTransform();
            
            float distance = Vector3.Distance(npcTransform.position, playerTransform.position);
            
            if (distance <= runtimeData.followDistance)
            {
                npcCore.SetZeroVelocity();
                npcCore.ChangeState(NPCStateType.Idle);
                return;
            }

            Vector2 direction = (playerTransform.position - npcTransform.position).normalized;
            npcCore.SetVelocity(direction.x * runtimeData.followSpeed, npcCore.Rb.linearVelocity.y);
            npcCore.ChangeState(NPCStateType.Move);
        }

        private void UpdateFacingDirection()
        {
            if (playerTransform == null) return;

            var spriteRenderer = npcCore.GetSpriteRenderer();
            if (spriteRenderer != null)
            {
                float xDirection = playerTransform.position.x - npcCore.GetTransform().position.x;
                spriteRenderer.flipX = xDirection < 0;
            }
        }

        private void ResetFacing()
        {
            var spriteRenderer = npcCore.GetSpriteRenderer();
            if (spriteRenderer != null)
            {
                spriteRenderer.flipX = false;
            }
        }

        public void Dispose()
        {
            playerTransform = null;
            npcCore = null;
        }
    }

    /// <summary>
    /// NPC状态系统
    /// </summary>
    public class NPCStateSystem : IDisposable
    {
        private NPCCore npcCore;
        private NPCStateType currentStateType = NPCStateType.Idle;

        public NPCStateType CurrentStateType => currentStateType;
        public NPCStateSystem(NPCCore core)
        {
            npcCore = core;
        }

        public void Initialize()
        {
            ChangeState(NPCStateType.Idle);
        }

        public void Update()
        {
            // 状态更新逻辑
        }

        public void ChangeState(NPCStateType newStateType)
        {
            if (currentStateType == newStateType) return;

            currentStateType = newStateType;
            NPCLogger.Log($"NPC {npcCore.NPCID} 状态变更为 {newStateType}", npcCore);
        }

        public void Dispose()
        {
            npcCore = null;
        }
    }
    
    /// <summary>
    /// NPC动画系统 - 统一管理NPC的动画控制
    /// </summary>
    public class NPCAnimationSystem : IDisposable
    {
        private NPCCore npcCore;
        private Animator animator;
        private NPCAnimationConfig animationConfig;
        private NPCAnimationState currentAnimationState;
        
        // 动画参数缓存
        private Dictionary<string, int> animationParameters = new Dictionary<string, int>();
        private Dictionary<NPCStateType, NPCAnimationState> stateAnimationMap = new Dictionary<NPCStateType, NPCAnimationState>();
        
        // 动画事件处理
        public event Action<string> OnAnimationEvent;
        public event Action<NPCAnimationState> OnAnimationStateChanged;
        
        // 动画控制标志
        private bool isAnimationLocked = false;
        private float animationLockTimer = 0f;

        public NPCAnimationSystem(NPCCore core)
        {
            npcCore = core;
            InitializeAnimationSystem();
        }

        #region 初始化
        private void InitializeAnimationSystem()
        {
            // 获取Animator组件
            animator = npcCore.GetComponentInChildren<Animator>();
            if (animator == null)
            {
                NPCLogger.LogWarning("未找到Animator组件", npcCore);
                return;
            }

            // 加载动画配置
            LoadAnimationConfig();
            
            // 缓存动画参数
            CacheAnimationParameters();
            
            // 建立状态到动画的映射
            BuildStateAnimationMapping();
            
            // 设置初始状态
            SetAnimationState(NPCAnimationState.Idle);
        }

        private void LoadAnimationConfig()
        {
            string configPath = $"ScriptableObjects/NPCs/Animations/{npcCore.NPCID}_AnimationConfig";
            animationConfig = Resources.Load<NPCAnimationConfig>(configPath);
            
            if (animationConfig == null)
            {
                // 创建默认配置
                animationConfig = CreateDefaultAnimationConfig();
                NPCLogger.LogWarning($"未找到动画配置，使用默认配置: {npcCore.NPCID}", npcCore);
            }
        }

        private NPCAnimationConfig CreateDefaultAnimationConfig()
        {
            var config = ScriptableObject.CreateInstance<NPCAnimationConfig>();
            config.InitializeDefaults();
            return config;
        }

        private void CacheAnimationParameters()
        {
            if (animator == null || animator.runtimeAnimatorController == null) return;

            // 缓存所有动画参数的Hash值以提高性能
            foreach (var param in animator.parameters)
            {
                animationParameters[param.name] = Animator.StringToHash(param.name);
            }
        }

        private void BuildStateAnimationMapping()
        {
            stateAnimationMap[NPCStateType.Idle] = NPCAnimationState.Idle;
            stateAnimationMap[NPCStateType.Move] = NPCAnimationState.Walk;
            stateAnimationMap[NPCStateType.Interact] = NPCAnimationState.Interact;
            stateAnimationMap[NPCStateType.Follow] = NPCAnimationState.Walk;
            stateAnimationMap[NPCStateType.Anxious] = NPCAnimationState.Anxious;
            stateAnimationMap[NPCStateType.Disabled] = NPCAnimationState.Idle;
        }
        #endregion

        #region 动画控制
        public void Update()
        {
            UpdateAnimationLock();
            UpdateAnimationParameters();
        }

        private void UpdateAnimationLock()
        {
            if (isAnimationLocked)
            {
                animationLockTimer -= Time.deltaTime;
                if (animationLockTimer <= 0f)
                {
                    isAnimationLocked = false;
                }
            }
        }

        private void UpdateAnimationParameters()
        {
            if (animator == null) return;

            // 更新速度参数
            var rb = npcCore.Rb;
            if (rb != null)
            {
                SetFloat("xVelocity", Mathf.Abs(rb.linearVelocity.x));
                SetFloat("yVelocity", rb.linearVelocity.y);
                SetFloat("totalVelocity", rb.linearVelocity.magnitude);
            }

            // 更新朝向参数
            var spriteRenderer = npcCore.GetSpriteRenderer();
            if (spriteRenderer != null)
            {
                SetBool("facingRight", !spriteRenderer.flipX);
            }
        }

        public void OnStateChanged(NPCStateType newState)
        {
            if (stateAnimationMap.TryGetValue(newState, out NPCAnimationState animState))
            {
                SetAnimationState(animState);
            }
        }

        public void SetAnimationState(NPCAnimationState newState)
        {
            if (isAnimationLocked || currentAnimationState == newState) return;

            // 退出当前状态
            ExitCurrentAnimationState();

            // 进入新状态
            currentAnimationState = newState;
            EnterAnimationState(newState);

            OnAnimationStateChanged?.Invoke(newState);
            NPCLogger.Log($"动画状态变更为: {newState}", npcCore);
        }

        private void EnterAnimationState(NPCAnimationState state)
        {
            string stateName = GetAnimationStateName(state);
            if (!string.IsNullOrEmpty(stateName))
            {
                SetBool(stateName, true);
            }

            // 执行特定状态的进入逻辑
            switch (state)
            {
                case NPCAnimationState.Anxious:
                    HandleAnxiousStateEnter();
                    break;
                case NPCAnimationState.Interact:
                    HandleInteractStateEnter();
                    break;
            }
        }

        private void ExitCurrentAnimationState()
        {
            if (currentAnimationState == NPCAnimationState.None) return;

            string stateName = GetAnimationStateName(currentAnimationState);
            if (!string.IsNullOrEmpty(stateName))
            {
                SetBool(stateName, false);
            }
        }

        private string GetAnimationStateName(NPCAnimationState state)
        {
            return animationConfig?.GetAnimationParameterName(state) ?? state.ToString();
        }
        #endregion

        #region 特殊状态处理
        private void HandleAnxiousStateEnter()
        {
            // 锁定动画，防止状态切换
            LockAnimation(animationConfig?.anxiousDuration ?? 2f);
        }

        private void HandleInteractStateEnter()
        {
            // 交互动画可能需要特殊处理
            LockAnimation(0.5f);
        }

        public void LockAnimation(float duration)
        {
            isAnimationLocked = true;
            animationLockTimer = duration;
        }

        public void UnlockAnimation()
        {
            isAnimationLocked = false;
            animationLockTimer = 0f;
        }
        #endregion

        #region 动画参数设置
        public void SetBool(string paramName, bool value)
        {
            if (animator == null || !animationParameters.ContainsKey(paramName)) return;

            try
            {
                animator.SetBool(animationParameters[paramName], value);
            }
            catch (Exception e)
            {
                NPCLogger.LogError($"设置Bool参数失败 {paramName}: {e.Message}", npcCore);
            }
        }

        public void SetFloat(string paramName, float value)
        {
            if (animator == null || !animationParameters.ContainsKey(paramName)) return;

            try
            {
                animator.SetFloat(animationParameters[paramName], value);
            }
            catch (Exception e)
            {
                NPCLogger.LogError($"设置Float参数失败 {paramName}: {e.Message}", npcCore);
            }
        }

        public void SetInteger(string paramName, int value)
        {
            if (animator == null || !animationParameters.ContainsKey(paramName)) return;

            try
            {
                animator.SetInteger(animationParameters[paramName], value);
            }
            catch (Exception e)
            {
                NPCLogger.LogError($"设置Integer参数失败 {paramName}: {e.Message}", npcCore);
            }
        }

        public void SetTrigger(string paramName)
        {
            if (animator == null || !animationParameters.ContainsKey(paramName)) return;

            try
            {
                animator.SetTrigger(animationParameters[paramName]);
            }
            catch (Exception e)
            {
                NPCLogger.LogError($"设置Trigger参数失败 {paramName}: {e.Message}", npcCore);
            }
        }
        #endregion

        #region 动画事件处理
        public void OnAnimationEventTriggered(string eventName)
        {
            OnAnimationEvent?.Invoke(eventName);
            
            // 处理内置动画事件
            HandleBuiltInAnimationEvents(eventName);
        }

        private void HandleBuiltInAnimationEvents(string eventName)
        {
            switch (eventName)
            {
                case "AnimationFinished":
                    HandleAnimationFinished();
                    break;
                case "AnxiousEnd":
                    HandleAnxiousEnd();
                    break;
                case "InteractionComplete":
                    HandleInteractionComplete();
                    break;
            }
        }

        private void HandleAnimationFinished()
        {
            UnlockAnimation();
        }

        private void HandleAnxiousEnd()
        {
            UnlockAnimation();
            SetAnimationState(NPCAnimationState.Idle);
            
            // 特殊处理：激活敌人（如果是LuXinsheng）
            if (npcCore.NPCID == "LuXinsheng")
            {
                EnemyManager.Instance?.ActivateEnemy(EnemyType.Enemy1);
            }
        }

        private void HandleInteractionComplete()
        {
            UnlockAnimation();
            SetAnimationState(NPCAnimationState.Idle);
        }
        #endregion

        #region 公共接口
        public bool IsAnimationLocked => isAnimationLocked;
        public NPCAnimationState CurrentAnimationState => currentAnimationState;
        public Animator GetAnimator() => animator;

        public bool HasParameter(string paramName)
        {
            return animationParameters.ContainsKey(paramName);
        }

        public void PlayAnimation(string animationName, int layer = 0)
        {
            if (animator == null) return;

            try
            {
                animator.Play(animationName, layer);
            }
            catch (Exception e)
            {
                NPCLogger.LogError($"播放动画失败 {animationName}: {e.Message}", npcCore);
            }
        }

        public void CrossFadeAnimation(string animationName, float duration = 0.2f, int layer = 0)
        {
            if (animator == null) return;

            try
            {
                animator.CrossFade(animationName, duration, layer);
            }
            catch (Exception e)
            {
                NPCLogger.LogError($"交叉淡入动画失败 {animationName}: {e.Message}", npcCore);
            }
        }
        #endregion

        #region 清理
        public void Dispose()
        {
            OnAnimationEvent = null;
            OnAnimationStateChanged = null;
            animationParameters.Clear();
            stateAnimationMap.Clear();
            
            animator = null;
            npcCore = null;
            animationConfig = null;
        }
        #endregion
    }
}