using System;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class Interactive_Animator:MonoBehaviour
    {
        public static Interactive_Animator Instance { get; private set; } // 单例实例
        public Image interactImage; // 交互按钮
        public Animator animator;
        
        protected void Awake()
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
            animator.enabled = false; // 禁用动画器
        }
        protected void Start()
        {
            interactImage.gameObject.SetActive(false); // 初始隐藏交互按钮
            
        }

        private void OnEnable()
        {
            DialogueManager.Instance.OnDialogueEnd += ActivateInteractImage;
        }
        private void OnDisable()
        {
            DialogueManager.Instance.OnDialogueEnd -= ActivateInteractImage;
        }

        protected void Update()
        {
            // 检测按键输入
            // if (Input.GetKeyDown(KeyCode.S) && (Input.GetKeyDown(KeyCode.LeftControl) || Input.GetKeyDown(KeyCode.RightControl)) && interactImage.gameObject.activeSelf )
            // {
            //     animator.enabled = true; // 启用动画器
            // }  
            if (Input.GetKeyDown(KeyCode.W) && interactImage.gameObject.activeSelf)
            {
                animator.gameObject.SetActive(true); // 启用动画器
                animator.enabled = true; // 启用动画器
                interactImage.gameObject.SetActive(false); // 隐藏交互按钮
            }
        }
        
        private void ActivateInteractImage(string dialogueID)
        {
            if (dialogueID == "homework")
            {
                interactImage.gameObject.SetActive(true);
            }
            if (dialogueID == "homework_over")
            {
                gameObject.SetActive(false);
            }
        }
        
        
    }
}