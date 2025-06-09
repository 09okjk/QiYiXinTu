using System;
using System.Collections.Generic;
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
        [SerializeField] private List<GameObject> nextLevelPoints; // 下一关卡传送点列表
        
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
            // 初始化关卡
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

            PlayerManager.Instance.UpdatePlayerCamera(PlayerCamera);
            DialogueManager.Instance.OnDialogueEnd += OnDialogueEnd;
        }

        private void OnDestroy()
        {
            DialogueManager.Instance.OnDialogueEnd -= OnDialogueEnd;
        }

        private void InitLevel()
        {
            // 播放场景加载动画
            // if (animationNames.Count > 0)
            // {
            //     sceneAnimator.gameObject.SetActive(true);
            //     sceneAnimator.Play(animationNames[0]);
            // }
            
            // 设置玩家出生点
            if (playerPoint != null)
            {
                PlayerManager.Instance.SetPlayerPosition(playerPoint);
                // PlayerManager.Instance.SetPlayerInuptCamera();
            }
            else
            {
                Debug.LogWarning("Player point is not set in LevelManager.");
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
                foreach (var nextLevelPoint in nextLevelPoints)
                {
                    nextLevelPoint.GetComponent<Collider2D>().isTrigger = true; // 启用传送点碰撞体
                }
            }
        }
    }
}