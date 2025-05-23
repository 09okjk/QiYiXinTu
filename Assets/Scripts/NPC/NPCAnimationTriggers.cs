using UnityEngine;

public class NPCAnimationTriggers:MonoBehaviour
{
    private NPC npc => GetComponentInParent<NPC>();
    
    private void AnimationTrigger()
    {
        npc.AnimationTrigger();
    }
    
    private void TriggerDialogue(string dialogueID)
    {
        Debug.Log($"TriggerDialogue: {dialogueID}");
        DialogueManager.Instance.StartDialogueByID(dialogueID);
    }
        
}