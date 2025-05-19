using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace News
{
    public class NewsInfoSlot:MonoBehaviour
    {
        private string newsID; // 新闻ID
        public TextMeshProUGUI newsTitleText; // 新闻标题文本
        public Image newsImage; // 新闻图片
        public Button slotButton; // 关闭按钮
        
        private NewsData currentNewsData;

        private void Awake()
        {
            slotButton.onClick.AddListener(OnSlotButtonClicked);
        }
        
        public void ShowNewsInfo(NewsData newsData)
        {
            newsID = newsData.newsID; // 设置新闻ID
            newsTitleText.text = newsData.newsTitle; // 设置新闻标题
            newsImage.sprite = newsData.newsImage; // 设置新闻图片
            currentNewsData = newsData; // 设置当前新闻数据
            
            gameObject.SetActive(true); // 显示新闻信息
        }

        private void OnSlotButtonClicked()
        {
            NewsManager.Instance.ShowNewsInfoPanel(currentNewsData);
        }
    }
}