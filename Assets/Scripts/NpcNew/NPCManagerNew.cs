using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

namespace NpcNew
{
    /// <summary>
    /// 增强版NPC管理器
    /// </summary>
    public class NPCManagerNew : MonoBehaviour
    {
        public static NPCManagerNew Instance { get; private set; }

        [Header("配置")]
        [SerializeField] private GameObject npcPrefab;
        [SerializeField] private Transform npcContainer;
        [SerializeField] private bool enableDebugMode = false; // 是否启用调试模式

        // NPC数据和实例
        private Dictionary<string, NPCDataNew> npcDataDictionary = new Dictionary<string, NPCDataNew>();
        private Dictionary<string, NPCCore> activeNPCs = new Dictionary<string, NPCCore>();
        private Queue<NPCCore> npcPool = new Queue<NPCCore>();

        // 场景管理
        private string currentSceneName;
        private List<string> loadedScenes = new List<string>(); // 已加载的场景列表

        // 事件
        public event Action<string> OnNPCActivated;
        public event Action<string> OnNPCDeactivated;
        public event Action<string> OnSceneNPCsLoaded;

        #region Unity生命周期
        private void Awake()
        {
            InitializeSingleton();
            LoadNPCData();
            InitializeNPCPool();
        }

        private void Start()
        {
            RegisterSceneEvents();
            StartCoroutine(InitializeCurrentSceneNPCs());
        }

        private void OnDestroy()
        {
            UnregisterSceneEvents();
            CleanupNPCs();
        }
        #endregion

        #region 初始化
        private void InitializeSingleton()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void LoadNPCData()
        {
            try
            {
                NPCDataNew[] npcDataArray = Resources.LoadAll<NPCDataNew>("ScriptableObjects/NPCs");
                
                foreach (var data in npcDataArray)
                {
                    if (data.IsValid())
                    {
                        npcDataDictionary[data.npcID] = data;
                    }
                    else
                    {
                        Debug.LogWarning($"无效的NPC数据: {data.name}");
                    }
                }

                Debug.Log($"加载了 {npcDataDictionary.Count} 个NPC配置");
            }
            catch (Exception e)
            {
                Debug.LogError($"加载NPC数据失败: {e.Message}");
            }
        }

        private void InitializeNPCPool()
        {
            if (npcContainer == null)
            {
                GameObject container = new GameObject("NPC_Container");
                container.transform.SetParent(transform);
                npcContainer = container.transform;
            }

            // 预创建一些NPC实例到对象池
            for (int i = 0; i < 5; i++)
            {
                CreatePooledNPC();
            }
        }

        /// <summary>
        /// 创建一个新的NPC实例并添加到对象池
        /// </summary>
        /// <returns>创建的NPC实例</returns>
        private NPCCore CreatePooledNPC()
        {
            GameObject npcObj = Instantiate(npcPrefab, npcContainer);
            NPCCore npcCore = npcObj.GetComponent<NPCCore>();
            
            if (npcCore == null)
            {
                npcCore = npcObj.AddComponent<NPCCore>();
            }

            npcObj.SetActive(false);
            npcPool.Enqueue(npcCore);
            
            return npcCore;
        }
        #endregion

        #region 场景管理
        private void RegisterSceneEvents()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
            SceneManager.sceneUnloaded += OnSceneUnloaded;
        }

        private void UnregisterSceneEvents()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            SceneManager.sceneUnloaded -= OnSceneUnloaded;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            currentSceneName = scene.name;
            
