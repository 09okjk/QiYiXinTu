using System;
using UnityEngine;

namespace UI
{
    public class Interactive_In_LiDe: InteractiveUI
    {
        protected override void Update()
        {
            base.Update();

            if (GameStateManager.Instance.GetFlag("CanEnter_" + "LiDe"))
            {
                gameObject.SetActive(true);
            }
            else
            {
                return;
            }
            
            // 检测按键输入
            if (Input.GetKeyDown(KeyCode.E) && interactImage.gameObject.activeSelf )
            {
                // 处理交互逻辑
                OnInteractButtonClicked();
            }
        }
    }
}