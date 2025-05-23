using System;
using UnityEngine;

namespace UI
{
    public class Interactive_ComputerSave:MonoBehaviour
    {
        public SpriteRenderer interactImage; // 交互按钮
        private Animator animator;
        
        protected void Awake()
        {
            animator = GetComponentInChildren<Animator>();
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
            if (Input.GetKeyDown(KeyCode.W))
            {
                animator.enabled = true; // 启用动画器
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