using System.Collections;
using UnityEngine;

namespace UI
{
    public class AnimatorStateDiagnostic : MonoBehaviour
    {
        [Header("要诊断的动画器")]
        public Animator targetAnimator;
        
        [Header("诊断设置")]
        public bool continuousMonitoring = true;
        public float monitorInterval = 0.1f;
        
        private void Start()
        {
            if (targetAnimator == null)
                targetAnimator = GetComponent<Animator>();
                
            if (continuousMonitoring)
                StartCoroutine(ContinuousMonitoring());
        }
        
        private IEnumerator ContinuousMonitoring()
        {
            while (true)
            {
                if (targetAnimator != null && targetAnimator.gameObject.activeInHierarchy)
                {
                    CheckAnimatorState();
                }
                yield return new WaitForSeconds(monitorInterval);
            }
        }
        
        private void CheckAnimatorState()
        {
            AnimatorStateInfo currentState = targetAnimator.GetCurrentAnimatorStateInfo(0);
            AnimatorStateInfo nextState = targetAnimator.GetNextAnimatorStateInfo(0);
            
            // 检查是否在转换中
            bool isInTransition = targetAnimator.IsInTransition(0);
            
            Debug.Log($"=== 动画器状态诊断 ({targetAnimator.name}) ===");
            Debug.Log($"当前状态哈希: {currentState.fullPathHash}");
            Debug.Log($"当前状态长度: {currentState.length}");
            Debug.Log($"当前播放进度: {currentState.normalizedTime}");
            Debug.Log($"当前播放速度: {currentState.speed}");
            Debug.Log($"是否在转换中: {isInTransition}");
            
            if (isInTransition)
            {
                AnimatorTransitionInfo transitionInfo = targetAnimator.GetAnimatorTransitionInfo(0);
                Debug.Log($"转换进度: {transitionInfo.normalizedTime}");
                Debug.Log($"转换持续时间: {transitionInfo.duration}");
                Debug.Log($"下一个状态哈希: {nextState.fullPathHash}");
            }
            
            // 检查参数
            foreach (var parameter in targetAnimator.parameters)
            {
                if (parameter.type == AnimatorControllerParameterType.Int)
                {
                    Debug.Log($"参数 {parameter.name}: {targetAnimator.GetInteger(parameter.name)}");
                }
                else if (parameter.type == AnimatorControllerParameterType.Bool)
                {
                    Debug.Log($"参数 {parameter.name}: {targetAnimator.GetBool(parameter.name)}");
                }
                else if (parameter.type == AnimatorControllerParameterType.Float)
                {
                    Debug.Log($"参数 {parameter.name}: {targetAnimator.GetFloat(parameter.name)}");
                }
            }
            
            // 检查是否卡住
            if (currentState.normalizedTime == 0 && currentState.length > 0)
            {
                Debug.LogWarning($"⚠️ 动画器 {targetAnimator.name} 可能卡在第一帧！");
                DiagnoseStuckAnimation();
            }
            
            Debug.Log("=== 诊断结束 ===\n");
        }
        
        private void DiagnoseStuckAnimation()
        {
            Debug.Log("🔍 开始诊断卡住的动画...");
            
            // 检查动画器组件状态
            Debug.Log($"动画器启用状态: {targetAnimator.enabled}");
            Debug.Log($"GameObject激活状态: {targetAnimator.gameObject.activeInHierarchy}");
            Debug.Log($"动画器更新模式: {targetAnimator.updateMode}");
            Debug.Log($"Culling模式: {targetAnimator.cullingMode}");
            
            // 检查时间缩放
            Debug.Log($"Time.timeScale: {Time.timeScale}");
            Debug.Log($"动画器速度: {targetAnimator.speed}");
            
            // 检查Animator Controller
            if (targetAnimator.runtimeAnimatorController == null)
            {
                Debug.LogError("❌ Animator Controller为空！");
                return;
            }
            
            // 尝试强制更新
            Debug.Log("🔄 尝试强制更新动画器...");
            targetAnimator.Update(Time.deltaTime);
            
            // 检查更新后的状态
            AnimatorStateInfo stateAfterUpdate = targetAnimator.GetCurrentAnimatorStateInfo(0);
            Debug.Log($"强制更新后播放进度: {stateAfterUpdate.normalizedTime}");
            
            if (stateAfterUpdate.normalizedTime == 0)
            {
                Debug.LogError("❌ 强制更新后仍然卡在第一帧！可能是Animator Controller配置问题。");
                SuggestSolutions();
            }
        }
        
        private void SuggestSolutions()
        {
            Debug.Log("💡 可能的解决方案:");
            Debug.Log("1. 检查Animator Controller中的状态转换条件");
            Debug.Log("2. 确认目标状态的Motion字段不为空");
            Debug.Log("3. 检查Has Exit Time设置");
            Debug.Log("4. 确认Transition Duration不会阻止动画播放");
            Debug.Log("5. 检查动画剪辑的Loop Time设置");
            Debug.Log("6. 验证从Any State或Entry的转换条件");
        }
        
        // 手动测试方法
        [ContextMenu("立即诊断")]
        public void DiagnoseNow()
        {
            if (targetAnimator != null)
            {
                CheckAnimatorState();
            }
        }
        
        [ContextMenu("测试设置Health=4")]
        public void TestSetHealth4()
        {
            if (targetAnimator != null)
            {
                Debug.Log("🧪 测试设置Health=4");
                targetAnimator.SetInteger("Health", 4);
                StartCoroutine(MonitorAfterParameterChange());
            }
        }
        
        private IEnumerator MonitorAfterParameterChange()
        {
            for (int i = 0; i < 10; i++)
            {
                yield return new WaitForSeconds(0.1f);
                AnimatorStateInfo state = targetAnimator.GetCurrentAnimatorStateInfo(0);
                Debug.Log($"参数设置后 {i * 0.1f}s: 进度={state.normalizedTime}, 哈希={state.fullPathHash}");
            }
        }
    }
}