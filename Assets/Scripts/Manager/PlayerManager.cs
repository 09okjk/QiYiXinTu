using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace Manager
{
    public class PlayerManager : MonoBehaviour
    {
        public static PlayerManager Instance { get; private set; }
        
        [Header("玩家设置")]
        [SerializeField] private GameObject playerPrefab;
        [SerializeField] private bool autoCreatePlayer = false;
        
        [HideInInspector] public Player player;
        
        // 原始玩家数据（只读）
        private PlayerData originalPlayerData;
        // 运行时玩家数据副本（可修改）
        private PlayerGameData runtimePlayerData;
        
        // 玩家状态
        private bool isPlayerInitialized = false;
        private Camera currentCamera;

        #region Unity生命周期
        
        private void Awake()
        {
            InitializeSingleton();
            LoadPlayerData();
            
            if (autoCreatePlayer)
            {
                CreatePlayer();
            }
        }
        
        private void Start()
        {
            SubscribeToEvents();
            Debug.Log("PlayerManager 启动完成");
        }
        
        private void OnDestroy()
        {
            UnsubscribeFromEvents();
            CleanupRuntimeData();
        }

        #endregion

        #region 初始化

        private void InitializeSingleton()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                Debug.Log("PlayerManager 初始化完成");
            }
            else
            {
                Debug.LogWarning("发现多个PlayerManager实例，销毁重复实例");
                Destroy(gameObject);
            }
        }


        /// <summary>
        /// 加载玩家数据
        /// </summary>
        /// <param name="playerGameData">如果提供了玩家数据，则使用该数据，否则加载默认数据</param>
        private bool LoadPlayerData(PlayerGameData playerGameData = null)
        {
            try
            {
                if (playerGameData == null)
                {
                    // 假设玩家数据存储在Resources文件夹中
                    originalPlayerData = Resources.Load<PlayerData>("ScriptableObjects/Player/DefaultPlayerData");
                    if (originalPlayerData != null)
                    {
                        // 使用增强后的工具类创建运行时副本
                        runtimePlayerData = new PlayerGameData
                        {
                            MaxHealth = originalPlayerData.MaxHealth,
                            CurrentHealth = originalPlayerData.MaxHealth,
                            MaxMana = originalPlayerData.MaxMana,
                            CurrentMana = originalPlayerData.MaxMana,
                            InvincibleTime = originalPlayerData.InvincibleTime,
                            knockbackDirection = originalPlayerData.knockbackDirection,
                            KnockbackDuration = originalPlayerData.KnockbackDuration,
                            itemIDs = new List<string>(originalPlayerData.itemIDs),
                            playerID = originalPlayerData.playerID,
                            playerName = originalPlayerData.playerName,
                            playerPosition = Vector3.zero, // 初始位置可以设置为零或其他默认值
                            moveSpeed = originalPlayerData.moveSpeed,
                            jumpForce = originalPlayerData.jumpForce,
                            wallJumpForce = originalPlayerData.wallJumpForce,
                            idleToMoveTransitionTime = originalPlayerData.idleToMoveTransitionTime,
                            comboTimeWindow = originalPlayerData.comboTimeWindow,
                            counterAttackDuration = originalPlayerData.counterAttackDuration,
                            attackDamage = originalPlayerData.attackDamage
                        };
                        Debug.Log("成功加载并创建玩家数据运行时数据");
                    }
                    else
                    {
                        Debug.LogWarning("未找到原始玩家数据，将使用默认设置");
                    }
                }
                else
                {
                    runtimePlayerData = playerGameData;
                }

                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"加载玩家数据时发生错误: {e.Message}");
                return false;
            }
        }

        /// <summary>
        /// 重置玩家数据到原始状态
        /// </summary>
        public void ResetPlayerData()
        {
            if (originalPlayerData != null && runtimePlayerData != null)
            {
                LoadPlayerData();
                player.playerData = runtimePlayerData; // 更新玩家数据
            }
        }

        /// <summary>
        /// 清理运行时数据
        /// </summary>
        private void CleanupRuntimeData()
        {
            runtimePlayerData = null;
        }

        public void CreatePlayer()
        {
            if (playerPrefab == null)
            {
                Debug.LogError("PlayerPrefab 未设置！");
                return;
            }

            if (player != null)
            {
                Debug.LogWarning("玩家已存在，跳过创建");
                return;
            }
            
            if (runtimePlayerData == null)
            {
                Debug.LogError("运行时玩家数据未加载，无法创建玩家");
                return;
            }

            try
            {
                GameObject playerObject = Instantiate(playerPrefab, transform);
                player = playerObject.GetComponent<Player>();
        
                if (player == null)
                {
                    Debug.LogError("PlayerPrefab 上未找到 Player 组件！");
                    Destroy(playerObject);
                    return;
                }

                // 验证Player的baseData
                if (player.baseData == null)
                {
                    Debug.LogError("Player预制体的baseData未赋值！");
                    Destroy(playerObject);
                    return;
                }
        
                if (!(player.baseData is PlayerData))
                {
                    Debug.LogError($"Player预制体的baseData类型错误！期望PlayerData，实际为{player.baseData.GetType()}");
                    Destroy(playerObject);
                    return;
                }

                playerObject.name = "Player";
                player.playerData = runtimePlayerData; // 使用运行时数据
                SetPlayerActive(false);
        
                Debug.Log($"创建初始玩家成功，playerData: {player.playerData?.playerName ?? "未设置名称"}");
            }
            catch (Exception e)
            {
                Debug.LogError($"创建玩家时发生错误: {e.Message}\nStackTrace: {e.StackTrace}");
            }
        }

        public void RemovePlayer()
        {
            // 上传玩家数据
            if (player != null)
            {
                runtimePlayerData = player.playerData;
                Debug.Log($"上传玩家数据: {runtimePlayerData.playerName}");
                // 清理玩家对象
                Destroy(player.gameObject);
            }
        }

        #endregion

        #region 事件管理

        private void SubscribeToEvents()
        {
            GameManager.Instance.OnDialogueManagerReady += OnDialogueManagerReady;

        }

        public void OnDialogueManagerReady()
        {
            try
            {
                if (DialogueManager.Instance != null)
                {
                    DialogueManager.Instance.OnDialogueEnd += OnDialogueEnd;
                    Debug.Log("成功订阅对话结束事件");
                }
                else
                {
                    Debug.LogWarning("DialogueManager.Instance 为空，无法订阅对话事件");
                    // 可以设置一个重试机制
                    StartCoroutine(RetryEventSubscription());
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"订阅事件时发生错误: {e.Message}");
            }
        }

        private void UnsubscribeFromEvents()
        {
            try
            {
                if (DialogueManager.Instance != null)
                {
                    DialogueManager.Instance.OnDialogueEnd -= OnDialogueEnd;
                    Debug.Log("成功取消订阅对话结束事件");
                }
                GameManager.Instance.OnDialogueManagerReady -= OnDialogueManagerReady;
            }
            catch (Exception e)
            {
                Debug.LogError($"取消订阅事件时发生错误: {e.Message}");
            }
        }

        private IEnumerator RetryEventSubscription()
        {
            int retryCount = 0;
            const int maxRetries = 10;

            while (retryCount < maxRetries)
            {
                yield return new WaitForSeconds(0.5f);
                
                if (DialogueManager.Instance != null)
                {
                    DialogueManager.Instance.OnDialogueEnd += OnDialogueEnd;
                    Debug.Log("延迟订阅对话结束事件成功");
                    yield break;
                }
                
                retryCount++;
            }
            
            Debug.LogWarning("无法订阅对话事件：DialogueManager.Instance 始终为空");
        }

        #endregion

        #region 玩家管理

        public void SetPlayerActive(bool active)
        {
            if (player == null)
            {
                Debug.LogError("玩家对象为空，无法设置激活状态");
                return;
            }

            if (active)
            {
                player.playerData = runtimePlayerData; // 确保使用运行时数据
                player.gameObject.SetActive(true);
            }
            else
            {
                runtimePlayerData = player.playerData; // 保存当前数据
                player.gameObject.SetActive(false);
            }
        }

        public bool SetPlayerPosition(GameObject playerPoint)
        {
            try
            {
                if (player == null)
                {
                    Debug.LogError("玩家对象为空，无法设置位置");
                    return false;
                }

                if (playerPoint == null)
                {
                    Debug.LogError("玩家出生点为空");
                    return false;
                }

                player.playerData.playerPosition = playerPoint.transform.position;
                player.transform.position = player.playerData.playerPosition;

                Debug.Log($"玩家位置设置为: {player.playerData.playerPosition}");

                isPlayerInitialized = true;
                return true;
            }
            catch (Exception e)
            {
                isPlayerInitialized = false;
                Debug.LogError($"设置玩家位置时发生错误: {e.Message}");
                return false;
            }
        }

        private void TriggerNameChangeDialogue()
        {
            try
            {
                if (DialogueManager.Instance != null)
                {
                    DialogueManager.Instance.StartDialogueByID("dialogue_001");
                }
                else
                {
                    Debug.LogWarning("DialogueManager.Instance 为空，无法触发对话");
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"触发对话时发生错误: {e.Message}");
            }
        }

        #endregion

        #region 相机管理

        public void UpdatePlayerCamera(Camera targetCamera)
        {
            if (player == null)
            {
                Debug.LogError("玩家对象为空，无法更新相机");
                return;
            }

            Camera cameraToUse = targetCamera ?? Camera.main;
            
            if (cameraToUse == null)
            {
                Debug.LogError("没有可用的相机");
                return;
            }

            // 更新PlayerInput的相机引用
            if (UpdatePlayerInputCamera(cameraToUse))
            {
                currentCamera = cameraToUse;
                Debug.Log($"成功更新玩家相机: {cameraToUse.name}");
            }

            // 设置相机跟随目标
            SetCameraFollowTarget();
        }

        private bool UpdatePlayerInputCamera(Camera camera)
        {
            try
            {
                var playerInput = player.GetComponent<PlayerInput>();
                if (playerInput != null)
                {
                    playerInput.camera = camera;
                    return true;
                }
                else
                {
                    Debug.LogWarning("玩家上未找到 PlayerInput 组件");
                    return false;
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"更新PlayerInput相机时发生错误: {e.Message}");
                return false;
            }
        }

        private void SetCameraFollowTarget()
        {
            try
            {
                if (CameraManager.Instance != null)
                {
                    CameraManager.Instance.SetFollowTarget(player.transform);
                    Debug.Log("设置相机跟随目标成功");
                }
                else
                {
                    Debug.LogWarning("CameraManager.Instance 为空");
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"设置相机跟随目标时发生错误: {e.Message}");
            }
        }

        #endregion

        #region 事件回调

        private void OnDialogueEnd(string dialogueID)
        {
            switch (dialogueID)
            {
                case "game_start":
                    HandleGameStartDialogue();
                    break;
                    
                default:
                    Debug.Log($"未处理的对话ID: {dialogueID}");
                    break;
            }
        }

        private void HandleGameStartDialogue()
        {
            try
            {
                if (CameraManager.Instance != null)
                {
                    CameraManager.Instance.SetCameraActive(true);
                    Debug.Log("游戏开始，激活相机");
                }
                else
                {
                    Debug.LogWarning("CameraManager.Instance 为空，无法激活相机");
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"处理游戏开始对话时发生错误: {e.Message}");
            }
        }

        #endregion

        #region 调试方法

        [ContextMenu("重新创建玩家")]
        public void RecreatePlayer()
        {
            if (player != null)
            {
                DestroyImmediate(player.gameObject);
                player = null;
            }
            
            isPlayerInitialized = false;
            CreatePlayer();
        }

        [ContextMenu("重置玩家数据")]
        public void DebugResetPlayerData()
        {
            ResetPlayerData();
        }

        [ContextMenu("显示玩家数据信息")]
        public void DebugShowPlayerDataInfo()
        {
            if (runtimePlayerData != null)
            {
                Debug.Log($"玩家名称: {runtimePlayerData.playerName}");
                Debug.Log($"当前生命值: {runtimePlayerData.CurrentHealth}/{runtimePlayerData.MaxHealth}");
                Debug.Log($"当前法力值: {runtimePlayerData.CurrentMana}/{runtimePlayerData.MaxMana}");
                Debug.Log($"移动速度: {runtimePlayerData.moveSpeed}");
            }
            else
            {
                Debug.LogWarning("运行时玩家数据为空");
            }
        }

        #endregion

        #region 公共方法

        public PlayerGameData GetPlayerGameData()
        {
            return runtimePlayerData;
        }

        public bool SetPlayerGameData(PlayerGameData playerGameData)
        {
            return LoadPlayerData(playerGameData);
        }
        
        public void ChangePlayerName(string newName)
        {
            if (string.IsNullOrWhiteSpace(newName))
            {
                Debug.LogError("新名称为空或无效");
                return;
            }

            // 添加更详细的检查
            if (player == null)
            {
                Debug.LogError("玩家对象为空，请先创建玩家");
                return;
            }

            if (player.playerData == null)
            {
                Debug.LogError("玩家数据组件未初始化，请检查Player预制体上的PlayerData组件");
                return;
            }

            string oldName = player.playerData.playerName;
            player.playerData.playerName = newName;
    
            Debug.Log($"玩家名称从 '{oldName}' 更改为 '{newName}'");
    
            TriggerNameChangeDialogue();
        }

        #endregion
    }

    public enum PlayerPointType
    {
        None,
        Left,
        Right,
        Middle1,
        Middle2,
        Middle3,
    }
}