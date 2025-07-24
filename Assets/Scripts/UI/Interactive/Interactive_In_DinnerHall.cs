using System;
using UI.Puzzle;
using Unity.VisualScripting;
using UnityEngine;

namespace UI
{
    public class Interactive_In_DinnerHall: InteractiveUI
    {
        public GameObject nextLevelGameObject; // 下一个关卡的GameObject
        bool showNextLevel = false;
        
        protected override void Start()
        {
            base.Start();
            nextLevelGameObject.SetActive(false);
            isActive = true;
            showNextLevel = false;
            gameObject.SetActive(true); // 初始显示交互按钮
        }
        protected override void Update()
        {
            base.Update();

            if (GameStateManager.Instance.GetFlag("canEnter_" + "In_LiDe_4"))
            {
                gameObject.SetActive(true);
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
                if(GameStateManager.Instance.GetFlag("puzzle2Completed"))
                    nextLevelGameObject.SetActive(true);
                else
                    PuzzleManager.Instance.OpenPanel();
            }
        }
    }
}