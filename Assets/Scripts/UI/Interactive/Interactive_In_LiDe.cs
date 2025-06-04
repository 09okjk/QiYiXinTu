using UnityEngine;

namespace UI
{
    public class Interactive_In_LiDe: InteractiveUI
    {
        protected override void Start()
        {
            base.Start();
            DialogueManager.Instance.OnDialogueEnd += ActivateInteractImage;
        }
        
        private void OnDisable()
        {
            DialogueManager.Instance.OnDialogueEnd -= ActivateInteractImage;
        }

        private void ActivateInteractImage(string dialogueID)
        {
            // 检查对话ID是否匹配，并设置交互图像的可见性
            interactImage.gameObject.SetActive(dialogueID == "fang_dialogue");
        }

        protected override void Update()
        {
            base.Update();
            
            // 检测按键输入
            if (Input.GetKeyDown(KeyCode.E) && interactImage.gameObject.activeSelf )
            {
                // 处理交互逻辑
                OnInteractButtonClicked();
            }
        }
    }
}