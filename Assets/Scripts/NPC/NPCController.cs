using UnityEngine;

public class NPCController : MonoBehaviour
{
    [Header("Dialogue Data")]
    [SerializeField] private DialogueData dialogue;
    [SerializeField] private float interactionDistance = 2f;
    
    private bool canInteract = false;
    
    private void Update()
    {
        // 检查玩家是否在交互距离内
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            float distance = Vector2.Distance(transform.position, player.transform.position);
            canInteract = distance <= interactionDistance;
            
            // 显示交互提示，如果玩家足够接近
            if (canInteract && Input.GetKeyDown(KeyCode.E))
            {
                TriggerDialogue();
            }
        }
    }
    
    /// <summary>
    /// 触发对话
    /// </summary>
    private void TriggerDialogue()
    {
        DialogueManager.Instance.StartDialogue(dialogue);
    }
    
    private void OnDrawGizmosSelected()
    {
        // 编辑器中的可视化辅助，用于显示交互范围
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionDistance);
    }
}