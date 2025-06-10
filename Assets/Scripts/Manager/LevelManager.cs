using System;
using System.Collections.Generic;
using Save;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

namespace Manager
{
    public class LevelManager:MonoBehaviour
    {
        public static LevelManager Instance { get; private set; }
        [Header("场景物体")]
        public Camera PlayerCamera;
        [SerializeField] private GameObject playerPoint; // 玩家出生点
        [SerializeField] private List<GameObject> npcsPoints; // NPC出生点列表
        [SerializeField] private List<GameObject> enemyPoints; // 敌人出生点列表
        // [SerializeField] private List<GameObject> nextLevelPoints; // 下一关卡传送点列表
        
        [Header("场景动画")]
        [SerializeField] private Animator sceneAnimator; // 场景动画控制器
        [SerializeField] private List<string> animationNames; // 场景动画名称列表
        

        private string levelName;
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }

            levelName = SceneManager.GetActiveScene().name;
            npcsPoints = new List<GameObject>(GameObject.FindGameObjectsWithTag("NPCPoint"));
        }
        
        private void Start()
        {
            sceneAnimator.gameObject.SetActive(false);
            AsyncSaveLoadSystem.OnLoadComplete += OnDataLoaded;
            DialogueManager.Instance.OnDialogueEnd += OnDialogueEnd;
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnDestroy()
        {
            AsyncSaveLoadSystem.OnLoadComplete -= OnDataLoaded;
            DialogueManager.Instance.OnDialogueEnd -= OnDialogueEnd;
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private void OnSceneLoaded(Scene arg0, LoadSceneMode arg1)
        {
            Debug.Log($"场景加载: {arg0.name}");
    
            // 确保玩家位置先设置好
            if (arg0.name != "女生宿舍")
            {
                InitLevel();
            }
    
            // 玩家位置设置完毕后再设置相机
            if (PlayerCamera != null)
            {
                Debug.Log("场景加载后更新相机");
                PlayerManager.Instance.UpdatePlayerCamera(PlayerCamera);
                CameraManager.Instance.SetCameraActive(true);
            }
        }

        private void OnDataLoaded(string obj)
        {
            if (GameStateManager.Instance.GetFlag("FirstEntry_" + levelName))
            {
                // 如果是第一次进入该关卡，执行初始化逻辑
                Debug.Log($"Initializing level: {levelName}");
                InitLevel();
            }
            else
            {
                // TODO:从保存的数据中加载关卡状态
            }
        }

        private void InitLevel()
        {
            Debug.Log($"初始化关卡: {levelName}");
    
            // 设置玩家出生点
            if (playerPoint != null)
            {
                Debug.Log($"设置玩家位置: {playerPoint.transform.position}");
                PlayerManager.Instance.SetPlayerPosition(playerPoint);
                // 确认玩家位置是否设置成功
                Debug.Log($"设置后的玩家位置: {PlayerManager.Instance.player.transform.position}");
            }
            else
            {
                Debug.LogError("Player point is null in LevelManager!");
            }
    
            // 先确保玩家已经正确放置，再设置相机
            if (PlayerCamera != null)
            {
                Debug.Log("更新玩家相机引用");
                PlayerManager.Instance.UpdatePlayerCamera(PlayerCamera);
                Debug.Log("激活相机");
                CameraManager.Instance.SetCameraActive(true);
            }
            else
            {
                Debug.LogError("PlayerCamera is null in LevelManager!");
            }
            
            // 设置NPC出生点
            if (npcsPoints != null && npcsPoints.Count > 0)
            {
                foreach (var npcPoint in npcsPoints)
                {
                    if (npcPoint != null)
                    {
                        string npcId = npcPoint.name;
                        // 在每个NPC出生点生成NPC
                        NPCManager.Instance.ShowNpc(npcId, npcPoint);
                    }
                }
            }
            else
            {
                Debug.LogWarning("NPC points are not set in LevelManager.");
            }
        }
        
        private void OnDialogueEnd(string dialogueID)
        {
            if (dialogueID == "dialogue_001" && levelName == "女生宿舍")
            {
                GameStateManager.Instance.SetFlag("CanEnter_"+"outside1", true);
            }
        }
    }
}