using UnityEngine;

public class NPCAnimationTriggers:MonoBehaviour
{
    private NPC npc => GetComponentInParent<NPC>();
    
    private void AnimationTrigger()
    {
        npc.AnimationTrigger();
    }

    private void AnxiousOver()
    {
        LuXinsheng luXinsheng = npc as LuXinsheng;
        if (luXinsheng)
        {
            luXinsheng.stateMachine.ChangeState(luXinsheng.IdleState);
            EnemyManager.Instance.ActivateEnemy(EnemyType.Enemy1);
        }
    }
        
}