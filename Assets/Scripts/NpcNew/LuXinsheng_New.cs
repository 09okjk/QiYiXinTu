using System;

namespace NpcNew
{
    public class LuXinsheng_New : NPCCore
    {
        protected override void Start()
        {
            base.Start();
        
            // 订阅动画事件
            var animSystem = GetAnimationSystem();
            if (animSystem != null)
            {
                animSystem.OnAnimationEvent += OnLuXinshengAnimationEvent;
            }
        }

        protected override void OnDestroy()
        {
            var animSystem = GetAnimationSystem();
            if (animSystem != null)
            {
                animSystem.OnAnimationEvent -= OnLuXinshengAnimationEvent;
            }
        
            base.OnDestroy();
        }

        private void OnLuXinshengAnimationEvent(string eventName)
        {
            switch (eventName)
            {
                case "AnxiousEnd":
                    OnAnxiousAnimationEnd();
                    break;
            }
        }

        private void OnAnxiousAnimationEnd()
        {
            // 切换到空闲状态
            ChangeState(NPCStateType.Idle);
        
            // 激活敌人
            EnemyManager.Instance?.ActivateEnemy(EnemyType.Enemy1);
        }

        // 重写焦虑方法，包含动画控制
        public void Anxious()
        {
            try
            {
                NPCLogger.Log("LuXinsheng 进入焦虑状态", this);
            
                // 设置状态
                ChangeState(NPCStateType.Anxious);
            
                // 设置动画
                SetAnimationState(NPCAnimationState.Anxious);
            
                // 锁定动画
                LockAnimation(2f);
            }
            catch (Exception e)
            {
                NPCLogger.LogError($"切换到焦虑状态失败: {e.Message}", this);
            }
        }
    }
}
