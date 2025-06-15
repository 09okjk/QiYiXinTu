using System;
using System.Collections;
using System.Collections.Generic;
using News;
using Save;
using UI;
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
        
        [Header("玩家出生点")]
        [SerializeField] private GameObject defaultPlayerPoint; // 默认玩家出生点
        [SerializeField] private GameObject leftPlayerPoint;    // 左侧出生点
        [SerializeField] private GameObject rightPlayerPoint;   // 右侧出生点
        [SerializeField] private GameObject middle1PlayerPoint; // 中间1出生点
        [SerializeField] private GameObject middle2PlayerPoint; // 中间2出生点
        [SerializeField] private GameObject middle3PlayerPoint; // 中间3出生点
        
        [SerializeField] private List<GameObject> npcsPoints; // NPC出生点列表
        [SerializeField] private List<GameObject> enemyPoints; // 敌人出生点列表
        [SerializeField] private List<NewsButton> newsObjects; // 新闻按钮列表
        [SerializeField] private GameObject startAinimation; // 开场动画对象
        
        [Header("场景动画")]
        [SerializeField] private Animator sceneAnimator; // 场景动画控制器
        [SerializeField] private List<string> animationNames; // 场景动画名称列表
        
        [Header("初始化设置")]
        [SerializeField] private float initializationDelay = 0.1f; // 初始化延迟时间
        [SerializeField] private bool showLoadingScreen = true; // 是否显示加载屏幕
        
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
            
            // 自动查找玩家出生点（如果没有手动设置）
            AutoFindPlayerPoints();
        }
        
        /// <summary>
        /// 自动查找场景中的玩家出生点
        /// </summary>
        private void AutoFindPlayerPoints()
        {
            // 如果没有手动设置，尝试通过标签或名称自动查找
            if (defaultPlayerPoint == null)
                defaultPlayerPoint = GameObject.FindGameObjectWithTag("PlayerPoint");
            
            if (leftPlayerPoint == null)
                leftPlayerPoint = GameObject.Find("LeftPlayerPoint");
                
            if (rightPlayerPoint == null)
                rightPlayerPoint = GameObject.Find("RightPlayerPoint");
                
            if (middle1PlayerPoint == null)
                middle1PlayerPoint = GameObject.Find("Middle1PlayerPoint");
                
            if (middle2PlayerPoint == null)
                middle2PlayerPoint = GameObject.Find("Middle2PlayerPoint");
                
            if (middle3PlayerPoint == null)
                middle3PlayerPoint = GameObject.Find("Middle3PlayerPoint");
                
            Debug.Log($"找到玩家出生点: Default={defaultPlayerPoint != null}, Left={leftPlayerPoint != null}, Right={rightPlayerPoint != null}");
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
            if (isDataLoaded  && isSceneLoaded && !isLevelInitialized)
            {
                // 添加小延迟确保所有组件都已准备就绪
                StartCoroutine(DelayedInitLevel());
            }
            else
            {
                // 如果未使用保存加载场景，则直接延迟初始化
                if (!GameStateManager.Instance.GetFlag("UseSaveLoadingScene"))
                {
                    // 添加小延迟确保所有组件都已准备就绪
                    StartCoroutine(DelayedInitLevel());
                    Debug.Log($"关卡 {levelName} 已经准备好进行初始化: 数据加载={isDataLoaded}, 场景加载={isSceneLoaded}, 已初始化={isLevelInitialized}");
                }
                else
                {
                    Debug.Log($"关卡 {levelName} 尚未准备好进行初始化: 数据加载={isDataLoaded}, 场景加载={isSceneLoaded}, 已初始化={isLevelInitialized}");
                }
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
        /// 关卡初始化序列（带加载屏幕）
        /// </summary>
        private IEnumerator InitLevelSequence()
        {
            // 显示加载屏幕
            if (showLoadingScreen && GameManager.Instance != null)
            {
                GameManager.Instance.ShowLoadingScreen($"正在初始化场景: {levelName}");
            }
            
            float totalSteps = 5f;
            float currentStep = 0f;
            
            // 步骤1: 设置玩家位置
            if (showLoadingScreen && GameManager.Instance != null)
            {
                GameManager.Instance.UpdateLoadingProgress(currentStep / totalSteps, "设置玩家位置...");
            }
            yield return StartCoroutine(SetPlayerPosition());
            currentStep++;
            
            // 步骤2: 设置相机
            if (showLoadingScreen && GameManager.Instance != null)
            {
                GameManager.Instance.UpdateLoadingProgress(currentStep / totalSteps, "设置相机...");
            }
            yield return StartCoroutine(SetupCamera());
            currentStep++;
            
            // 步骤3: 生成NPC
            if (showLoadingScreen && GameManager.Instance != null)
            {
                GameManager.Instance.UpdateLoadingProgress(currentStep / totalSteps, "生成NPC...");
            }
            yield return StartCoroutine(SpawnNPCs());
            currentStep++;
            
            // 步骤4: 生成敌人（如果需要）
            if (showLoadingScreen && GameManager.Instance != null)
            {
                GameManager.Instance.UpdateLoadingProgress(currentStep / totalSteps, "生成敌人...");
            }
            yield return StartCoroutine(SpawnEnemies());
            currentStep++;
            
            // 步骤5: 生成新闻物体
            if (showLoadingScreen && GameManager.Instance != null)
            {
                GameManager.Instance.UpdateLoadingProgress(currentStep / totalSteps, "生成新闻物体...");
            }
            yield return StartCoroutine(SpawnNewsObjects());
            currentStep++;
            
            // 完成初始化
            if (showLoadingScreen && GameManager.Instance != null)
            {
                GameManager.Instance.UpdateLoadingProgress(1f, "初始化完成！");
                yield return new WaitForSeconds(0.5f); // 显示完成信息
                GameManager.Instance.HideLoadingScreen();
            }
            
            // 标记初始化完成
            isLevelInitialized = true;
            Debug.Log($"关卡 {levelName} 初始化完成");
            
            // 触发初始化完成事件
            OnLevelInitialized();
        }

        /// <summary>
        /// 根据PlayerPointType设置玩家位置
        /// </summary>
        private IEnumerator SetPlayerPosition()
        {
            // 获取应该使用的玩家出生点
            GameObject targetPlayerPoint = GetPlayerSpawnPoint();
            
            if (targetPlayerPoint == null)
            {
                Debug.LogError($"未找到合适的玩家出生点在关卡 {levelName} 中!");
                yield break;
            }

            if (PlayerManager.Instance == null)
            {
                Debug.LogError("PlayerManager.Instance 为空!");
                yield break;
            }

            Debug.Log($"设置玩家位置: {targetPlayerPoint.transform.position} (类型: {GameStateManager.Instance.GetPlayerPointType()})");
            PlayerManager.Instance.SetPlayerPosition(targetPlayerPoint);
            
            // 等待一帧确保位置设置生效
            yield return null;
            
            // 验证玩家位置是否设置成功
            if (PlayerManager.Instance.player != null)
            {
                Debug.Log($"玩家位置设置成功: {PlayerManager.Instance.player.transform.position}");
            }
        }
        
        /// <summary>
        /// 根据PlayerPointType获取对应的玩家出生点
        /// </summary>
        private GameObject GetPlayerSpawnPoint()
        {
            if (GameStateManager.Instance == null)
            {
                Debug.LogWarning("GameStateManager.Instance 为空，使用默认出生点");
                return defaultPlayerPoint;
            }

            PlayerPointType pointType = GameStateManager.Instance.GetPlayerPointType();
            
            GameObject selectedPoint = pointType switch
            {
                PlayerPointType.Left => leftPlayerPoint,
                PlayerPointType.Right => rightPlayerPoint,
                PlayerPointType.Middle1 => middle1PlayerPoint,
                PlayerPointType.Middle2 => middle2PlayerPoint,
                PlayerPointType.Middle3 => middle3PlayerPoint,
                PlayerPointType.None => defaultPlayerPoint,
                _ => defaultPlayerPoint
            };

            // 如果指定的出生点不存在，回退到默认点
            if (selectedPoint == null)
            {
                Debug.LogWarning($"指定的出生点类型 {pointType} 不存在，使用默认出生点");
                selectedPoint = defaultPlayerPoint;
            }

            Debug.Log($"选择玩家出生点: {pointType} -> {selectedPoint?.name}");
            return selectedPoint;
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
            
            for (int i = 0; i < npcsPoints.Count; i++)
            {
                var npcPoint = npcsPoints[i];
                if (npcPoint != null)
                {
                    string npcId = npcPoint.name;
                    Debug.Log($"生成NPC: {npcId} 在位置: {npcPoint.transform.position}");
                    
                    NPCManager.Instance.ShowNPC(npcId, npcPoint);
                    
                    // 更新进度（如果正在显示加载屏幕）
                    if (showLoadingScreen && GameManager.Instance != null && GameManager.Instance.IsLoadingScreenActive())
                    {
                        float npcProgress = (float)(i + 1) / npcsPoints.Count;
                        GameManager.Instance.UpdateLoadingProgress(
                            2f/5f + (npcProgress * 0.2f), // 在第3步骤内部更新进度
                            $"生成NPC... ({i + 1}/{npcsPoints.Count})"
                        );
                    }
                    
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
            
            for (int i = 0; i < enemyPoints.Count; i++)
            {
                var enemyPoint = enemyPoints[i];
                if (enemyPoint != null)
                {
                    Debug.Log($"生成敌人在位置: {enemyPoint.transform.position}");
                    // TODO: 实现敌人生成逻辑
                    // EnemyManager.Instance.SpawnEnemy(enemyPoint);
                    
                    // 更新进度
                    if (showLoadingScreen && GameManager.Instance != null && GameManager.Instance.IsLoadingScreenActive())
                    {
                        float enemyProgress = (float)(i + 1) / enemyPoints.Count;
                        GameManager.Instance.UpdateLoadingProgress(
                            3f/5f + (enemyProgress * 0.2f), // 在第4步骤内部更新进度
                            $"生成敌人... ({i + 1}/{enemyPoints.Count})"
                        );
                    }
                    
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

            for (int i = 0; i < newsObjects.Count; i++)
            {
                var newsObject = newsObjects[i];
                var news = NewsManager.Instance.GetNewsByID(newsObject.newsID);
                if (news == null)
                {
                    Debug.LogWarning($"未找到新闻ID: {newsObject.newsID}");
                    continue;
                }
                newsObject.SetNewsData(news);
                
                // 更新进度
                if (showLoadingScreen && GameManager.Instance != null && GameManager.Instance.IsLoadingScreenActive())
                {
                    float newsProgress = (float)(i + 1) / newsObjects.Count;
                    GameManager.Instance.UpdateLoadingProgress(
                        4f/5f + (newsProgress * 0.2f), // 在第5步骤内部更新进度
                        $"设置新闻物体... ({i + 1}/{newsObjects.Count})"
                    );
                }
                
                yield return null; // 每帧处理一个新闻物体
            }
            
            Debug.Log("新闻物体生成完成");
        }

        /// <summary>
        /// 关卡初始化完成回调
        /// </summary>
        private async void OnLevelInitialized()
        {
            // 可以在这里添加初始化完成后的逻辑
            Debug.Log($"关卡 {levelName} 完全初始化完成");
            
            // 将UseSaveLoadingScene标志设置为默认的false
            GameStateManager.Instance.SetFlag("UseSaveLoadingScene",false);
            
            // 如果是第一次进入，设置对应的标志
            if (GameStateManager.Instance != null)
            {
                if (startAinimation != null)
                {
                    startAinimation.gameObject.SetActive(GameStateManager.Instance.GetFlag("FirstEntry_" + levelName));
                }
                GameStateManager.Instance.SetFlag("FirstEntry_" + levelName, false);
            }
            
            var isSave =await AsyncSaveLoadSystem.SaveGameAsync(0);

            if (isSave)
            {
                if (levelName == "女生宿舍")
                {
                    StartAnimationCotroller.Instance.PlayVideo(0);
                }
                if (levelName == "outside1")
                {
                    DialogueManager.Instance.StartDialogueByID("lide_dialogue");
                }

                if (levelName == "In_LiDe")
                {
                    DialogueManager.Instance.StartDialogueByID("lide_inside1_instruction_dialogue");
                }

                if (levelName == "Space_Time")
                {
                    DialogueManager.Instance.StartDialogueByID("rift_1955_dialogue");
                }
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

            if (dialogueID == "fang_dialogue")
            {
                GameStateManager.Instance.SetFlag("CanEnter_"+"In_LiDe", true);
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

        #region 调试和辅助方法

        /// <summary>
        /// 手动设置玩家出生点类型（调试用）
        /// </summary>
        [ContextMenu("设置玩家出生点为Left")]
        public void SetPlayerPointTypeToLeft()
        {
            if (GameStateManager.Instance != null)
            {
                GameStateManager.Instance.SetPlayerPointType(PlayerPointType.Left);
                Debug.Log("设置玩家出生点类型为Left");
            }
        }

        /// <summary>
        /// 显示当前玩家出生点类型（调试用）
        /// </summary>
        [ContextMenu("显示当前玩家出生点类型")]
        public void ShowCurrentPlayerPointType()
        {
            if (GameStateManager.Instance != null)
            {
                Debug.Log($"当前玩家出生点类型: {GameStateManager.Instance.GetPlayerPointType()}");
            }
        }

        /// <summary>
        /// 强制显示加载屏幕（调试用）
        /// </summary>
        [ContextMenu("测试加载屏幕")]
        public void TestLoadingScreen()
        {
            if (GameManager.Instance != null)
            {
                StartCoroutine(TestLoadingScreenCoroutine());
            }
        }

        private IEnumerator TestLoadingScreenCoroutine()
        {
            GameManager.Instance.ShowLoadingScreen("测试加载屏幕");
            
            for (float i = 0; i <= 1f; i += 0.1f)
            {
                GameManager.Instance.UpdateLoadingProgress(i, $"测试进度: {i:P0}");
                yield return new WaitForSeconds(0.2f);
            }
            
            GameManager.Instance.HideLoadingScreen();
        }

        #endregion
    }
}