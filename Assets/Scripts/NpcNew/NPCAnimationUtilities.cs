using UnityEngine;
using System.Collections.Generic;

namespace NpcNew
{
    /// <summary>
    /// NPC动画工具类
    /// </summary>
    public static class NPCAnimationUtilities
    {
        /// <summary>
        /// 获取动画长度
        /// </summary>
        public static float GetAnimationLength(Animator animator, string animationName)
        {
            if (animator == null || animator.runtimeAnimatorController == null) return 0f;

            foreach (AnimationClip clip in animator.runtimeAnimatorController.animationClips)
            {
                if (clip.name == animationName)
                {
                    return clip.length;
                }
            }
            return 0f;
        }

        /// <summary>
        /// 检查动画是否存在
        /// </summary>
        public static bool HasAnimation(Animator animator, string animationName)
        {
            if (animator == null || animator.runtimeAnimatorController == null) return false;

            foreach (AnimationClip clip in animator.runtimeAnimatorController.animationClips)
            {
                if (clip.name == animationName)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 检查参数是否存在
        /// </summary>
        public static bool HasParameter(Animator animator, string parameterName)
        {
            if (animator == null) return false;

            foreach (AnimatorControllerParameter param in animator.parameters)
            {
                if (param.name == parameterName)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 获取当前动画状态信息
        /// </summary>
        public static AnimatorStateInfo GetCurrentStateInfo(Animator animator, int layer = 0)
        {
            return animator?.GetCurrentAnimatorStateInfo(layer) ?? default;
        }

        /// <summary>
        /// 检查动画是否正在播放
        /// </summary>
        public static bool IsAnimationPlaying(Animator animator, string animationName, int layer = 0)
        {
            if (animator == null) return false;

            var stateInfo = animator.GetCurrentAnimatorStateInfo(layer);
            return stateInfo.IsName(animationName);
        }

        /// <summary>
        /// 等待动画完成
        /// </summary>
        public static bool IsAnimationComplete(Animator animator, int layer = 0)
        {
            if (animator == null) return true;

            var stateInfo = animator.GetCurrentAnimatorStateInfo(layer);
            return stateInfo.normalizedTime >= 1.0f && !animator.IsInTransition(layer);
        }
    }

    /// <summary>
    /// NPC动画调试器
    /// </summary>
    [System.Serializable]
    public class NPCAnimationDebugger
    {
        [Header("调试信息")]
        public bool enableDebug = false;
        public string currentAnimation;
        public NPCAnimationState currentState;
        public bool isLocked;
        public float lockTimer;
        
        public void UpdateDebugInfo(NPCAnimationSystem animSystem)
        {
            if (!enableDebug || animSystem == null) return;

            currentState = animSystem.CurrentAnimationState;
            isLocked = animSystem.IsAnimationLocked;
            
            var animator = animSystem.GetAnimator();
            if (animator != null)
            {
                var stateInfo = animator.GetCurrentAnimatorStateInfo(0);
                currentAnimation = GetStateName(stateInfo);
            }
        }

        private string GetStateName(AnimatorStateInfo stateInfo)
        {
            // 这里可以根据实际需要解析状态名称
            return stateInfo.fullPathHash.ToString();
        }
    }
}