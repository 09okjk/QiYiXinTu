using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Manager;
using Save;
using UnityEngine;
using UnityEngine.SceneManagement;
using Utils;

public class NPCManager : MonoBehaviour
{
    public const string NpcPointFormat = "NPCPoint_";
    
    public static NPCManager Instance { get; private set; }
    
    [Header("NPC设置")]
    [SerializeField] private GameObject npcPrefab;
    [SerializeField] private int initialPoolSize = 10;
    [SerializeField] private bool useObjectPool = true;
    
    // 原始NPC数据（只读）
    private NPCData[] originalNpcDataList;
    // 运行时NPC数据副本
    private Dictionary<string, NPCData> runtimeNpcDataDictionary = new Dictionary<string, NPCData>();
    private Dictionary<string, GameObject> npcObjectDictionary = new Dictionary<string, GameObject>();
    
    // 对象池
    private Queue<GameObject> npcPool = new Queue<GameObject>();
    private List<GameObject> activeNPCs = new List<GameObject>();
    
    // 当前场景相关
    private string currentSceneName;
    private List<NPCData> currentSceneNPCs = new List<NPCData>();

    #region Unity生命周期

    private void Awake()
    {
        InitializeSingleton();
        LoadOriginalNPCData();
        CreateRuntimeDataCopies();
        InitializeObjectPool();
    }
    
