using UnityEngine;
using System.Collections.Generic;

public class NPC : MonoBehaviour
{
    [Header("NPC Data")]
    [SerializeField] private NPCData npcData;
    
    [Header("交互设置")]
    [SerializeField] private float interactionDistance = 2f; // 交互距离
    [SerializeField] private GameObject interactionIndicator;// 交互提示UI
    
    private bool canInteract = false; // 是否可以交互
    private DialogueData cachedDialogue; // 缓存对话数据
    
    private void Start()
    {
        // 初始设置
        if (interactionIndicator != null)
            interactionIndicator.SetActive(false);
            
        // 根据NPC类型设置外观或行为
        SetupNPCBasedOnType();
        
        // 预加载对话数据
        if (!string.IsNullOrEmpty(npcData.dialogueID))
            cachedDialogue = Resources.Load<DialogueData>($"Dialogues/{npcData.dialogueID}");
    }
    
    private void Update()
    {
        // 检查玩家是否在交互距离内
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            float distance = Vector2.Distance(transform.position, player.transform.position);
            bool wasInteractable = canInteract;
            canInteract = distance <= interactionDistance;
            
            // 更新交互提示
            if (interactionIndicator != null && wasInteractable != canInteract)
                interactionIndicator.SetActive(canInteract);
            
            // 处理交互输入
            if (canInteract && Input.GetKeyDown(KeyCode.E))
            {
                Interact();
            }
        }
    }
    
    private void SetupNPCBasedOnType()
    {
        // 根据NPC类型设置组件或行为
        switch (npcData.npcType)
        {
            case NPCType.Merchant:
                // 可能添加商店组件
                break;
            case NPCType.QuestGiver:
                // 可能添加任务指示器
                break;
            // 添加其他类型的处理...
        }
    }
    
    private void Interact()
    {
        // 根据NPC类型执行不同的交互
        switch (npcData.npcType)
        {
            case NPCType.Merchant:
                OpenShop();
                break;
            case NPCType.QuestGiver:
                CheckAndOfferQuests();
                break;
            default:
                // 默认交互是对话
                TriggerDialogue();
                break;
        }
    }
    
    private void TriggerDialogue()
    {
        if (cachedDialogue != null)
        {
            DialogueManager.Instance.StartDialogue(cachedDialogue);
        }
        else if (!string.IsNullOrEmpty(npcData.dialogueID))
        {
            // 尝试加载对话
            DialogueData dialogue = Resources.Load<DialogueData>($"Dialogues/{npcData.dialogueID}");
            if (dialogue != null)
            {
                cachedDialogue = dialogue;
                DialogueManager.Instance.StartDialogue(dialogue);
            }
            else
            {
                Debug.LogWarning($"无法找到对话数据: {npcData.dialogueID}");
            }
        }
    }
    
    private void OpenShop()
    {
        // 实现打开商店界面的逻辑
        Debug.Log($"打开{npcData.npcName}的商店");
        // ShopManager.Instance.OpenShop(npcData);
    }
    
    private void CheckAndOfferQuests()
    {
        // 检查是否有可用任务
        bool hasQuests = false;
        
        foreach (string questID in npcData.availableQuestIDs)
        {
            // 检查任务是否可以提供（既不是活跃的也不是已完成的）
            if (CanOfferQuest(questID))
            {
                hasQuests = true;
                break;
            }
        }
        
        // 首先进行对话，然后在对话结束时提供任务
        TriggerDialogue();
        
        if (hasQuests)
        {
            // 在对话结束后提供任务，使用正确的事件名称
            DialogueManager.Instance.OnDialogueEnd += OfferAvailableQuests;
        }
    }
    
    // 新增辅助方法：检查任务是否可以提供
    private bool CanOfferQuest(string questID)
    {
        // 任务可以提供的条件：既不是活跃的也不是已完成的
        return !QuestManager.Instance.IsQuestActive(questID) && 
               !QuestManager.Instance.IsQuestCompleted(questID);
    }
    
    private void OfferAvailableQuests()
    {
        // 解除事件订阅，使用正确的事件名称
        DialogueManager.Instance.OnDialogueEnd -= OfferAvailableQuests;
        
        // 提供可用任务
        foreach (string questID in npcData.availableQuestIDs)
        {
            if (CanOfferQuest(questID))
            {
                QuestManager.Instance.StartQuest(questID);
                break; // 一次只提供一个任务
            }
        }
    }
    
    private void OnDrawGizmosSelected()
    {
        // 编辑器中的可视化辅助，用于显示交互范围
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionDistance);
    }
}