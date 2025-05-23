using UnityEngine;

namespace UI
{
    public class Interactive_General:InteractiveUI
    {
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