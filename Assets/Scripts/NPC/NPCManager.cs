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
    public static NPCManager Instance { get; private set; }
    
    [Header("NPC设置")]
    [SerializeField] private List<GameObject> npcPrefabs = new List<GameObject>();
    [SerializeField] private int initialPoolSize = 10;
    [SerializeField] private bool useObjectPool = true;
    
    // 运行时NPC数据副本
    private Dictionary<string, NpcGameData> runtimeNpcDataDictionary = new Dictionary<string, NpcGameData>();
    private readonly Dictionary<string, NPC> npcDictionary = new Dictionary<string, NPC>();

    #region Unity生命周期

    private void Awake()
    {
        InitializeSingleton();
        InitializeNpcObjectList();
    }
    
    private void Start()
    {
    }

    private void OnDestroy()
    {
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
    /// 保存所有Npc运行时数据
    /// </summary>
    private void SaveAllNpcRuntimeData()
    {
        foreach (var npc in npcDictionary.Values)
        {
            if (npc != null)
            {
                SaveNpcRuntimeData(npc.GetNpcGameData().npcID);
            }
        }
        Debug.Log($"保存了 {runtimeNpcDataDictionary.Count} 个NPC运行时数据");
    }
    
    /// <summary>
    /// 保存指定NPC的运行时数据
    /// </summary>
    /// <param name="npcID">NPC的唯一标识符</param>
    private void SaveNpcRuntimeData(string npcID)
    {
        if (npcDictionary.TryGetValue(npcID, out var npc))
        {
            if (npc != null)
            {
                runtimeNpcDataDictionary[npcID] = npc.GetNpcGameData();
                Debug.Log($"保存NPC {npcID} 的运行时数据");
            }
        }
        else
        {
            Debug.LogWarning($"未找到NPC {npcID}，无法保存运行时数据");
        }
    }

    /// <summary>
    /// 重置所有NPC数据到原始状态
    /// </summary>
    public void ResetAllNPCData()
    {
        foreach (var npcComponent in npcDictionary.Values)
        {
            npcComponent.ResetNPC(); // 假设NPC类有一个ResetNPC方法来重置状态
        }
        
        Debug.Log("已重置所有NPC数据到原始状态");
    }

    /// <summary>
    /// 清理运行时数据
    /// </summary>
    private void CleanupRuntimeData()
    {
        runtimeNpcDataDictionary.Clear();
    }

    private void InitializeNpcObjectList()
    {
        if (npcPrefabs == null)
        {
            Debug.LogError("NPC预制体列表未设置");
            return;
        }

        foreach (var npc in npcPrefabs.Select(CreateNpcObject).Where(npc => npc != null))
        {
            npc.SetActive(false);
            NPC npcComponent = npc.GetComponent<NPC>();
            npcComponent.SetNpcGameData();
            if (npcComponent != null)
            {
                npcDictionary[npcComponent.GetNpcGameData().npcID] = npcComponent;
            }
        }
        // 保存为初始运行时数据副本
        SaveAllNpcRuntimeData();
        
        Debug.Log($"NPC对象列表初始化完成，初始大小: {npcPrefabs.Count}");
    }

    #endregion

    #region 场景管理

    /// <summary>
    /// 加载当前场景的NPC数据
    /// </summary>
    /// <param name="currentSceneName">当前场景名称</param>
    public bool LoadCurrentSceneNpCs(string currentSceneName)
    {
        try
        {
            int npcCount = 0;
            foreach (var runtimeData in runtimeNpcDataDictionary.Values)
            {
                if (runtimeData != null && runtimeData.sceneName == currentSceneName)
                {
                    SetNpc(npcDictionary[runtimeData.npcID], runtimeData);
                    npcCount++;
                }
            }

            Debug.Log($"加载场景 {currentSceneName} 的NPC数据，共加载 {npcCount} 个NPC");
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"加载场景 {currentSceneName} 的NPC数据时发生错误: {e.Message}");
            return false;
        }
    }

    #endregion

    #region NPC对象管理

    private GameObject CreateNpcObject(GameObject npcPrefab)
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

    private void SetNpc(NPC npcComponent, NpcGameData npcGameData = null)
    {
        npcComponent.SetNpcGameData(npcGameData);
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
        
        if (!npcDictionary.ContainsKey(npcID))
        {
            Debug.LogError($"未找到ID为 {npcID} 的NPC");
            return false;
        }

        NPC npc = npcDictionary[npcID];
        
        // 检查是否应该在当前场景显示此NPC
        if (!ShouldShowNPCInCurrentScene(npc.GetNpcGameData()))
        {
            Debug.Log($"NPC {npcID} 不应该在当前场景{SceneManager.GetActiveScene().name}中显示");
            return false;
        }

        try
        {
            GameObject npcObject = npcDictionary[npcID].gameObject;
            if (npcObject == null) return false;

            // 设置NPC位置
            SetNPCPosition(npcPoint, npc);
            
            // 激活NPC
            ActivateNPC(npc);
            
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"显示NPC {npcID} 时发生错误: {e.Message}");
            return false;
        }
    }

    private void SetNPCPosition( GameObject npcPoint, NPC npc)
    {
        if (npc == null)
        {
            Debug.LogError($"NPC对象 {npc.GetNpcGameData().npcID} 上未找到NPC组件");
            return;
        }
        if (npc != null && npc.GetNpcGameData().isFollowing)
        {
            // 设置跟随玩家的NPC的位置
            SetNPCFollowPosition(npc.gameObject);
        }
        else if (npcPoint != null)
        {
            // 设置到指定的NPC点位置
            npc.SetPosition(npcPoint.transform.position);
            Debug.Log($"NPC {npc.GetNpcGameData().npcID} 设置到位置: {npcPoint.transform.position}");
        }
        else
        {
            Debug.LogWarning($"NPC {npc.GetNpcGameData().npcID} 没有有效的位置设置");
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

    public void ActivateNPC(NPC npc)
    {
        if (npc == null) return;
        npc.ActivateNpc();
    }

    private bool ShouldShowNPCInCurrentScene(NpcGameData npcData)
    {
        return npcData.sceneName == SceneManager.GetActiveScene().name || npcData.sceneName == "AllScenes";
    }

    public void HideNPC(NPC npc)
    {
        if (npc == null) return;
        npc.DeactivateNpc();
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

        if (npcDictionary.ContainsKey(npcID))
        {
            var npc = npcDictionary[npcID];
            if (npc != null)
            {
                return npc;
            }
        }

        Debug.LogWarning($"未找到ID为 {npcID} 的NPC对象");
        return null;
    }
    
    public bool IsNPCActive(string npcID)
    {
        var npc = GetNPC(npcID);
        return npc != null && npc.gameObject.activeInHierarchy;
    }

    #endregion

    #region 数据保存和加载
    
    public Dictionary<string,NpcGameData> GetAllNPCData()
    {
        SaveAllNpcRuntimeData();
        return new Dictionary<string, NpcGameData>(runtimeNpcDataDictionary);
    }

    public bool SetNpcDatas(Dictionary<string, NpcGameData> npcDataDictionary)
    {
        if (npcDataDictionary == null || npcDataDictionary.Count == 0)
        {
            Debug.LogWarning("传入的NPC数据字典为空或无效");
            return false;
        }
        
        runtimeNpcDataDictionary = new Dictionary<string, NpcGameData>(npcDataDictionary);
        return true;
    }

    #endregion
}