    private void Start()
    {
        currentSceneName = SceneManager.GetActiveScene().name;
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
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
            Debug.Log("NPCManager 初始化完成");
        }
        else
        {
            Debug.LogWarning("发现多个NPCManager实例，销毁重复实例");
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// 加载原始NPC数据（只读）
    /// </summary>
    private void LoadOriginalNPCData()
    {
        try
        {
            originalNpcDataList = Resources.LoadAll<NPCData>("ScriptableObjects/NPCs");
            
            if (originalNpcDataList == null || originalNpcDataList.Length == 0)
            {
                Debug.LogWarning("未找到NPC数据文件");
                return;
            }
            
            Debug.Log($"成功加载 {originalNpcDataList.Length} 个原始NPC数据");
        }
        catch (Exception e)
        {
            Debug.LogError($"加载原始NPC数据时发生错误: {e.Message}");
        }
    }

    /// <summary>
    /// 创建运行时数据副本
    /// </summary>
    private void CreateRuntimeDataCopies()
    {
        runtimeNpcDataDictionary.Clear();
    
        if (originalNpcDataList == null) return;

        foreach (var originalData in originalNpcDataList)
        {
            if (originalData != null && !string.IsNullOrEmpty(originalData.npcID))
            {
                // 使用增强后的工具类
                var runtimeCopy = Utils.ScriptableObjectUtils.CreateNPCDataCopy(originalData);
                runtimeNpcDataDictionary[originalData.npcID] = runtimeCopy;
            }
        }
    
        Debug.Log($"创建了 {runtimeNpcDataDictionary.Count} 个NPC运行时数据副本");
    }

    /// <summary>
    /// 重置所有NPC数据到原始状态
    /// </summary>
    public void ResetAllNPCData()
    {
        foreach (var originalData in originalNpcDataList)
        {
            if (originalData != null && runtimeNpcDataDictionary.ContainsKey(originalData.npcID))
            {
                var runtimeData = runtimeNpcDataDictionary[originalData.npcID];
                ScriptableObjectUtils.ResetToOriginal(originalData, runtimeData);
            }
        }
        
        Debug.Log("已重置所有NPC数据到原始状态");
    }

    /// <summary>
    /// 清理运行时数据
    /// </summary>
    private void CleanupRuntimeData()
    {
        ScriptableObjectUtils.SafeDestroyRuntimeCopies(runtimeNpcDataDictionary.Values);
        runtimeNpcDataDictionary.Clear();
    }

    private void InitializeObjectPool()
    {
        if (!useObjectPool || npcPrefab == null)
        {
            return;
        }

        for (int i = 0; i < initialPoolSize; i++)
        {
            GameObject pooledNPC = CreateNPCObject();
            if (pooledNPC != null)
            {
                pooledNPC.SetActive(false);
                npcPool.Enqueue(pooledNPC);
            }
        }
        
        Debug.Log($"NPC对象池初始化完成，初始大小: {npcPool.Count}");
    }

    #endregion

    #region 场景管理

    private void OnSceneLoaded(Scene scene, LoadSceneMode loadMode)
    {
        currentSceneName = scene.name;
        Debug.Log($"场景切换到: {currentSceneName}");
        
        // 清理当前场景的NPC
        CleanupCurrentSceneNPCs();
        
        // 加载新场景的NPC数据
        LoadCurrentSceneNPCs();
    }

    private void LoadCurrentSceneNPCs()
    {
        currentSceneNPCs.Clear();
        
        foreach (var runtimeData in runtimeNpcDataDictionary.Values)
        {
            if (runtimeData != null && runtimeData.sceneName == currentSceneName)
            {
                currentSceneNPCs.Add(runtimeData);
            }
        }
        
        Debug.Log($"当前场景 {currentSceneName} 需要加载 {currentSceneNPCs.Count} 个NPC");
    }

    private void CleanupCurrentSceneNPCs()
    {
        // 将激活的NPC返回对象池或销毁
        for (int i = activeNPCs.Count - 1; i >= 0; i--)
        {
            var npcObject = activeNPCs[i];
            if (npcObject != null)
            {
                ReturnNPCToPool(npcObject);
            }
        }
        
        activeNPCs.Clear();
        Debug.Log("清理了当前场景的所有NPC");
    }

    #endregion

    #region NPC对象管理

    private GameObject CreateNPCObject()
    {
        if (npcPrefab == null)
        {
            Debug.LogError("NPC预制体未设置");
            return null;
        }

        try
        {
            GameObject npcObject = Instantiate(npcPrefab, transform);
            return npcObject;
        }
        catch (Exception e)
        {
            Debug.LogError($"创建NPC对象时发生错误: {e.Message}");
            return null;
        }
    }

    private GameObject GetNPCFromPool()
    {
        if (npcPool.Count > 0)
        {
            return npcPool.Dequeue();
        }
        
        // 对象池为空时创建新对象
        return CreateNPCObject();
    }

    private void ReturnNPCToPool(GameObject npcObject)
    {
        if (npcObject == null) return;

        // 重置NPC状态
        var npc = npcObject.GetComponent<NPC>();
        if (npc != null)
        {
            npc.ResetNPC(); // 需要在NPC类中实现这个方法
        }

        npcObject.SetActive(false);
        
        if (useObjectPool)
        {
            npcPool.Enqueue(npcObject);
        }
        else
        {
            Destroy(npcObject);
        }
    }

    #endregion

    #region NPC显示和隐藏

    public bool ShowNPC(string npcID, GameObject npcPoint)
    {
        if (string.IsNullOrEmpty(npcID))
        {
            Debug.LogError("NPC ID 为空");
            return false;
        }

        // 使用运行时数据副本
        if (!runtimeNpcDataDictionary.ContainsKey(npcID))
        {
            Debug.LogError($"未找到ID为 {npcID} 的NPC运行时数据");
            return false;
        }

        NPCData npcData = runtimeNpcDataDictionary[npcID];
        
        // 检查是否应该在当前场景显示此NPC
        if (!ShouldShowNPCInCurrentScene(npcData))
        {
            Debug.Log($"NPC {npcID} 不应该在当前场景 {currentSceneName} 中显示");
            return false;
        }

        try
        {
            GameObject npcObject = GetOrCreateNPCObject(npcID, npcData);
            if (npcObject == null) return false;

            // 设置NPC位置
            SetNPCPosition(npcObject, npcPoint, npcData);
            
            // 配置NPC组件
            ConfigureNPCComponent(npcObject, npcData);
            
            // 激活NPC
            ActivateNPC(npcObject, npcData);
            
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"显示NPC {npcID} 时发生错误: {e.Message}");
            return false;
        }
    }

