using System;
using System.Collections.Generic;
using Manager;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace News
{
    public class NewsManager : MonoBehaviour
    {
        public static NewsManager Instance; // 单例实例
        
        [Header("UI组件")]
        public GameObject newsBasePanel;
        public GameObject newsInfoUI;
        public TextMeshProUGUI newsTitleText;
        public TextMeshProUGUI newsContentText;
        public Image newsImage;
        public Button closeButton;
        
        [Header("新闻列表UI")]
        public GameObject newsInfoBookPanel;
        public ScrollRect newsInfoScrollRect;
        public GameObject newsInfoSlotPrefab;
        public GameObject newsInfoPanel;
        public TextMeshProUGUI newsInfoTitleText;
        public TextMeshProUGUI newsInfoContentText;
        public Image newsInfoImage;
        public Button newsInfoCloseButton;
        
        // 原始新闻数据（只读）
        private NewsData[] originalNewsDataArray;
        // 运行时新闻数据副本
        private Dictionary<string, NewsData> runtimeNewsDataDict = new Dictionary<string, NewsData>();
        private List<NewsData> runtimeNewsDataArray = new List<NewsData>();
        
        public List<NewsData> checkedNewsDataArray = new List<NewsData>();
        private NewsData currentNewsData;
        private List<GameObject> newsInfoSlotPool = new List<GameObject>();

        public event Action<bool> OnNewsBookStateChanged;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                LoadOriginalNewsData();
                CreateRuntimeDataCopies();
            }
            else
            {
                Destroy(gameObject);
                return;
            }
        }
        
        /// <summary>
        /// 加载原始新闻数据（只读）
        /// </summary>
        private void LoadOriginalNewsData()
        {
            try
            {
                originalNewsDataArray = Resources.LoadAll<NewsData>("ScriptableObjects/News");
                Debug.Log($"成功加载 {originalNewsDataArray?.Length ?? 0} 个原始新闻数据");
            }
            catch (Exception e)
            {
                Debug.LogError($"加载原始新闻数据时发生错误: {e.Message}");
            }
        }

        /// <summary>
        /// 创建运行时数据副本
        /// </summary>
        private void CreateRuntimeDataCopies()
        {
            runtimeNewsDataDict.Clear();
            runtimeNewsDataArray.Clear();
    
            if (originalNewsDataArray == null) return;

            foreach (var originalNews in originalNewsDataArray)
            {
                if (originalNews != null && !string.IsNullOrEmpty(originalNews.newsID))
                {
                    // 使用增强后的工具类
                    var runtimeCopy = Utils.ScriptableObjectUtils.CreateNewsDataCopy(originalNews);
                    runtimeNewsDataDict[originalNews.newsID] = runtimeCopy;
                    runtimeNewsDataArray.Add(runtimeCopy);
                }
            }

            checkedNewsDataArray = new List<News.NewsData>();
            Debug.Log($"创建了 {runtimeNewsDataDict.Count} 个新闻运行时数据副本");
        }

        /// <summary>
        /// 重置所有新闻数据到原始状态
        /// </summary>
        public void ResetAllNewsData()
        {
            foreach (var originalNews in originalNewsDataArray)
            {
                if (originalNews != null && runtimeNewsDataDict.ContainsKey(originalNews.newsID))
                {
                    var runtimeNews = runtimeNewsDataDict[originalNews.newsID];
                    ScriptableObjectUtils.ResetToOriginal(originalNews, runtimeNews);
                }
            }
            
            // 重置已读新闻列表
            checkedNewsDataArray.Clear();
            currentNewsData = null;
            
            Debug.Log("已重置所有新闻数据到原始状态");
        }

        /// <summary>
        /// 清理运行时数据
        /// </summary>
        private void CleanupRuntimeData()
        {
            foreach (var runtimeNews in runtimeNewsDataDict.Values)
            {
                if (runtimeNews != null)
                {
                    DestroyImmediate(runtimeNews);
                }
            }
            runtimeNewsDataDict.Clear();
            runtimeNewsDataArray.Clear();
        }

        private void OnDestroy()
        {
            CleanupRuntimeData();
        }
        
        private void Start()
        {
            newsInfoUI.SetActive(false);
            newsInfoBookPanel.SetActive(false);
            newsInfoPanel.SetActive(false);

            // 使用运行时数据副本
            foreach (NewsData data in runtimeNewsDataArray)
            {
                if (data.isRead)
                {
                    checkedNewsDataArray.Add(data);
                }
            }
            
            closeButton.onClick.AddListener(CloseNewsInfo);
            newsInfoCloseButton.onClick.AddListener(ToggleNewsInfoBook);
        }

        private void OnEnable()
        {
            OnNewsBookStateChanged += OnNewsBookStateChangedHandler;
        }

        private void OnDisable()
        {
            OnNewsBookStateChanged -= OnNewsBookStateChangedHandler;
        }

        private void OnNewsBookStateChangedHandler(bool isOpen)
        {
            PlayerManager.Instance.player.HandleNewsBookStateChanged(isOpen);
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (newsInfoBookPanel.activeSelf)
                {
                    ToggleNewsInfoBook();
                }
            }
        }

        public void ApplyNewsDatas(Dictionary<string, bool> newsDatas)
        {
            // 使用运行时数据副本，不会污染原始资源
            foreach (var newsData in newsDatas)
            {
                if (runtimeNewsDataDict.TryGetValue(newsData.Key, out NewsData data))
                {
                    data.isRead = newsData.Value; // 修改运行时副本
                }
            }
        }
        
        public Dictionary<string, bool> GetNewsDatas()
        {
            // 从运行时数据副本获取数据
            Dictionary<string, bool> newsDataDict = new Dictionary<string, bool>();
            foreach (var newsData in runtimeNewsDataArray)
            {
                newsDataDict[newsData.newsID] = newsData.isRead;
            }
            return newsDataDict;
        }
        
        public NewsData GetNewsByID(string newsID)
        {
            // 返回运行时数据副本
            if (runtimeNewsDataDict.TryGetValue(newsID, out NewsData newsData))
            {
                return newsData;
            }
            else
            {
                Debug.LogError($"找不到ID为 {newsID} 的新闻数据");
                return null;
            }
        }
        
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
            if (stopwatch.ElapsedMilliseconds > 100)
            {
                Debug.LogWarning($"OpenNewsInfo took {stopwatch.ElapsedMilliseconds}ms");
            }
        }
        
        private void CloseNewsInfo()
        {
            newsInfoUI.SetActive(false);
            newsBasePanel.SetActive(false);
            
            // 修改运行时副本，不会污染原始资源
            if (currentNewsData != null)
            {
                currentNewsData.isRead = true;
                checkedNewsDataArray.Add(currentNewsData);
                OnNewsBookStateChanged?.Invoke(false);
            }
        }

        public void ToggleNewsInfoBook()
        {
            Debug.Log("ToggleNewsInfoBook");
            newsBasePanel.SetActive(!newsBasePanel.activeSelf);
            newsInfoBookPanel.SetActive(!newsInfoBookPanel.activeSelf);
            OnNewsBookStateChanged?.Invoke(newsInfoBookPanel.activeSelf);
            if (newsInfoBookPanel.activeSelf)
            {
                newsInfoPanel.SetActive(false);
                ShowNewsInfoSlotList();
            }
        }

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
        
                if (slotIndex < newsInfoSlotPool.Count)
                {
                    newsInfoSlot = newsInfoSlotPool[slotIndex];
                }
                else
                {
                    newsInfoSlot = Instantiate(newsInfoSlotPrefab, newsInfoScrollRect.content);
                    newsInfoSlotPool.Add(newsInfoSlot);
                }
        
                newsInfoSlot.SetActive(true);
                NewsInfoSlot newsSlot = newsInfoSlot.GetComponent<NewsInfoSlot>();
                newsSlot.ShowNewsInfo(newsData);
        
                slotIndex++;
            }
        }

        public void ShowNewsInfoPanel(NewsData newsData)
        {
            newsInfoTitleText.text = newsData.newsTitle;
            newsInfoContentText.text = newsData.newsContent;
            newsInfoImage.sprite = newsData.newsImage;
            newsInfoPanel.SetActive(true);
        }
    }
}