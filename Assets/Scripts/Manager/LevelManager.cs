using System;
using System.Collections;
using System.Collections.Generic;
using News;
using Save;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

namespace Manager
{
    public class LevelManager : MonoBehaviour
    {
        public static LevelManager Instance { get; private set; }
        
        [Header("场景物体")]
        public Camera PlayerCamera;
        [SerializeField] private GameObject playerPoint; // 玩家出生点
        [SerializeField] private List<GameObject> npcsPoints; // NPC出生点列表
        [SerializeField] private List<GameObject> enemyPoints; // 敌人出生点列表
        [SerializeField] private List<NewsButton> newsObjects; // 新闻按钮列表
        [SerializeField] private GameObject startAinimation; // 开场动画对象
        // [SerializeField] private List<GameObject> nextLevelPoints; // 下一关卡传送点列表
        
        [Header("场景动画")]
        [SerializeField] private Animator sceneAnimator; // 场景动画控制器
        [SerializeField] private List<string> animationNames; // 场景动画名称列表
        
        [Header("初始化设置")]
        [SerializeField] private float initializationDelay = 0.1f; // 初始化延迟时间
        
        private string levelName;
        private bool isLevelInitialized = false;
        private bool isDataLoaded = false;
        private bool isSceneLoaded = false;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
                return;
            }

            levelName = SceneManager.GetActiveScene().name;
            