    private GameObject GetOrCreateNPCObject(string npcID, NPCData npcData)
    {
        // 检查是否已存在
        if (npcObjectDictionary.ContainsKey(npcID))
        {
            var existingObject = npcObjectDictionary[npcID];
            if (existingObject != null)
            {
                return existingObject;
            }
            else
            {
                npcObjectDictionary.Remove(npcID); // 清理无效引用
            }
        }

        // 从对象池获取或创建新对象
        GameObject npcObject = GetNPCFromPool();
        if (npcObject == null) return null;

        npcObject.name = npcID;
        npcObjectDictionary[npcID] = npcObject;
        activeNPCs.Add(npcObject);

        return npcObject;
    }

    private void SetNPCPosition(GameObject npcObject, GameObject npcPoint, NPCData npcData)
    {
        var npc = npcObject.GetComponent<NPC>();
        if (npc == null)
        {
            Debug.LogError($"NPC对象 {npcData.npcID} 上未找到NPC组件");
            return;
        }
        npc.isFollowing = GameStateManager.Instance.GetFlag("Following_" + npcData.npcID);
        if (npc != null && npc.isFollowing)
        {
            // 跟随玩家的NPC
            SetNPCFollowPosition(npcObject);
        }
        else if (npcPoint != null)
        {
            // 设置到指定的NPC点位置
            npcObject.transform.position = npcPoint.transform.position;
            Debug.Log($"NPC {npcData.npcID} 设置到位置: {npcPoint.transform.position}");
        }
        else
        {
            Debug.LogWarning($"NPC {npcData.npcID} 没有有效的位置设置");
        }
    }

    private void SetNPCFollowPosition(GameObject npcObject)
    {
        if (PlayerManager.Instance?.player != null)
        {
            Vector3 playerPosition = PlayerManager.Instance.player.transform.position;
            Vector3 followOffset = PlayerManager.Instance.player.transform.forward * -2f;
            npcObject.transform.position = playerPosition + followOffset;
        }
    }

    private void ConfigureNPCComponent(GameObject npcObject, NPCData npcData)
    {
        var npc = npcObject.GetComponent<NPC>();
        if (npc == null)
        {
            Debug.LogError($"NPC对象 {npcData.npcID} 上未找到NPC组件");
            return;
        }

        // 设置NPC数据
        npc.npcData = npcData;
        npc.dialogueIDs = npcData.dialogueIDs;
        // npc.canInteract = npcData.canInteract;
    }

    private void ActivateNPC(GameObject npcObject, NPCData npcData)
    {
        var npc = npcObject.GetComponent<NPC>();
        if (npc == null) return;

        // 检查特殊条件
        if (ShouldNPCBeActive(npcData))
        {
            npc.ActivateNpc();
            npcObject.SetActive(true);
            Debug.Log($"激活NPC: {npcData.npcID}");
        }
        else
        {
            npc.DeactivateNpc();
            Debug.Log($"NPC {npcData.npcID} 因特殊条件未激活");
        }
    }

    private bool ShouldShowNPCInCurrentScene(NPCData npcData)
    {
        return npcData.sceneName == currentSceneName || npcData.sceneName == "AllScenes";
    }

    private bool ShouldNPCBeActive(NPCData npcData)
    {
        // 检查游戏状态标志
        if (GameStateManager.Instance != null)
        {
            string firstEntryFlag = "FirstEntry_" + currentSceneName;
            bool isFirstEntry = GameStateManager.Instance.GetFlag(firstEntryFlag);
            
            // 根据NPC的特殊配置决定是否激活
            return !IsNPCSpeciallyDeactivated(npcData, isFirstEntry);
        }
        
        return true; // 默认激活
    }

    private bool IsNPCSpeciallyDeactivated(NPCData npcData, bool isFirstEntry)
    {
        // 可以在NPCData中添加特殊规则字段，而不是硬编码
        // 这里暂时保留原逻辑但使其可配置
        if (isFirstEntry && npcData.npcID == "LuXinsheng")
        {
            return true;
        }
        
        return false;
    }

