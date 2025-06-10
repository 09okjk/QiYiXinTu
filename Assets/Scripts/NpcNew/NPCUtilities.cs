using UnityEngine;
using System.Collections.Generic;
using NpcNew;

namespace NpcNew
{
    /// <summary>
    /// NPC资源管理器
    /// </summary>
    public static class NPCResourceManager
    {
        private static Dictionary<string, Sprite> spriteCache = new Dictionary<string, Sprite>();
        private static Dictionary<string, DialogueData> dialogueCache = new Dictionary<string, DialogueData>();

        public static Sprite LoadSprite(string spriteID)
        {
            if (string.IsNullOrEmpty(spriteID)) return null;

            if (spriteCache.TryGetValue(spriteID, out Sprite cachedSprite))
            {
                return cachedSprite;
            }

            Sprite sprite = Resources.Load<Sprite>($"Art/NPCs/{spriteID}");
            if (sprite == null)
            {
                sprite = Resources.Load<Sprite>("Art/NPCs/default_avatar");
            }

            if (sprite != null)
            {
                spriteCache[spriteID] = sprite;
            }

            return sprite;
        }

        public static DialogueData LoadDialogue(string dialogueID)
        {
            if (string.IsNullOrEmpty(dialogueID)) return null;

            if (dialogueCache.TryGetValue(dialogueID, out DialogueData cachedDialogue))
            {
                return cachedDialogue;
            }

            DialogueData dialogue = Resources.Load<DialogueData>($"ScriptableObjects/Dialogues/{dialogueID}");
            if (dialogue != null)
            {
                dialogueCache[dialogueID] = dialogue;
            }

            return dialogue;
        }

        public static void ClearCache()
        {
            spriteCache.Clear();
            dialogueCache.Clear();
        }
    }

    /// <summary>
    /// NPC事件总线
    /// </summary>
    public class NPCEventBus : MonoBehaviour
    {
        public static NPCEventBus Instance { get; private set; }

        private Dictionary<string, NPCCore> registeredNPCs = new Dictionary<string, NPCCore>();

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

        /// <summary>
        /// 注册NPC到事件总线
        /// </summary>
        /// <param name="npc">要注册的NPC实例</param>
        public void RegisterNPC(NPCCore npc)
        {
            if (!string.IsNullOrEmpty(npc.NPCID))
            {
                registeredNPCs[npc.NPCID] = npc;
            }
        }

        /// <summary>
        /// 取消注册NPC
        /// </summary>
        /// <param name="npc">要取消注册的NPC实例</param>
        public void UnregisterNPC(NPCCore npc)
        {
            if (!string.IsNullOrEmpty(npc.NPCID))
            {
                registeredNPCs.Remove(npc.NPCID);
            }
        }

        public NPCCore GetNPC(string npcID)
        {
            registeredNPCs.TryGetValue(npcID, out NPCCore npc);
            return npc;
        }

        public void BroadcastEvent(string eventName, object data = null)
        {
            foreach (var npc in registeredNPCs.Values)
            {
                // 处理全局NPC事件
            }
        }
    }

    /// <summary>
    /// NPC日志系统
    /// </summary>
    public static class NPCLogger
    {
        public static bool EnableLogging = true;

        public static void Log(string message, NPCCore npc = null)
        {
            if (!EnableLogging) return;
            
            string prefix = npc != null ? $"[NPC:{npc.NPCID}] " : "[NPC] ";
            Debug.Log(prefix + message);
        }

        public static void LogWarning(string message, NPCCore npc = null)
        {
            if (!EnableLogging) return;
            
            string prefix = npc != null ? $"[NPC:{npc.NPCID}] " : "[NPC] ";
            Debug.LogWarning(prefix + message);
        }

        public static void LogError(string message, NPCCore npc = null)
        {
            string prefix = npc != null ? $"[NPC:{npc.NPCID}] " : "[NPC] ";
            Debug.LogError(prefix + message);
        }
    }
    
}