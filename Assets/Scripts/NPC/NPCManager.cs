using System;
using System.Collections.Generic;
using Manager;
using UnityEngine;

public class NPCManager:MonoBehaviour
{
    public static NPCManager Instance { get; private set; }
    public List<GameObject> npcGameObjectList = new List<GameObject>();
    public List<NPC> npcList = new List<NPC>();

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
    }
    
    private void Start()
    {
        // 初始化NPC管理器
        InitializeNPCManager();
    }

    private void InitializeNPCManager()
    {
        // NPC[] npcs = GetComponentsInChildren<NPC>();
        GameObject [] npcObjects = GameObject.FindGameObjectsWithTag("NPC");
        foreach (var npc in npcObjects)
        {
            npcGameObjectList.Add(npc);
            NPC npcComponent = npc.GetComponent<NPC>();
            npcList.Add(npcComponent);
            npc.GetComponent<NPC>().DeactivateNpc();
        }
    }

    public NPC GetNpc(string npcID)
    {
        return npcList.Find(n => n.npcData.npcID == npcID);
    }
    
    public void ShowNpc(string npcID)
    {
        var npc = npcGameObjectList.Find(n => n.gameObject.name == npcID);
        if (GameStateManager.Instance.GetFlag("Following_" + npcID))
        {
            // 在玩家附近生成
            Vector3 playerPosition = PlayerManager.Instance.player.transform.position;
            // 在玩家后方生成NPC
            Vector3 spawnPosition = playerPosition + PlayerManager.Instance.player.transform.forward * -2f;
            npc.transform.position = spawnPosition;
        }
        if (npc)
        {
            npc.GetComponent<NPC>().ActivateNpc();
        }
        else
        {
            Debug.LogWarning($"NPC with ID {npcID} not found.");
        }
    }
}