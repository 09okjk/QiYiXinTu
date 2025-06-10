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
        [SerializeField] private bool autoCreatePlayer = true;
        
        [HideInInspector] public Player player;
        
        // 玩家状态
        private bool isPlayerInitialized = false;
        private Camera currentCamera;

        #region Unity生命周期
        
        private void Awake()
        {
            InitializeSingleton();
            
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

            if (player?.playerData == null)
            {
                Debug.LogError("玩家数据为空，无法更改名称");
                return;
            }

            string oldName = player.playerData.playerName;
            player.playerData.playerName = newName;
            
            Debug.Log($"玩家名称从 '{oldName}' 更改为 '{newName}'");
            
            // 触发相关对话
            TriggerNameChangeDialogue();
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