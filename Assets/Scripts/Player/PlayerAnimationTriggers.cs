using System.Collections;
using System.Collections.Generic;
using Manager;
using UnityEngine;

public class PlayerAnimationTriggers : MonoBehaviour
{
    private Player player => GetComponentInParent<Player>();

    private void AnimationTrigger()
    {
        player.AnimationTrigger();
    }

    private void AttackTrigger()
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(player.attackCheck.position, player.attackCheckRadius, player.whatIsEnemy);
        foreach (Collider2D collider in colliders)
        {
            if (collider.TryGetComponent<Enemy>(out Enemy enemy))
            {
                enemy.Damage(player.playerData.attackDamage);
            }
        }
    }
    
    private void DefendTrigger()
    {
        
    }

    private void ActivateNpc(string npcID)
    {
        NPCManager.Instance.GetNpc(npcID).ActivateNpc();
    }
    
    private void StartQuest(string questID)
    {
        QuestManager.Instance.StartQuest(questID);
    }
}
