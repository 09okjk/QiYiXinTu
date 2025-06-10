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
            // TODO: 合并startButton和continueButton为startButton
            // 当没有存档时，startButton的文本应为“开始游戏”，有存档时应为“继续游戏”

            InitButtonAnimatior();
            
            // 设置按钮的点击事件
            startButton.onClick.AddListener(OnLoadButtonClicked);
            continueButton.onClick.AddListener(OnLoadButtonClicked);
            loadButton.onClick.AddListener(OnLoadButtonClicked);
            settingButton.onClick.AddListener(OnSettingButtonClicked);
            exitButton.onClick.AddListener(OnExitButtonClicked);
        }

        private void InitButtonAnimatior()
        {
            titleImage.gameObject.GetComponent<Animator>().enabled = false; // 禁用标题图片的Animator
            // 需要初始化的按钮和图片
            var uiElements = new GameObject[]
            {
                startButton.gameObject,
                continueButton.gameObject,
                loadButton.gameObject,
                settingButton.gameObject,
                exitButton.gameObject
            };

            foreach (var element in uiElements)
            {
                var image = element.GetComponent<Image>();
                if (image)
                {
                    var color = image.color;
                    color.a = 0.12157f; // 设置图片透明度为0.12157
                    image.color = color;
                }
                var text = element.GetComponentInChildren<TMPro.TMP_Text>();
                if (text != null)
                {
                    var alpha = text.alpha;
                    alpha = 1f; // 设置文本透明度为1
                    text.alpha = alpha;
                }
                var animator = element.GetComponent<Animator>();
                if (animator != null)
                {
                    animator.enabled = false;
                }
            }
        }


        private void OnStartButtonClicked()
        {
            // EnterGame();
        }

        private void OnLoadButtonClicked()
        {
            titleImage.gameObject.GetComponent<Animator>().enabled = true;
            //MenuManager.Instance.OpenSavePanel();
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
            if (enableAnimatorCount >= 5)
            {
                enableAnimatorCount = 0;
                return;
            }
            
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
                default:
                    break;
            }
            Debug.Log($"EnableAnimator计数: {enableAnimatorCount}");

        }

        public void ShowMainMenuUI()
        {
            InitButtonAnimatior();
            gameObject.SetActive(true);
        }
        public void EnterGame()
        {
            // 如果有存档，则继续游戏
            // if ()
            MenuManager.Instance.StartNewGame();
        }
    }
}