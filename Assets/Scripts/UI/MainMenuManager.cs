using System;
using Save;
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
            InitButtonAnimatior();
            
            // 设置按钮的点击事件
            startButton.onClick.AddListener(OnLoadButtonClicked);
            continueButton.onClick.AddListener(OncontinueButtonClicked);
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

        private async void OncontinueButtonClicked()
        {
            // 从存档中获取更新时间最近的游戏数据
            SaveDataInfo[] dataList =await AsyncSaveLoadSystem.GetSaveDataInfosAsync();
            SaveDataInfo saveDataInfo = dataList[0];
            // 从存档中加载SaveDataInfo.saveDate最近的存档
            foreach (var saveData in dataList)
            {
                if (saveData == null) continue; // 跳过空的存档数据
                // 如果当前存档的日期比已记录的日期新，则更新
                if (saveData.saveDate > saveDataInfo.saveDate)
                {
                    saveDataInfo = saveData;
                }
            }
            // 如果没有找到存档，则提示用户
            if (saveDataInfo == null)
            {
                Debug.LogWarning("没有找到可用的存档。");
                return;
            }
            // 加载存档
            await AsyncSaveLoadSystem.LoadGameAsync(saveDataInfo.slotIndex);
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
    }
}