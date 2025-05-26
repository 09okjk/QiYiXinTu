using UnityEngine;

public class NPCAnimationTriggers:MonoBehaviour
{
    private NPC npc => GetComponentInParent<NPC>();
    
    private void AnimationTrigger()
    {
        npc.AnimationTrigger();
    }
        
}