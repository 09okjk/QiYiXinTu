﻿using UnityEngine;

public class SkeletonAnimationTriggers:MonoBehaviour
{
    private Skeleton skeleton => GetComponentInParent<Skeleton>();
    
    private void AnimationTrigger()
    {
        skeleton.AnimationFinishTrigger();
    }
    
    private void AttackTrigger()
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(skeleton.attackCheck.position, skeleton.attackCheckRadius, skeleton.whatIsPlayer);
        foreach (Collider2D collider in colliders)
        {
            if (collider.TryGetComponent<Player>(out Player player))
            {
                player.Damage(skeleton.enemyData.attackDamage);
            }
        }
    }

    private void DeathTrigger()
    {
        skeleton.Die();
    }
    
    private void OpenCounterWindow() => skeleton.OpenCounterAttackWindow();
    private void CloseCounterWindow() => skeleton.CloseCounterAttackWindow();
}