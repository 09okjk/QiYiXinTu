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
            DontDestroyOnLoad(gameObject);
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
        // 加载场景中所有的NPC
        NPC[] npcs = FindObjectsOfType<NPC>();
        foreach (NPC npc in npcs)
        {
            npcList.Add(npc);
        }
    }

    public NPC GetNpc(string npcID)
    {
        return npcList.Find(n => n.npcData.npcID == npcID);
    }
}