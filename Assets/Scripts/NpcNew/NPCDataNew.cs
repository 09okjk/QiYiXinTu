using System.Collections.Generic;
using Core;
using UnityEngine;

namespace NpcNew
{
    [CreateAssetMenu(fileName = "New NPCData", menuName = "NPC/NPC Data New")]
    public class NPCDataNew : EntityData
    {
        [Header("基本信息")]
        public string npcID;
        public string npcName;
        public string spriteID;
        public NPCType npcType = NPCType.General;
        
        [Header("场景信息")]
        public string sceneName;
        public Vector3 defaultPosition;
        
        [Header("行为设置")]
        public bool canInteract = true;
        public bool isFollowing = false;
        public float followSpeed = 2f;
        public float followDistance = 1.5f;
        public float interactionDistance = 2f;
        
        [Header("对话设置")]
        public List<string> dialogueIDs = new List<string>();
        public InteractionType primaryInteractionType = InteractionType.Dialogue;
        
        [Header("状态设置")]
        public NPCStateType defaultState = NPCStateType.Idle;
        public List<NPCStateType> availableStates = new List<NPCStateType>();
        
        [Header("激活条件")]
        public List<NPCActivationRule> activationRules = new List<NPCActivationRule>();
        
        [Header("扩展属性")]
        public NPCProperty[] properties;

        // 验证数据有效性
        public bool IsValid()
        {
            return !string.IsNullOrEmpty(npcID) && 
                   !string.IsNullOrEmpty(npcName) && 
                   !string.IsNullOrEmpty(spriteID);
        }
    }

    [System.Serializable]
    public class NPCActivationRule
    {
        public string ruleName;
        public NPCActivationType activationType;
        public string conditionKey;
        public string conditionValue;
        public bool shouldActivate = true;
    }

    public enum NPCActivationType
    {
        Always,
        FirstEntry,
        GameStateFlag,
        DialogueCompleted,
        ItemPossessed,
        SceneName,
        PlayerLevel
    }

    [System.Serializable]
    public class NPCProperty
    {
        public string key;
        public string value;
        
    }
}
