using System;
using System.Collections;
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
        // 运行时玩家数据副本
        private PlayerData runtimePlayerData;
        
        // 玩家状态
        private bool isPlayerInitialized = false;
        private Camera currentCamera;

        #region Unity生命周期
        
        private void Awake()
        {
            InitializeSingleton();
            LoadOriginalPlayerData();
            
            if (autoCreatePlayer)
            {
                CreatePlayer();
            }
        }

        private void Start()
        {
            // 延迟订阅事件，确保其他Manager已初始化
            StartCoroutine(DelayedEventSubscription());
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
        /// 加载原始玩家数据（只读）
        /// </summary>
        private void LoadOriginalPlayerData()
        {
            try
            {
                // 假设玩家数据存储在Resources文件夹中
                originalPlayerData = Resources.Load<PlayerData>("ScriptableObjects/Player/DefaultPlayerData");
                
                if (originalPlayerData != null)
                {
                    // 使用增强后的工具类创建运行时副本
                    runtimePlayerData = Utils.ScriptableObjectUtils.CreatePlayerDataCopy(originalPlayerData);
                    Debug.Log("成功加载并创建玩家数据运行时副本");
                }
                else
                {
                    Debug.LogWarning("未找到原始玩家数据，将使用默认设置");
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"加载玩家数据时发生错误: {e.Message}");
            }
        }

        /// <summary>
        /// 重置玩家数据到原始状态
        /// </summary>
        public void ResetPlayerData()
        {
            if (originalPlayerData != null && runtimePlayerData != null)
            {
                Utils.ScriptableObjectUtils.ResetToOriginal(originalPlayerData, runtimePlayerData);
                
                // 如果玩家对象存在，重新应用数据
                if (player != null)
                {
                    player.baseData = runtimePlayerData;
                }
                
                Debug.Log("已重置玩家数据到原始状态");
            }
        }

        /// <summary>
        /// 清理运行时数据
        /// </summary>
        private void CleanupRuntimeData()
        {
            if (runtimePlayerData != null)
            {
                Utils.ScriptableObjectUtils.SafeDestroyRuntimeCopy(runtimePlayerData);
                runtimePlayerData = null;
            }
        }

        private void CreatePlayer()
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

                // 使用运行时数据副本
                if (runtimePlayerData != null)
                {
                    player.baseData = runtimePlayerData;
                }

                playerObject.name = "Player";
                
                // 初始状态设为非激活，等待场景设置完成后激活
                SetPlayerActive(false);
                
                Debug.Log("玩家创建成功");
            }
            catch (Exception e)
            {
                Debug.LogError($"创建玩家时发生错误: {e.Message}");
            }
        }

        private IEnumerator DelayedEventSubscription()
        {
            // 等待一帧确保所有Manager都已初始化
            yield return null;
            
            SubscribeToEvents();
        }

        #endregion

        #region 事件管理

        private void SubscribeToEvents()
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

            player.gameObject.SetActive(active);
            Debug.Log($"玩家激活状态设置为: {active}");
        }

        public void SetPlayerPosition(GameObject playerPoint)
        {
            if (player == null)
            {
                Debug.LogError("玩家对象为空，无法设置位置");
                return;
            }

            if (playerPoint == null)
            {
                Debug.LogError("玩家出生点为空");
                return;
            }

            Vector3 targetPosition = playerPoint.transform.position;
            player.transform.position = targetPosition;
            
            Debug.Log($"玩家位置设置为: {targetPosition}");
            
            // 确保玩家在设置位置后是激活的
            if (!player.gameObject.activeInHierarchy)
            {
                if (SceneManager.GetActiveScene().name != "女生宿舍")
                    SetPlayerActive(true);
                
                if (!GameStateManager.Instance.GetFlag("FirstEntry_" + SceneManager.GetActiveScene().name))
                    SetPlayerActive(true);
            }
            
            isPlayerInitialized = true;
        }

        public void ChangePlayerName(string newName)
        {
            if (string.IsNullOrWhiteSpace(newName))
            {
                Debug.LogError("新名称为空或无效");
                return;
            }

            // 修改运行时数据副本，不会污染原始资源
            if (runtimePlayerData != null)
            {
                string oldName = runtimePlayerData.playerName;
                runtimePlayerData.playerName = newName;
                Debug.Log($"玩家名称从 '{oldName}' 更改为 '{newName}'");
                
                // 触发相关对话
                TriggerNameChangeDialogue();
            }
            else
            {
                Debug.LogError("运行时玩家数据为空，无法更改名称");
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

        #region 公共查询方法

        public bool IsPlayerInitialized()
        {
            return isPlayerInitialized && player != null;
        }

        public Vector3 GetPlayerPosition()
        {
            return player?.transform.position ?? Vector3.zero;
        }

        public Camera GetCurrentCamera()
        {
            return currentCamera;
        }

        /// <summary>
        /// 获取运行时玩家数据（用于游戏逻辑）
        /// </summary>
        /// <returns>运行时玩家数据副本</returns>
        public PlayerData GetRuntimePlayerData()
        {
            return runtimePlayerData;
        }

        /// <summary>
        /// 获取原始玩家数据（只读）
        /// </summary>
        /// <returns>原始玩家数据</returns>
        public PlayerData GetOriginalPlayerData()
        {
            return originalPlayerData;
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