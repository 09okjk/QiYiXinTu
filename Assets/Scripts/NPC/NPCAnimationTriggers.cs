using UnityEngine;

public class NPCAnimationTriggers:MonoBehaviour
{
    private NPC npc => GetComponentInParent<NPC>();
    
    private void AnimationTrigger()
    {
        npc.AnimationTrigger();
    }
    
    private void WeekUpTrigger()
    {
        LuXinsheng luXinsheng = npc as LuXinsheng;
        if (luXinsheng)
        {
            luXinsheng.stateMachine.ChangeState(luXinsheng.IdleState);
            DialogueManager.Instance.StartDialogueByID("lu_first_dialogue");
        }
        
    }
        
}