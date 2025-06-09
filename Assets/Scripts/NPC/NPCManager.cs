using System;
using System.Collections.Generic;
using Manager;
using Save;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NPCManager:MonoBehaviour
{
    public const string NpcPointFormat = "NPCPoint_";
    
    public static NPCManager Instance { get; private set; }
    public List<GameObject> npcGameObjectList = new List<GameObject>();
    public GameObject npcPrefab;
    public List<NPC> npcList = new List<NPC>();
    private NPCData[] npcDataList;

    private void Awake()
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
        // 从Resources文件夹下的ScriptableObjects/NPCs文件中加载所有的NPC配置
        npcDataList = Resources.LoadAll<NPCData>("ScriptableObjects/NPCs");
    }
    
    private void Start()
    {
        // SceneManager.sceneLoaded += OnSceneLoaded;
        LoadAllNpcs();
    }

    private void LoadAllNpcs()
    {
        foreach (var npcData in npcDataList)
        {
            GameObject npcObject = Instantiate(npcPrefab,transform);
            npcObject.name = npcData.npcID; // 设置GameObject名称为NPC ID
            NPC npcComponent = npcObject.GetComponent<NPC>();
            npcComponent.npcData = npcData;
            npcComponent.isFollowing = false;
            npcComponent.dialogueIDs = npcData.dialogueIDs;
            npcComponent.isActive = false;
            npcComponent.canInteract = false;
            npcComponent.npcData.sceneName = npcData.sceneName;
            npcGameObjectList.Add(npcObject);
            npcList.Add(npcComponent);
            npcObject.SetActive(false);
        }
    }

    private void OnDestroy()
    {
        // SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    public void InitializeNPCManager(List<AsyncSaveLoadSystem.NPCSaveData> npcSaveDataList = null)
    {
        npcGameObjectList.Clear();
        npcList.Clear();
        
        if (npcSaveDataList == null || npcSaveDataList.Count == 0)
        {
            Debug.LogWarning("No NPC save data found. Initializing with default NPCs.");
        }
        else
        {
            // 遍历保存的数据，将每个NPC的保存数据转换为NPC对象
            foreach (var npcSaveData in npcSaveDataList)
            {
                // 查找对应的NPC配置
                NPCData npcData = Array.Find(npcDataList, n => n.npcID == npcSaveData.npcID);
                if (npcData != null)
                {
                    // 实例化NPC GameObject
                    GameObject npcObject = Instantiate(npcPrefab,transform);
                    npcObject.name = npcData.npcID; // 设置GameObject名称为NPC ID
                    npcObject.transform.position = new Vector3(npcSaveData.position[0], npcSaveData.position[1], npcSaveData.position[2]);
                    NPC npcComponent = npcObject.GetComponent<NPC>();
                    npcComponent.npcData = npcData;
                    npcComponent.isFollowing = npcSaveData.isFollowing;
                    npcComponent.dialogueIDs = npcSaveData.dialogueIDs;
                    npcComponent.isActive = npcSaveData.isActive;
                    npcComponent.canInteract = npcSaveData.canInteract;
                    npcComponent.npcData.sceneName = npcSaveData.sceneName;
                    // 添加到列表中
                    npcGameObjectList.Add(npcObject);
                    npcList.Add(npcComponent);
                }
            }
        }
    }

    public NPC GetNpc(string npcID)
    {
        return npcGameObjectList.Find(n => n.name == npcID).GetComponent<NPC>();
    }
    
    public void ShowNpc(string npcID, GameObject npcPoint = null)
    {
        var npcObject = npcGameObjectList.Find(n => n.gameObject.name == npcID);
        if (!npcObject)
        {
            Debug.LogError($"NPC with ID {npcID} not found in the list.");
            return;
        }
        
        var npc = npcObject.GetComponent<NPC>();
        if (!npc)
        {
            Debug.LogError($"NPC component not found on GameObject with ID {npcID}.");
            return;
        }
        
        try
        {
            if (npc.isFollowing)
            {
                // 在玩家附近生成
                Vector3 playerPosition = PlayerManager.Instance.player.transform.position;
                // 在玩家后方生成NPC
                Vector3 spawnPosition = playerPosition + PlayerManager.Instance.player.transform.forward * -2f;
                npcObject.transform.position = spawnPosition;
            }
            else
            {
                if (npcPoint != null) npcObject.transform.position = npcPoint.transform.position;
            }
            
            if(GameStateManager.Instance.GetFlag("FirstEntry_" + SceneManager.GetActiveScene().name))
            {
                // 如果是第一次进入该场景，设置NPC为非激活状态
                if (npcID == "LuXinsheng")
                {
                    npc.DeactivateNpc();
                    return;
                }
            }
            npc.ActivateNpc();
        }
        catch (Exception e)
        {
            Debug.LogError($"Error showing NPC {npcID}: {e.Message}");
            throw;
        }

    }
}