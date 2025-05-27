using System;
using System.Collections.Generic;
using UnityEngine;

public class NPCManager:MonoBehaviour
{
    public static NPCManager Instance { get; private set; }
    private List<NPC> npcList = new List<NPC>();

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
        NPC[] npcs = GetComponentsInChildren<NPC>();
        foreach (NPC npc in npcs)
        {
            npcList.Add(npc);
            npc.DeactivateNpc();
        }
    }

    public NPC GetNpc(string npcID)
    {
        return npcList.Find(n => n.npcData.npcID == npcID);
    }
    
    public void ShowNpc(string npcID)
    {
        NPC npc = GetNpc(npcID);
        if (npc)
        {
            npc.ActivateNpc();
        }
        else
        {
            Debug.LogWarning($"NPC with ID {npcID} not found.");
        }
    }
}