            if (!IsMainMenuScene(scene.name))
            {
                StartCoroutine(LoadSceneNPCs(scene.name));
            }
        }

        private void OnSceneUnloaded(Scene scene)
        {
            UnloadSceneNPCs(scene.name);
        }

        /// <summary>
        /// 初始化当前场景的NPC
        /// </summary>
        /// <returns> IEnumerator </returns>
        private IEnumerator InitializeCurrentSceneNPCs()
        {
            yield return new WaitForEndOfFrame();
            
            if (!IsMainMenuScene(currentSceneName))
            {
                yield return StartCoroutine(LoadSceneNPCs(currentSceneName));
            }
        }

        /// <summary>
        /// 加载当前场景的NPC
        /// </summary>
        /// <param name="sceneName"> 场景名称 </param>
        /// <returns> IEnumerator </returns>
        private IEnumerator LoadSceneNPCs(string sceneName)
        {
            var sceneNPCs = GetNPCsForScene(sceneName);
            
            foreach (var npcData in sceneNPCs)
            {
                if (ShouldActivateNPC(npcData))
                {
                    ActivateNPC(npcData.npcID, npcData.defaultPosition);
                    yield return null; // 分帧加载
                }
            }

            OnSceneNPCsLoaded?.Invoke(sceneName);
            
            if (enableDebugMode)
            {
                Debug.Log($"场景 {sceneName} 加载了 {sceneNPCs.Count} 个NPC");
            }
        }

        private void UnloadSceneNPCs(string sceneName)
        {
            var npcsToRemove = activeNPCs.Values
                .Where(npc => npc.GetNPCData().sceneName == sceneName)
                .ToList();

            foreach (var npc in npcsToRemove)
            {
                DeactivateNPC(npc.NPCID);
            }
        }

        private List<NPCDataNew> GetNPCsForScene(string sceneName)
        {
            return npcDataDictionary.Values
                .Where(data => data.sceneName == sceneName)
                .ToList();
        }

        private bool IsMainMenuScene(string sceneName)
        {
            return sceneName == "MainMenu" || sceneName == "InitializationScene";
        }
        #endregion

        #region NPC管理
        public void ActivateNPC(string npcID, Vector3? position = null)
        {
            try
            {
                if (activeNPCs.ContainsKey(npcID))
                {
                    Debug.LogWarning($"NPC {npcID} 已经激活");
                    return;
                }

                if (!npcDataDictionary.TryGetValue(npcID, out NPCDataNew npcData))
                {
                    Debug.LogError($"未找到NPC数据: {npcID}");
                    return;
                }

                NPCCore npcCore = GetOrCreateNPC();
                npcCore.Initialize(npcData);
                
                // 设置位置
                Vector3 spawnPosition = position ?? FindNPCSpawnPoint(npcID) ?? npcData.defaultPosition;
                npcCore.GetTransform().position = spawnPosition;

                // 激活NPC
                npcCore.ActivateNPC();
                activeNPCs[npcID] = npcCore;

                OnNPCActivated?.Invoke(npcID);

                if (enableDebugMode)
                {
                    Debug.Log($"激活NPC: {npcID} 位置: {spawnPosition}");
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"激活NPC失败 {npcID}: {e.Message}");
            }
        }

        public void DeactivateNPC(string npcID)
        {
            try
            {
                if (activeNPCs.TryGetValue(npcID, out NPCCore npc))
                {
                    npc.DeactivateNPC();
                    activeNPCs.Remove(npcID);
                    ReturnNPCToPool(npc);

                    OnNPCDeactivated?.Invoke(npcID);

                    if (enableDebugMode)
                    {
                        Debug.Log($"禁用NPC: {npcID}");
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"禁用NPC失败 {npcID}: {e.Message}");
            }
        }

        private NPCCore GetOrCreateNPC()
        {
            if (npcPool.Count > 0)
            {
                return npcPool.Dequeue();
            }
            else
            {
                return CreatePooledNPC();
            }
        }

        private void ReturnNPCToPool(NPCCore npc)
        {
            npc.gameObject.SetActive(false);
            npc.GetTransform().position = Vector3.zero;
            npcPool.Enqueue(npc);
        }

        private Vector3? FindNPCSpawnPoint(string npcID)
        {
            GameObject spawnPoint = GameObject.Find($"NPCPoint_{npcID}");
            return spawnPoint?.transform.position;
        }
        #endregion

        #region 激活条件检查
        private bool ShouldActivateNPC(NPCDataNew npcData)
        {
            if (npcData.activationRules == null || npcData.activationRules.Count == 0)
            {
                return true; // 默认激活
            }

            foreach (var rule in npcData.activationRules)
            {
                if (!CheckActivationRule(rule))
                {
                    return false;
                }
            }

            return true;
        }

        private bool CheckActivationRule(NPCActivationRule rule)
        {
            switch (rule.activationType)
            {
                case NPCActivationType.Always:
                    return rule.shouldActivate;

                case NPCActivationType.GameStateFlag:
                    bool flagValue = GameStateManager.Instance?.GetFlag(rule.conditionKey) ?? false;
                    return flagValue == rule.shouldActivate;

                case NPCActivationType.SceneName:
                    bool sceneMatch = currentSceneName == rule.conditionValue;
                    return sceneMatch == rule.shouldActivate;

                case NPCActivationType.FirstEntry:
                    bool isFirstEntry = !loadedScenes.Contains(currentSceneName);
                    if (isFirstEntry && rule.shouldActivate)
                    {
                        loadedScenes.Add(currentSceneName);
                    }
                    return isFirstEntry == rule.shouldActivate;

                // 可以添加更多条件类型...
                default:
                    return true;
            }
        }
        #endregion

        #region 公共接口
        public NPCCore GetNPC(string npcID)
        {
            activeNPCs.TryGetValue(npcID, out NPCCore npc);
            return npc;
        }

        public List<NPCCore> GetAllActiveNPCs()
        {
            return activeNPCs.Values.ToList();
        }

        public List<NPCCore> GetNPCsByType(NPCType npcType)
        {
            return activeNPCs.Values
                .Where(npc => npc.GetNPCData().npcType == npcType)
                .ToList();
        }

        public bool IsNPCActive(string npcID)
        {
            return activeNPCs.ContainsKey(npcID);
        }

        public void ReloadNPCData()
        {
            npcDataDictionary.Clear();
            LoadNPCData();
        }
        #endregion

        #region 保存/加载
        public NPCSaveData[] GetSaveData()
        {
            return activeNPCs.Values.Select(npc => new NPCSaveData
            {
                npcID = npc.NPCID,
                position = npc.GetTransform().position,
                isFollowing = npc.IsFollowing,
                canInteract = npc.CanInteract,
                isActive = npc.IsActive,
                sceneName = npc.GetNPCData().sceneName
            }).ToArray();
        }

        public void LoadSaveData(NPCSaveData[] saveData)
        {
            // 清除当前活动的NPCs
            var currentNPCs = activeNPCs.Keys.ToList();
            foreach (var npcID in currentNPCs)
            {
                DeactivateNPC(npcID);
            }

            // 加载保存的NPCs
            foreach (var data in saveData)
            {
                if (data.sceneName == currentSceneName)
                {
                    ActivateNPC(data.npcID, data.position);
                    
                    var npc = GetNPC(data.npcID);
                    if (npc != null)
                    {
                        npc.IsFollowing = data.isFollowing;
                        npc.CanInteract = data.canInteract;
                        npc.IsActive = data.isActive;
                    }
                }
            }
        }
        #endregion

        #region 清理
        private void CleanupNPCs()
        {
            foreach (var npc in activeNPCs.Values)
            {
                if (npc != null && npc.gameObject != null)
                {
                    Destroy(npc.gameObject);
                }
            }

            activeNPCs.Clear();

            while (npcPool.Count > 0)
            {
                var pooledNPC = npcPool.Dequeue();
                if (pooledNPC != null && pooledNPC.gameObject != null)
                {
                    Destroy(pooledNPC.gameObject);
                }
            }
        }
        #endregion
    }

    [System.Serializable]
    public class NPCSaveData
    {
        public string npcID;
        public Vector3 position;
        public bool isFollowing;
        public bool canInteract;
        public bool isActive;
        public string sceneName;
    }
}