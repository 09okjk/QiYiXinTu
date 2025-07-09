using System.Collections;
using System.Collections.Generic;
using Audio;
using Manager;
using UnityEngine;

public class PlayerAnimationTriggers : MonoBehaviour
{
    private Player player => GetComponentInParent<Player>();

    private void AnimationTrigger()
    {
        player.AnimationTrigger();
    }

    private void AttackTrigger(string param)
    {
        var arr = param.Split(',');
        if (arr.Length < 1 || arr.Length > 3)
        {
            Debug.LogError("Invalid parameters for AttackTrigger. Expected 1 to 3 parameters.");
            return;
        }
        AudioManager.Instance.PlayEffectAudio("attack_audio",false);
        int _attackType = int.Parse(arr[0]);
        AttackType attackType = (AttackType)_attackType;
        if (arr.Length == 3)
        {
            float direction_x = float.Parse(arr[1]);
            float direction_y = float.Parse(arr[2]);
            Vector2 direction = new Vector2(direction_x, direction_y);
            player.attackCheckerManager.EnableAttackCollider(attackType,direction);
            return;
        }
        player.attackCheckerManager.EnableAttackCollider(attackType);
    }
    
    private void DefendTrigger()
    {
        
    }
    
    private void StartQuest(string questID)
    {
        QuestManager.Instance.StartQuest(questID);
    }
}
