using System;
using System.Collections.Generic;
using Manager;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace News
{
    [Serializable]
    public class NewsGameData
    { 
        public string newsID; // 新闻ID
        public string newsTitle; // 新闻标题
        [TextArea] public string newsContent; // 新闻内容
        public string newsImageID; // 新闻图片ID
        public bool isRead; // 是否已读
    }
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
        private Dictionary<string, NewsGameData> runtimeNewsDataDict = new();
        
        public List<NewsGameData> checkedNewsDataArray = new();
        private NewsGameData currentNewsData;
        private List<GameObject> newsInfoSlotPool = new();

        public event Action<bool> OnNewsBookStateChanged;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                LoadOriginalNewsData();
                SaveAllNewsData();
            }
            else
            {
                Destroy(gameObject);
            }
        }
                
        private void Start()
        {
            newsInfoUI.SetActive(false);
            newsInfoBookPanel.SetActive(false);
            newsInfoPanel.SetActive(false);


            
            closeButton.onClick.AddListener(CloseNewsInfo);
            newsInfoCloseButton.onClick.AddListener(ToggleNewsInfoBook);
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
        private bool SaveAllNewsData(Dictionary<string,NewsGameData> newsGameDataDict = null)
        {
            try
            {
                runtimeNewsDataDict.Clear();

                if (newsGameDataDict == null || newsGameDataDict.Count == 0)
                {
                    foreach (var originalNewsData in originalNewsDataArray)
                    {
                        var newsGameData = new NewsGameData
                        {
                            newsID = originalNewsData.newsID,
                            newsTitle = originalNewsData.newsTitle,
                            newsContent = originalNewsData.newsContent,
                            newsImageID = originalNewsData.newsImageID,
                            isRead = originalNewsData.isRead
                        };
                        runtimeNewsDataDict[newsGameData.newsID] = newsGameData;
                    }
                }
                else
                {
                    runtimeNewsDataDict = new Dictionary<string, NewsGameData>(newsGameDataDict);
                }
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"设置新闻数据时发生错误: {e.Message}");
                return false;
            }
        }

        public bool SetAllNewsData(Dictionary<string, NewsGameData> newsGameDataDict)
        {
            return SaveAllNewsData(newsGameDataDict);
        }

        /// <summary>
        /// 重置所有新闻数据到原始状态
        /// </summary>
        public void ResetAllNewsData()
        {
            SaveAllNewsData();
            
            // 重置已读新闻列表
            checkedNewsDataArray.Clear();
            currentNewsData = null;
            
            Debug.Log("已重置所有新闻数据到原始状态");
        }


        
        public NewsGameData GetNewsByID(string newsID)
        {
            // 返回运行时数据副本
            if (runtimeNewsDataDict.TryGetValue(newsID, out NewsGameData newsData))
            {
                return newsData;
            }
            else
            {
                Debug.LogError($"找不到ID为 {newsID} 的新闻数据");
                return null;
            }
        }
        
        public Dictionary<string, NewsGameData> GetAllNewsData()
        {
            // 返回运行时数据副本
            return new Dictionary<string, NewsGameData>(runtimeNewsDataDict);
        }
        
        public void OpenNewsInfo(string newsID)
        {
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            
            if (!runtimeNewsDataDict.TryGetValue(newsID, out NewsGameData newsData))
            {
                Debug.LogError($"找不到ID为 {newsID} 的新闻数据");
                return;
            }
            newsBasePanel.SetActive(true);
    
            if (newsData.isRead)
                return;
        
            currentNewsData = newsData;
            newsTitleText.text = newsData.newsTitle;
            newsContentText.text = newsData.newsContent;
            newsImage.sprite = GetNewsImageByID(newsData.newsImageID);
            newsInfoUI.SetActive(true);
            OnNewsBookStateChanged?.Invoke(true);
            
            stopwatch.Stop();
            if (stopwatch.ElapsedMilliseconds > 100)
            {
                Debug.LogWarning($"OpenNewsInfo took {stopwatch.ElapsedMilliseconds}ms");
            }
        }

        public Sprite GetNewsImageByID(string imageID)
        {
            Sprite sprite = Resources.Load<Sprite>($"Art/News/{imageID}");
            if (sprite == null)
            {
                Debug.LogError($"找不到ID为 {imageID} 的新闻图片");
                return null;
            }
            return sprite;
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
            checkedNewsDataArray.Clear();
            foreach (NewsGameData data in runtimeNewsDataDict.Values)
            {
                if (data.isRead)
                {
                    checkedNewsDataArray.Add(data);
                }
            }
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

        public void ShowNewsInfoPanel(NewsGameData newsData)
        {
            newsInfoTitleText.text = newsData.newsTitle;
            newsInfoContentText.text = newsData.newsContent;
            newsInfoImage.sprite = GetNewsImageByID(newsData.newsImageID);
            newsInfoPanel.SetActive(true);
        }
    }
}