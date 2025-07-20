using System;
using Unity.VisualScripting;
using UnityEngine;

namespace UI
{
    public class Interactive_In_No_1: InteractiveUI
    {
        public GameObject nextLevelGameObject; // 下一个关卡的GameObject
        bool showNextLevel = false;
        
        protected override void Start()
        {
            base.Start();
            nextLevelGameObject.SetActive(false);
            isActive = false;
            showNextLevel = false;
            gameObject.SetActive(true); // 初始显示交互按钮
        }
        protected override void Update()
        {
            base.Update();
    
            showNextLevel = true;
            
            // 检测按键输入
            if (Input.GetKeyDown(KeyCode.E) && interactImage.gameObject.activeSelf && showNextLevel)
            {
                // 处理交互逻辑
                nextLevelGameObject.SetActive(true);
            }
        }
    }
}