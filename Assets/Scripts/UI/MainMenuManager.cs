using System;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class MainMenuManager:MonoBehaviour
    {
        public static MainMenuManager Instance;
        
        public Image titleImage; // 主菜单标题图片
        public Button startButton; // 开始游戏按钮
        public Button continueButton; // 继续游戏按钮
        public Button loadButton; // 加载存档按钮
        public Button settingButton; // 设置按钮
        public Button exitButton; // 退出游戏按钮
        
        private int enableAnimatorCount = 0; // 用于计数启用的Animator数量

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this; // 设置单例实例
            }
            else
            {
                Destroy(gameObject); // 如果实例已存在，则销毁当前对象
            }
        }

        private void Start()
        {
            // 设置各个组件的Animator不启用
            titleImage.gameObject.GetComponent<Animator>().enabled = false;
            startButton.gameObject.GetComponent<Animator>().enabled = false;
            continueButton.gameObject.GetComponent<Animator>().enabled = false;
            loadButton.gameObject.GetComponent<Animator>().enabled = false;
            settingButton.gameObject.GetComponent<Animator>().enabled = false;
            exitButton.gameObject.GetComponent<Animator>().enabled = false;
            
            // 设置按钮的点击事件
            startButton.onClick.AddListener(OnStartButtonClicked);
            continueButton.onClick.AddListener(OnStartButtonClicked);
            loadButton.onClick.AddListener(OnLoadButtonClicked);
            settingButton.onClick.AddListener(OnSettingButtonClicked);
            exitButton.onClick.AddListener(OnExitButtonClicked);
        }

        
        private void OnStartButtonClicked()
        {
            // titleImage.gameObject.GetComponent<Animator>().enabled = true;
            EnterGame();
        }

        private void OnLoadButtonClicked()
        {
            MenuManager.Instance.OpenSavePanel();
        }

        private void OnSettingButtonClicked()
        {
            MenuManager.Instance.OpenSettings();
        }

        private void OnExitButtonClicked()
        {
            MenuManager.Instance.QuitToDesktop();
        }

        public void EnableAnimator()
        {
            enableAnimatorCount += 1;
            switch (enableAnimatorCount)
            {
                case 1:
                    startButton.gameObject.GetComponent<Animator>().enabled = true;
                    break;
                case 2:
                    continueButton.gameObject.GetComponent<Animator>().enabled = true;
                    break;
                case 3:
                    loadButton.gameObject.GetComponent<Animator>().enabled = true;
                    break;
                case 4:
                    settingButton.gameObject.GetComponent<Animator>().enabled = true;
                    break;
                case 5:
                    exitButton.gameObject.GetComponent<Animator>().enabled = true;
                    break;
            }
        }

        public void EnterGame()
        {
            MenuManager.Instance.StartNewGame();
        }
    }
}