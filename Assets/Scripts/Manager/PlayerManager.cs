using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace Manager
{
    public class PlayerManager:MonoBehaviour
    {
        public static PlayerManager Instance { get; private set; }
        [SerializeField] private GameObject playerPrefab;
        [HideInInspector] public Player player;
        
        // [SerializeField]
        // private List<GameObject> createPlayerPoints = new List<GameObject>();
        private void Awake()
        {
            if (!Instance)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Debug.LogWarning("Multiple PlayerManager instances found. Destroying duplicate.");
                Destroy(gameObject);
            }
            player = Instantiate(playerPrefab,transform).GetComponent<Player>();
            player.gameObject.name = "Player";
        }

        private void Start()
        {
            player.gameObject.SetActive(false);
            
            // SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnEnable()
        {
            DialogueManager.Instance.OnDialogueEnd += CheckDialogueID;
        }

        private void OnDisable()
        {
            DialogueManager.Instance.OnDialogueEnd -= CheckDialogueID;
            // SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        // public void SetPlayerInuptCamera()
        // {
        //     if (player == null) return;
        //
        //     // 获取 PlayerInput 组件
        //     var playerInput = player.GetComponent<PlayerInput>();
        //     if (playerInput != null)
        //     {
        //         // 设置相机引用为主相机
        //         Camera mainCamera = Camera.main;
        //         if (mainCamera != null)
        //         {
        //             // 如果是使用 PlayerInput 的 Actions 模式
        //             playerInput.camera = mainCamera;
        //     
        //             // 如果是使用其他自定义输入组件，可能需要不同的设置方式
        //             // player.GetComponent<YourCustomInputComponent>().camera = mainCamera;
        //         }
        //         else
        //         {
        //             Debug.LogWarning("主相机未找到，可能会影响玩家输入");
        //         }
        //     }
        // }

        private void CheckDialogueID(string dialogueID)
        {
            if (dialogueID == "game_start")
            {
                CameraManager.Instance.SetCameraActive(true);
            }
        }

        public void SetPlayerPosition(GameObject playerPoint = null)
        {
            // player = Instantiate(playerPrefab).GetComponent<Player>();
            // player.gameObject.name = "Player";
            // player.gameObject.SetActive(false);
            
            if (playerPoint != null)
            {
                player.gameObject.transform.position = playerPoint.transform.position;
            }
        }

        public void ChangePlayerName(string newName)
        {
            if (string.IsNullOrEmpty(newName))
            {
                Debug.LogError("New name is null or empty.");
                return;
            }
            
            player.playerData.playerName = newName;
            Debug.Log($"Player name changed to: {newName}");
            
            DialogueManager.Instance.StartDialogueByID("dialogue_001");
        }
        
        // 使用指定相机更新 PlayerInput
        public void UpdatePlayerCamera(Camera targetCamera = null)
        {
            if (player == null) return;
    
            var playerInput = player.GetComponent<PlayerInput>();
            if (playerInput != null)
            {
                // 如果没有指定相机，则使用主相机
                Camera cameraToUse = targetCamera != null ? targetCamera : Camera.main;
        
                if (cameraToUse != null)
                {
                    playerInput.camera = cameraToUse;
                    Debug.Log("已更新 PlayerInput 的相机引用: " + cameraToUse.name);
                }
                else
                {
                    Debug.LogWarning("没有可用的相机，玩家输入可能无法正常工作");
                }
            }
            
            CameraManager.Instance.SetFollowTarget(player.transform);
        }
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