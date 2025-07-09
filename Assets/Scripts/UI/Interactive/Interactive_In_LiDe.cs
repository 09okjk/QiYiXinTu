using System;
using Unity.VisualScripting;
using UnityEngine;

namespace UI
{
    public class Interactive_In_LiDe: InteractiveUI
    {
        public GameObject nextLevelGameObject; // 下一个关卡的GameObject
        bool showNextLevel = false;
        protected override void Awake()
        {
            base.Awake();
            nextLevelGameObject.SetActive(false);
            isActive = true;
            showNextLevel = false;
        }
        
        protected override void Start()
        {
            base.Start();
            gameObject.SetActive(true); // 初始显示交互按钮
        }
        protected override void Update()
        {
            base.Update();

            if (GameStateManager.Instance.GetFlag("CanEnter_" + "In_LiDe"))
            {
                showNextLevel = true;
            }
            else
            {
                interactImage.gameObject.SetActive(false);
                return;
            }
            
            // 检测按键输入
            if (Input.GetKeyDown(KeyCode.E) && interactImage.gameObject.activeSelf && showNextLevel)
            {
                // 处理交互逻辑
                nextLevelGameObject.SetActive(true);
            }
        }
    }
}