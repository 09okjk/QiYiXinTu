using UnityEngine;

namespace NpcNew
{
    /// <summary>
    /// 增强版NPC动画触发器
    /// </summary>
    public class NPCAnimationTriggersNew : MonoBehaviour
    {
        private NPCCore npcCore;
        private NPCAnimationSystem animationSystem;

        private void Awake()
        {
            npcCore = GetComponentInParent<NPCCore>();
            if (npcCore != null)
            {
                animationSystem = npcCore.GetAnimationSystem();
            }
        }

        #region 动画事件回调
        
        /// <summary>
        /// 通用动画事件触发器
        /// </summary>
        /// <param name="eventName">事件名称</param>
        public void OnAnimationEvent(string eventName)
        {
            animationSystem?.OnAnimationEventTriggered(eventName);
            NPCLogger.Log($"动画事件触发: {eventName}", npcCore);
        }

        /// <summary>
        /// 动画完成触发器
        /// </summary>
        public void OnAnimationFinished()
        {
            OnAnimationEvent("AnimationFinished");
        }

        /// <summary>
        /// 焦虑动画结束
        /// </summary>
        public void OnAnxiousEnd()
        {
            OnAnimationEvent("AnxiousEnd");
            
            // 特殊处理逻辑（保持与原代码兼容）
            if (npcCore is LuXinsheng_New luXinsheng)
            {
                HandleLuXinshengAnxiousEnd(luXinsheng);
            }
        }

        /// <summary>
        /// 交互动画完成
        /// </summary>
        public void OnInteractionComplete()
        {
            OnAnimationEvent("InteractionComplete");
        }

        /// <summary>
        /// 移动开始
        /// </summary>
        public void OnMoveStart()
        {
            OnAnimationEvent("MoveStart");
        }

        /// <summary>
        /// 移动结束
        /// </summary>
        public void OnMoveEnd()
        {
            OnAnimationEvent("MoveEnd");
        }

        /// <summary>
        /// 睡觉动画完成
        /// </summary>
        public void OnSleepComplete()
        {
            OnAnimationEvent("SleepComplete");
        }

        /// <summary>
        /// 自定义动画事件
        /// </summary>
        /// <param name="customEventName">自定义事件名</param>
        public void OnCustomEvent(string customEventName)
        {
            OnAnimationEvent($"Custom_{customEventName}");
        }

        #endregion

        #region 特殊处理逻辑

        private void HandleLuXinshengAnxiousEnd(LuXinsheng_New luXinsheng)
        {
            try
            {
                // 切换到空闲状态
                luXinsheng.ChangeState(NPCStateType.Idle);
                
                // 激活敌人
                if (EnemyManager.Instance != null)
                {
                    EnemyManager.Instance.ActivateEnemy(EnemyType.Enemy1);
                }
                
                NPCLogger.Log("LuXinsheng焦虑动画结束，激活Enemy1", npcCore);
            }
            catch (System.Exception e)
            {
                NPCLogger.LogError($"处理LuXinsheng焦虑结束失败: {e.Message}", npcCore);
            }
        }

        #endregion

        #region 调试支持

        [ContextMenu("测试动画事件")]
        private void TestAnimationEvent()
        {
            OnAnimationEvent("TestEvent");
        }

        [ContextMenu("触发焦虑结束")]
        private void TestAnxiousEnd()
        {
            OnAnxiousEnd();
        }

        #endregion
    }
}