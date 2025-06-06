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
        
        public event Action<bool> OnNewsBookStateChanged; // 新闻信息状态改变事件
        
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this; // 设置单例实例
            }
            else
            {
                Destroy(gameObject); // 如果实例已存在，则销毁当前对象
                return;
            }
            // 在Awake中加载所有新闻数据
            newsDataArray = Resources.LoadAll<NewsData>("ScriptableObjects/News");
            checkedNewsDataArray = new List<NewsData>(); // 初始化已读新闻数据列表
        }
        
        private void Start()
        {
            PlayerManager.Instance.player.RegisterNewsBookEvent();
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

        
        # region 显示单个新闻
        public void OpenNewsInfo(string newsID)
        {
            newsBasePanel.SetActive(true); // 显示新闻基础面板
            // 在这里根据newsID查找对应的新闻数据
            NewsData newsData = System.Array.Find(newsDataArray, news => news.newsID == newsID);
            
            if (newsData)
            {
                if (newsData.isRead)
                    return; // 如果新闻已读，则不执行任何操作
                
                currentNewsData = newsData; // 设置当前新闻数据
                // 更新UI显示新闻信息
                newsTitleText.text = newsData.newsTitle;
                newsContentText.text = newsData.newsContent;
                
                // 加载新闻图片
                newsImage.sprite = newsData.newsImage;
                
                // 显示新闻信息UI
                newsInfoUI.SetActive(true);
                OnNewsBookStateChanged?.Invoke(true); // 触发新闻信息状态改变事件
                
                
            }
        }
        
        private void CloseNewsInfo()
        {
            newsInfoUI.SetActive(false); // 隐藏新闻信息UI
            newsBasePanel.SetActive(false); // 隐藏新闻基础面板
            currentNewsData.isRead = true; // 设置当前新闻为已读
            checkedNewsDataArray.Add(currentNewsData); // 将当前新闻添加到已读列表
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
            // 清空之前的新闻列表
            foreach (var obj in newsInfoSlotPool)
            {
                obj.gameObject.SetActive(false); // 隐藏之前的新闻列表项
            }

            foreach (var newsData in checkedNewsDataArray)
            {
                if (newsInfoSlotPool.Count < checkedNewsDataArray.Count)
                {
                    GameObject newsInfoSlot = Instantiate(newsInfoSlotPrefab, newsInfoScrollRect.content);
                    newsInfoSlotPool.Add(newsInfoSlot); // 添加到预制体池
                    
                    // 获取新闻按钮组件并初始化
                    NewsInfoSlot newsSlot = newsInfoSlot.GetComponent<NewsInfoSlot>();
                    newsSlot.ShowNewsInfo(newsData); // 显示新闻信息
                }
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
