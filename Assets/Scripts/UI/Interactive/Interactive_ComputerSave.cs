using System;
using UnityEngine;

namespace UI
{
    public class Interactive_ComputerSave:InteractiveUI
    {
        private Animator animator;
        protected override void Start()
        {
            base.Start();
            
            DialogueManager.Instance.OnDialogueEnd += ActivateInteractImage;
            animator = GetComponentInChildren<Animator>();
            animator.enabled = false; // 禁用动画器
        }

        protected override void Update()
        {
            base.Update();
            
            // 检测按键输入
            if (Input.GetKeyDown(KeyCode.S) && (Input.GetKeyDown(KeyCode.LeftControl) || Input.GetKeyDown(KeyCode.RightControl)) && interactImage.gameObject.activeSelf )
            {
                // 处理交互逻辑
                OnInteractButtonClicked();
            }
        }

        private void OnDisable()
        {
            DialogueManager.Instance.OnDialogueEnd -= ActivateInteractImage;
        }

        private void ActivateInteractImage(string dialogueID)
        {
            if (dialogueID == "homework")
            {
                interactImage.gameObject.SetActive(true);
            }
        }
        
        
    }
}