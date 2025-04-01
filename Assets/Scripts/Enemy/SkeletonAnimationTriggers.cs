using UnityEngine;

public class SkeletonAnimationTriggers:MonoBehaviour
{
    private Enemy_Skeleton skeleton => GetComponentInParent<Enemy_Skeleton>();
    
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
                player.Damage();
            }
        }
    }
}