    public void HideNPC(string npcID)
    {
        if (npcObjectDictionary.ContainsKey(npcID))
        {
            var npcObject = npcObjectDictionary[npcID];
            if (npcObject != null)
            {
                ReturnNPCToPool(npcObject);
                activeNPCs.Remove(npcObject);
                npcObjectDictionary.Remove(npcID);
                Debug.Log($"隐藏NPC: {npcID}");
            }
        }
    }

    #endregion

    #region NPC查询

    public NPC GetNPC(string npcID)
    {
        if (string.IsNullOrEmpty(npcID))
        {
            Debug.LogError("NPC ID 为空");
            return null;
        }

        if (npcObjectDictionary.ContainsKey(npcID))
        {
            var npcObject = npcObjectDictionary[npcID];
            if (npcObject != null)
            {
                return npcObject.GetComponent<NPC>();
            }
        }

        Debug.LogWarning($"未找到ID为 {npcID} 的NPC对象");
        return null;
    }

    public List<NPC> GetActiveNPCs()
    {
        List<NPC> activeNPCComponents = new List<NPC>();
        
        foreach (var npcObject in activeNPCs)
        {
            if (npcObject != null && npcObject.activeInHierarchy)
            {
                var npc = npcObject.GetComponent<NPC>();
                if (npc != null)
                {
                    activeNPCComponents.Add(npc);
                }
            }
        }
        Debug.Log("获取到当前激活的NPC数量: " + activeNPCComponents.Count);
        return activeNPCComponents;
    }

    public bool IsNPCActive(string npcID)
    {
        var npc = GetNPC(npcID);
        return npc != null && npc.gameObject.activeInHierarchy;
    }

    #endregion

    #region 数据保存和加载

    public void InitializeNPCManager(List<AsyncSaveLoadSystem.NPCSaveData> npcSaveDataList = null)
    {
        if (npcSaveDataList == null || npcSaveDataList.Count == 0)
        {
            Debug.Log("没有NPC保存数据，使用默认设置");
            return;
        }

        try
        {
            foreach (var saveData in npcSaveDataList)
            {
                LoadNPCFromSaveData(saveData);
            }
            
            Debug.Log($"从保存数据中加载了 {npcSaveDataList.Count} 个NPC");
        }
        catch (Exception e)
        {
            Debug.LogError($"初始化NPC管理器时发生错误: {e.Message}");
        }
    }

    private void LoadNPCFromSaveData(AsyncSaveLoadSystem.NPCSaveData saveData)
    {
        if (string.IsNullOrEmpty(saveData.npcID)) return;

        // 使用运行时数据副本
        if (!runtimeNpcDataDictionary.ContainsKey(saveData.npcID)) return;

        var npcObject = GetOrCreateNPCObject(saveData.npcID, runtimeNpcDataDictionary[saveData.npcID]);
        if (npcObject == null) return;

        // 设置位置
        Vector3 position = new Vector3(saveData.position[0], saveData.position[1], saveData.position[2]);
        npcObject.transform.position = position;

        // 设置NPC状态（修改运行时副本，不影响原始资源）
        var npc = npcObject.GetComponent<NPC>();
        if (npc != null)
        {
            npc.isFollowing = saveData.isFollowing;
            npc.dialogueIDs = saveData.dialogueIDs;
            npc.isActive = saveData.isActive;
            npc.canInteract = saveData.canInteract;
            
            // 修改运行时数据副本
            var runtimeData = runtimeNpcDataDictionary[saveData.npcID];
            runtimeData.sceneName = saveData.sceneName;
            npc.npcData = runtimeData;
        }
    }

    #endregion

    #region 调试方法

    [ContextMenu("显示所有激活的NPC")]
    public void DebugShowActiveNPCs()
    {
        Debug.Log($"当前激活的NPC数量: {activeNPCs.Count}");
        foreach (var npcObject in activeNPCs)
        {
            if (npcObject != null)
            {
                Debug.Log($"- {npcObject.name} (激活: {npcObject.activeInHierarchy})");
            }
        }
    }

    [ContextMenu("显示对象池状态")]
    public void DebugShowPoolStatus()
    {
        Debug.Log($"对象池大小: {npcPool.Count}");
    }

    #endregion
}