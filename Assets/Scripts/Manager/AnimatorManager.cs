using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Manager
{
    public class AnimatorManager: MonoBehaviour
    {
        public static AnimatorManager Instance { get; private set; } // 单例实例
        
        [SerializeField] 
        private List<GameObject> animators;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this; // 设置单例实例
            }
            else
            {
                Destroy(gameObject); // 如果实例已存在，则销毁当前对象
                return;
            }
        }
        
        private void Start()
        {
            InitializeAnimators(); // 初始化
            
            DialogueManager.Instance.OnDialogueEnd += CheckDialogueID; // 订阅对话结束事件
        }
        
        private void OnDestroy()
        {
            DialogueManager.Instance.OnDialogueEnd -= CheckDialogueID; // 取消订阅对话结束事件
        }

        private void CheckDialogueID(string dialogueID)
        {
            if (dialogueID == "silence_dialogue")
            {
                // 从物体列表中找到名字为OpenDoorAnimation的物体
                GameObject openDoorAnimator = animators.FirstOrDefault(animator => animator.name == "OpenDoorAnimation");
                if (openDoorAnimator) openDoorAnimator.SetActive(true); // 激活OpenDoorAnimation物体
            }
        }

        private void InitializeAnimators()
        {
            // 获取子物体中所有Animator组件
            animators = GetComponentsInChildren<Animator>(true)
                .Select(animator => animator.gameObject)
                .ToList(); // 将Animator组件的GameObject存储到列表中
        }
    }
}