using System;
using System.Collections.Generic;
using Manager;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace News
{
    public class NewsManager:MonoBehaviour
    {
        public static NewsManager Instance; // 单例实例
        
        public GameObject newsBasePanel; // 新闻基础面板
        
        [Header("单个新闻信息UI")]
        public GameObject newsInfoUI; // 单个新闻信息UI
        public TextMeshProUGUI newsTitleText; // 新闻标题文本
        public TextMeshProUGUI newsContentText; // 新闻内容文本
        public Image newsImage; // 新闻图片
        public Button closeButton; // 关闭按钮
        
        [Header("新闻列表UI")]
        public GameObject newsInfoBookPanel; //Ruc一日新闻素材库面板
        public ScrollRect newsInfoScrollRect; // 新闻列表滚动视图
        public GameObject newsInfoSlotPrefab; // 新闻列表预制体
        public GameObject newsInfoPanel; // 新闻信息面板
        public TextMeshProUGUI newsInfoTitleText; // 新闻信息标题文本
        public TextMeshProUGUI newsInfoContentText; // 新闻信息内容文本
        public Image newsInfoImage; // 新闻信息图片
        public Button newsInfoCloseButton; // 新闻信息关闭按钮
        
        private NewsData[] newsDataArray; // 存储所有新闻数据的数组
        public List<NewsData> checkedNewsDataArray; // 存储已读新闻数据的数组
        private NewsData currentNewsData; // 当前新闻数据
        private List<GameObject> newsInfoSlotPool = new List<GameObject>(); // 新闻列表预制体池
        private Dictionary<string, NewsData> newsDataDict; // 添加字典用于快速查找

        public event Action<bool> OnNewsBookStateChanged; // 新闻信息状态改变事件
        
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this; // 设置单例实例
                InitializeNewsData(); // 提取初始化逻辑
            }
            else
            {
                Destroy(gameObject); // 如果实例已存在，则销毁当前对象
                return;
            }
        }
        
        private void InitializeNewsData()
        {
            // 可以考虑异步加载或延迟加载
            newsDataArray = Resources.LoadAll<NewsData>("ScriptableObjects/News");
    
            // 创建字典用于快速查找
            newsDataDict = new Dictionary<string, NewsData>();
            foreach (var newsData in newsDataArray)
            {
                newsDataDict[newsData.newsID] = newsData;
            }
    
            checkedNewsDataArray = new List<NewsData>();
        }
        
        // private async void InitializeNewsDataAsync()
        // {
        //     await Task.Run(() =>
        //     {
        //         // 在后台线程加载数据
        //         var loadedData = Resources.LoadAll<NewsData>("ScriptableObjects/News");
        //
        //         // 切换回主线程更新UI
        //         UnityMainThreadDispatcher.Instance().Enqueue(() =>
        //         {
        //             newsDataArray = loadedData;
        //             CreateNewsDataDict();
        //         });
        //     });
        // }
        //
        // private void CreateNewsDataDict()
        // {
        //     newsDataDict = new Dictionary<string, NewsData>();
        //     foreach (var newsData in newsDataArray)
        //     {
        //         newsDataDict[newsData.newsID] = newsData;
        //     }
        // }
        
        private void Start()
        {
            newsInfoUI.SetActive(false); // 隐藏新闻信息UI
            newsInfoBookPanel.SetActive(false); // 隐藏新闻列表UI
            newsInfoPanel.SetActive(false); // 隐藏新闻信息面板UI

            foreach (NewsData data in newsDataArray)
            {
                if (data.isRead)
                {
                    checkedNewsDataArray.Add(data); // 将已读新闻添加到列表
                }
            }
            closeButton.onClick.AddListener(CloseNewsInfo); // 绑定关闭按钮事件
            newsInfoCloseButton.onClick.AddListener(ToggleNewsInfoBook); // 绑定新闻信息面板关闭按钮事件
        }

        private void OnEnable()
        {
            OnNewsBookStateChanged += OnNewsBookStateChangedHandler; // 订阅新闻信息状态改变事件
        }

        private void OnDisable()
        {
            OnNewsBookStateChanged -= OnNewsBookStateChangedHandler; // 取消订阅新闻信息状态改变事件
        }

        private void OnNewsBookStateChangedHandler(bool isOpen)
        {
            PlayerManager.Instance.player.HandleNewsBookStateChanged(isOpen);
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape) )
            {
                if (newsInfoBookPanel.activeSelf)
                {
                    ToggleNewsInfoBook(); // 切换新闻列表UI
                }
            }
        }

        public void ApplyNewsDatas(Dictionary<string,bool> newsDatas)
        {
            // 保存新闻数据到持久化存储
            foreach (var newsData in newsDatas)
            {
                NewsData data = System.Array.Find(newsDataArray, n => n.newsID == newsData.Key);
                if (data != null)
                {
                    data.isRead = newsData.Value; // 更新新闻的已读状态
                }
            }
        }
        
        public Dictionary<string, bool> GetNewsDatas()
        {
            // 获取新闻数据的字典形式
            Dictionary<string, bool> newsDataDict = new Dictionary<string, bool>();
            foreach (var newsData in newsDataArray)
            {
                newsDataDict[newsData.newsID] = newsData.isRead; // 将新闻ID和已读状态添加到字典
            }
            return newsDataDict;
        }
        
        public NewsData GetNewsByID(string newsID)
        {
            // 根据新闻ID获取新闻数据
            if (newsDataDict.TryGetValue(newsID, out NewsData newsData))
            {
                return newsData; // 返回找到的新闻数据
            }
            else
            {
                Debug.LogError($"找不到ID为 {newsID} 的新闻数据");
                return null; // 如果没有找到，返回null
            }
        }
        
        # region 显示单个新闻
        public void OpenNewsInfo(NewsData newsData)
        {
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            newsBasePanel.SetActive(true);
    
            if (newsData.isRead)
                return;
        
            currentNewsData = newsData;
            newsTitleText.text = newsData.newsTitle;
            newsContentText.text = newsData.newsContent;
            newsImage.sprite = newsData.newsImage;
            newsInfoUI.SetActive(true);
            OnNewsBookStateChanged?.Invoke(true);
            
            stopwatch.Stop();
            if (stopwatch.ElapsedMilliseconds > 100) // 如果超过100ms就警告
            {
                Debug.LogWarning($"OpenNewsInfo took {stopwatch.ElapsedMilliseconds}ms");
            }
        }
        
        private void CloseNewsInfo()
        {
            newsInfoUI.SetActive(false); // 隐藏新闻信息UI
            newsBasePanel.SetActive(false); // 隐藏新闻基础面板
            currentNewsData.isRead = true; // 设置当前新闻为已读
            checkedNewsDataArray.Add(currentNewsData); // 将当前新闻添加到已读列表
            newsDataDict[currentNewsData.newsID] = currentNewsData; // 更新字典中的新闻数据
            OnNewsBookStateChanged?.Invoke(false); // 触发新闻信息状态改变事件
        }
        # endregion
        
        # region 显示新闻列表

        public void ToggleNewsInfoBook()
        {
            Debug.Log("ToggleNewsInfoBook");
            newsBasePanel.SetActive(!newsBasePanel.activeSelf); // 切换新闻基础面板的显示状态
            newsInfoBookPanel.SetActive(!newsInfoBookPanel.activeSelf); // 切换新闻列表UI的显示状态
            OnNewsBookStateChanged?.Invoke(newsInfoBookPanel.activeSelf); // 触发新闻信息状态改变事件
            if (newsInfoBookPanel.activeSelf)
            {
                newsInfoPanel.SetActive(false); // 隐藏新闻信息面板UI
                ShowNewsInfoSlotList(); // 显示新闻列表
            }
        }

        // 显示新闻列表
        private void ShowNewsInfoSlotList()
        {
            // 首先隐藏所有池中的对象
            foreach (var obj in newsInfoSlotPool)
            {
                obj.SetActive(false);
            }

            int slotIndex = 0;
            foreach (var newsData in checkedNewsDataArray)
            {
                GameObject newsInfoSlot;
        
                // 如果池中有可用对象，复用它
                if (slotIndex < newsInfoSlotPool.Count)
                {
                    newsInfoSlot = newsInfoSlotPool[slotIndex];
                }
                else
                {
                    // 池中没有足够对象时才创建新的
                    newsInfoSlot = Instantiate(newsInfoSlotPrefab, newsInfoScrollRect.content);
                    newsInfoSlotPool.Add(newsInfoSlot);
                }
        
                // 激活并设置新闻信息
                newsInfoSlot.SetActive(true);
                NewsInfoSlot newsSlot = newsInfoSlot.GetComponent<NewsInfoSlot>();
                newsSlot.ShowNewsInfo(newsData);
        
                slotIndex++;
            }
        }

        // 显示选中新闻的信息
        public void ShowNewsInfoPanel(NewsData newsData)
        {
            // 更新新闻信息UI
            newsInfoTitleText.text = newsData.newsTitle;
            newsInfoContentText.text = newsData.newsContent;
                    
            // 加载新闻图片
            newsInfoImage.sprite = newsData.newsImage;
            
            // 显示新闻信息面板
            newsInfoPanel.SetActive(true);
        }

        # endregion
        
    }
}
