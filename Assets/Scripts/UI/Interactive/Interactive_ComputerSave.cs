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
            
            animator = GetComponentInChildren<Animator>();
            animator.enabled = false; // 禁用动画器
        }

        private void OnEnable()
        {
            DialogueManager.Instance.OnDialogueEnd += ActivateInteractImage;
        }
        private void OnDisable()
        {
            DialogueManager.Instance.OnDialogueEnd -= ActivateInteractImage;
        }

        protected override void Update()
        {
            base.Update();
            
            // 检测按键输入
            if (Input.GetKeyDown(KeyCode.S) && (Input.GetKeyDown(KeyCode.LeftControl) || Input.GetKeyDown(KeyCode.RightControl)) && interactImage.gameObject.activeSelf )
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
        }
        
        
    }
}