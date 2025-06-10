using UnityEngine;
using System.Collections.Generic;

namespace NpcNew
{
    /// <summary>
    /// NPC动画配置
    /// </summary>
    [CreateAssetMenu(fileName = "NPCAnimationConfig", menuName = "NPC/Animation Config")]
    public class NPCAnimationConfig : ScriptableObject
    {
        [Header("基本动画参数")]
        public string idleParameterName = "Idle";
        public string walkParameterName = "Move";
        public string interactParameterName = "Interact";
        public string anxiousParameterName = "Anxious";
        
        [Header("特殊动画设置")]
        public float anxiousDuration = 2f;
        public float interactionDuration = 1f;
        
        [Header("动画参数映射")]
        public List<AnimationParameterMapping> parameterMappings = new List<AnimationParameterMapping>();
        
        [Header("动画事件配置")]
        public List<AnimationEventConfig> animationEvents = new List<AnimationEventConfig>();

        public void InitializeDefaults()
        {
            // 设置默认参数映射
            parameterMappings.Clear();
            parameterMappings.Add(new AnimationParameterMapping { state = NPCAnimationState.Idle, parameterName = "Idle" });
            parameterMappings.Add(new AnimationParameterMapping { state = NPCAnimationState.Walk, parameterName = "Move" });
            parameterMappings.Add(new AnimationParameterMapping { state = NPCAnimationState.Interact, parameterName = "Interact" });
            parameterMappings.Add(new AnimationParameterMapping { state = NPCAnimationState.Anxious, parameterName = "Anxious" });
        }

        public string GetAnimationParameterName(NPCAnimationState state)
        {
            var mapping = parameterMappings.Find(m => m.state == state);
            return mapping?.parameterName ?? state.ToString();
        }
    }

    /// <summary>
    /// 动画参数映射
    /// </summary>
    [System.Serializable]
    public class AnimationParameterMapping
    {
        public NPCAnimationState state;
        public string parameterName;
    }

    /// <summary>
    /// 动画事件配置
    /// </summary>
    [System.Serializable]
    public class AnimationEventConfig
    {
        public string eventName;
        public string description;
        public float triggerTime;
    }

    /// <summary>
    /// NPC动画状态枚举
    /// </summary>
    public enum NPCAnimationState
    {
        None,
        Idle,       // 空闲
        Walk,       // 行走
        Run,        // 跑步
        Interact,   // 交互
        Anxious,    // 焦虑
        Sleep,      // 睡觉
        Custom      // 自定义
    }
}