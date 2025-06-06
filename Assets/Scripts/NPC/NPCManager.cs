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
    private List<GameObject> spawnedNpcPoints = new List<GameObject>(); // 用于存储生成的NPC点

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
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    public void InitializeNPCManager(List<AsyncSaveLoadSystem.NPCSaveData> npcSaveDataList = null)
    {
        npcGameObjectList.Clear();
        npcList.Clear();
        
        if (npcSaveDataList == null || npcSaveDataList.Count == 0)
        {
            Debug.LogWarning("No NPC save data found. Initializing with default NPCs.");
            // TODO: 这里可以添加默认NPC的初始化逻辑
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
                    GameObject npcObject = Instantiate(npcPrefab);
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

    private void OnSceneLoaded(Scene scene, LoadSceneMode arg1)
    {
        if (npcList.Count == 0)
        {
            Debug.LogWarning("No NPCs found in the list. Please initialize NPCManager first.");
            return;
        }
        
        spawnedNpcPoints.Clear();
        // 查找当前场景中的所有NPC点
        spawnedNpcPoints.AddRange(GameObject.FindGameObjectsWithTag("NPCPoint"));
        
        foreach (var npc in npcList)
        {
            if (npc.isActive && !npcGameObjectList.Exists(n => n.name == npc.npcData.npcID) &&
                scene.name == npc.npcData.sceneName)
            {
                ShowNpc(npc.npcData.npcID);
            }
        }
    }

    public NPC GetNpc(string npcID)
    {
        return npcList.Find(n => n.npcData.npcID == npcID);
    }
    
    public void ShowNpc(string npcID)
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
                npcObject.transform.position = GameObject.Find(NpcPointFormat + npc.npcData.npcID).transform.position;
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