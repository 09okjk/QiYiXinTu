using UnityEngine;

public class SkeletonAnimationTriggers:MonoBehaviour
{
    private Enemy_Skeleton skeleton => GetComponentInParent<Enemy_Skeleton>();
    
    private void AnimationTrigger()
    {
        skeleton.AnimationFinishTrigger();
    }
}