            // 在Awake中查找NPC点，确保在Start之前完成
            FindNPCPoints();
        }
        
        private void Start()
        {
            if (sceneAnimator != null)
            {
                sceneAnimator.gameObject.SetActive(false);
            }
            
            // 订阅事件
            AsyncSaveLoadSystem.OnLoadComplete += OnDataLoaded;
            if (DialogueManager.Instance != null)
            {
                DialogueManager.Instance.OnDialogueEnd += OnDialogueEnd;
            }
            SceneManager.sceneLoaded += OnSceneLoaded;
            
            // 如果当前场景已经加载，手动调用一次
            OnSceneLoaded(SceneManager.GetActiveScene(), LoadSceneMode.Single);

        }

        private void OnDestroy()
        {
            // 取消订阅事件
            AsyncSaveLoadSystem.OnLoadComplete -= OnDataLoaded;
            if (DialogueManager.Instance != null)
            {
                DialogueManager.Instance.OnDialogueEnd -= OnDialogueEnd;
            }
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        /// <summary>
        /// 查找场景中的NPC出生点
        /// </summary>
        private void FindNPCPoints()
        {
            npcsPoints = new List<GameObject>(GameObject.FindGameObjectsWithTag("NPCPoint"));
            Debug.Log($"找到 {npcsPoints.Count} 个NPC出生点");
        }

        /// <summary>
        /// 场景加载完成回调
        /// </summary>
        private void OnSceneLoaded(Scene scene, LoadSceneMode arg1)
        {
            Debug.Log($"场景加载完成: {scene.name}");
            isSceneLoaded = true;
            
            // 检查是否可以开始初始化
            TryInitializeLevel();
        }

        /// <summary>
        /// 数据加载完成回调
        /// </summary>
        private void OnDataLoaded(string obj)
        {
            Debug.Log($"数据加载完成: {obj}");
            isDataLoaded = true;
            
            // 检查是否可以开始初始化
            TryInitializeLevel();
        }

        /// <summary>
        /// 尝试初始化关卡（确保数据和场景都已加载）
        /// </summary>
        private void TryInitializeLevel()
        {
            // 只有当数据和场景都加载完成且未初始化时才进行初始化
            if (isDataLoaded && isSceneLoaded && !isLevelInitialized)
            {
                // 添加小延迟确保所有组件都已准备就绪
                StartCoroutine(DelayedInitLevel());
            }
        }

        /// <summary>
        /// 延迟初始化关卡
        /// </summary>
        private IEnumerator DelayedInitLevel()
        {
            yield return new WaitForSeconds(initializationDelay);
            
            if (ShouldInitializeLevel())
            {
                InitLevel();
            }
            else
            {
                Debug.Log($"跳过关卡初始化: {levelName}");
                // 对于特殊场景（如女生宿舍），仍需要设置基本的相机
                SetupCameraOnly();
            }
        }

        /// <summary>
        /// 判断是否应该初始化关卡
        /// </summary>
        private bool ShouldInitializeLevel()
        {
            // 女生宿舍等特殊场景可能需要特殊处理
            // if (levelName == "女生宿舍")
            // {
            //     return GameStateManager.Instance.GetFlag("FirstEntry_" + levelName);
            // }
            
            return true; // 其他场景默认都需要初始化
        }

        /// <summary>
        /// 初始化关卡的主方法
        /// </summary>
        private void InitLevel()
        {
            if (isLevelInitialized)
            {
                Debug.LogWarning($"关卡 {levelName} 已经初始化过了");
                return;
            }

            Debug.Log($"开始初始化关卡: {levelName}");
            
            // 按照正确的顺序进行初始化
            StartCoroutine(InitLevelSequence());
        }

        /// <summary>
        /// 关卡初始化序列
        /// </summary>
        private IEnumerator InitLevelSequence()
        {
            // 步骤1: 设置玩家位置
            yield return StartCoroutine(SetPlayerPosition());
            
            // 步骤2: 设置相机
            yield return StartCoroutine(SetupCamera());
            
            // 步骤3: 生成NPC
            yield return StartCoroutine(SpawnNPCs());
            
            // 步骤4: 生成敌人（如果需要）
            yield return StartCoroutine(SpawnEnemies());
            
            // 步骤5: 生成新闻物体
            yield return StartCoroutine(SpawnNewsObjects());
            
            // 标记初始化完成
            isLevelInitialized = true;
            Debug.Log($"关卡 {levelName} 初始化完成");
            
            // 触发初始化完成事件
            OnLevelInitialized();
        }

        /// <summary>
        /// 设置玩家位置
        /// </summary>
        private IEnumerator  SetPlayerPosition()
        {
            if (playerPoint == null)
            {
                Debug.LogError($"玩家出生点未设置在关卡 {levelName} 中!");
                yield break;
            }

            if (PlayerManager.Instance == null)
            {
                Debug.LogError("PlayerManager.Instance 为空!");
                yield break;
            }

            Debug.Log($"设置玩家位置: {playerPoint.transform.position}");
            PlayerManager.Instance.SetPlayerPosition(playerPoint);
            
            // 等待一帧确保位置设置生效
            yield return null;
            
            // 验证玩家位置是否设置成功
            if (PlayerManager.Instance.player != null)
            {
                Debug.Log($"玩家位置设置成功: {PlayerManager.Instance.player.transform.position}");
            }
        }

        /// <summary>
        /// 设置相机
        /// </summary>
        private IEnumerator SetupCamera()
        {
            if (PlayerCamera == null)
            {
                Debug.LogError($"PlayerCamera 未设置在关卡 {levelName} 中!");
                yield break;
            }

            if (PlayerManager.Instance == null || CameraManager.Instance == null)
            {
                Debug.LogError("PlayerManager 或 CameraManager 实例为空!");
                yield break;
            }

            Debug.Log("设置玩家相机");
            PlayerManager.Instance.UpdatePlayerCamera(PlayerCamera);
            
            // 等待一帧确保相机引用更新
            yield return null;
            
            Debug.Log("激活相机管理器");
            CameraManager.Instance.SetCameraActive(true);
            
            yield return null;
        }

        /// <summary>
        /// 仅设置相机（用于特殊场景）
        /// </summary>
        private void SetupCameraOnly()
        {
            if (PlayerCamera != null && PlayerManager.Instance != null && CameraManager.Instance != null)
            {
                Debug.Log("设置相机（特殊场景模式）");
                PlayerManager.Instance.UpdatePlayerCamera(PlayerCamera);
                CameraManager.Instance.SetCameraActive(true);
            }
        }

        /// <summary>
        /// 生成NPC
        /// </summary>
        private IEnumerator SpawnNPCs()
        {
            if (npcsPoints == null || npcsPoints.Count == 0)
            {
                Debug.Log($"关卡 {levelName} 中没有NPC出生点");
                yield break;
            }

            if (NPCManager.Instance == null)
            {
                Debug.LogError("NPCManager.Instance 为空!");
                yield break;
            }

            Debug.Log($"开始生成 {npcsPoints.Count} 个NPC");
            
            foreach (var npcPoint in npcsPoints)
            {
                if (npcPoint != null)
                {
                    string npcId = npcPoint.name;
                    Debug.Log($"生成NPC: {npcId} 在位置: {npcPoint.transform.position}");
                    
                    NPCManager.Instance.ShowNpc(npcId, npcPoint);
                    
                    // 在每个NPC生成之间添加小延迟，避免同时生成造成的问题
                    yield return new WaitForSeconds(0.05f);
                }
                else
                {
                    Debug.LogWarning("发现空的NPC出生点引用");
                }
            }
            
            Debug.Log("NPC生成完成");
        }

        /// <summary>
        /// 生成敌人
        /// </summary>
        private IEnumerator SpawnEnemies()
        {
            if (enemyPoints == null || enemyPoints.Count == 0)
            {
                Debug.Log($"关卡 {levelName} 中没有敌人出生点");
                yield break;
            }

            Debug.Log($"开始生成 {enemyPoints.Count} 个敌人");
            
            // TODO: 实现敌人生成逻辑
            foreach (var enemyPoint in enemyPoints)
            {
                if (enemyPoint != null)
                {
                    Debug.Log($"生成敌人在位置: {enemyPoint.transform.position}");
                    // EnemyManager.Instance.SpawnEnemy(enemyPoint);
                    yield return new WaitForSeconds(0.1f);
                }
            }
            
            Debug.Log("敌人生成完成");
        }

        /// <summary>
        /// 生成新闻物体
        /// </summary>
        private IEnumerator SpawnNewsObjects()
        {
            if (newsObjects == null || newsObjects.Count == 0)
            {
                Debug.Log($"关卡 {levelName} 中没有新闻物体");
                yield break;
            }
            
            Debug.Log($"开始生成 {newsObjects.Count} 个新闻物体");

            foreach (var newsObject in newsObjects)
            {
                var news = NewsManager.Instance.GetNewsByID(newsObject.newsID);
                if (news == null)
                {
                    Debug.LogWarning($"未找到新闻ID: {newsObject.newsID}");
                    continue;
                }
                newsObject.SetNewsData(news);
            }
        }

        /// <summary>
        /// 关卡初始化完成回调
        /// </summary>
        private void OnLevelInitialized()
        {
            // 可以在这里添加初始化完成后的逻辑
            Debug.Log($"关卡 {levelName} 完全初始化完成");
            
            // 如果是第一次进入，设置对应的标志
            if (GameStateManager.Instance != null)
            {
                startAinimation.gameObject.SetActive(GameStateManager.Instance.GetFlag("FirstEntry_" + levelName));
                GameStateManager.Instance.SetFlag("FirstEntry_" + levelName, false);
            }
        }

        /// <summary>
        /// 对话结束回调
        /// </summary>
        private void OnDialogueEnd(string dialogueID)
        {
            // 处理特定对话结束后的逻辑
            if (dialogueID == "dialogue_001" && levelName == "女生宿舍")
            {
                if (GameStateManager.Instance != null)
                {
                    GameStateManager.Instance.SetFlag("CanEnter_outside1", true);
                    Debug.Log("女生宿舍对话完成，设置outside1可进入标志");
                }
            }
        }

        /// <summary>
        /// 手动重新初始化关卡（调试用）
        /// </summary>
        [ContextMenu("重新初始化关卡")]
        public void ReInitializeLevel()
        {
            isLevelInitialized = false;
            InitLevel();
        }

        /// <summary>
        /// 获取关卡初始化状态
        /// </summary>
        public bool IsLevelInitialized()
        {
            return isLevelInitialized;
        }
    }
}