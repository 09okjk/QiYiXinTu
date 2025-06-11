using UnityEngine;
using UnityEngine.UI;

namespace News
{
    public class NewsButton : MonoBehaviour
    {
        public string newsID;
        public Button newsButton;
        public Image shadowImage; // 用于显示按钮的阴影效果
        private NewsData newsData;
        
        private void Awake()
        {
            newsButton = GetComponent<Button>();
            if (newsButton == null)
            {
                Debug.LogError("Button组件缺失: " + gameObject.name);
            }
        }
        
        private void Start()
        {
            if (newsButton != null)
            {
                newsButton.onClick.RemoveAllListeners(); // 清除可能存在的监听器
                newsButton.onClick.AddListener(OnNewsButtonClicked);
                Debug.Log($"按钮 {gameObject.name} 已添加点击监听器");
            }
        }
        
        public void InitializeNewsButton(string id)
        {
            newsID = id;
            Debug.Log($"按钮 {gameObject.name} 已初始化，ID: {id}");
        }

        private void OnNewsButtonClicked()
        {
            Debug.Log($"按钮 {gameObject.name} 被点击，ID: {newsID}");
            
            // 确保NewsManager实例存在
            if (NewsManager.Instance != null)
            {
                if (newsData == null)
                {
                    Debug.LogError("新闻数据未设置，无法打开新闻信息");
                    return;
                }
                NewsManager.Instance.OpenNewsInfo(newsData);
                shadowImage.gameObject.SetActive(false);
                gameObject.SetActive(false); // 隐藏按钮
            }
            else
            {
                Debug.LogError("NewsManager实例不存在");
            }
        }
        
        public void SetNewsData(NewsData data)
        {
            if (newsData == null)
            {
                return;
            }
            
            if(newsID == data.newsID)
            {
                Debug.Log($"设置新闻数据: {data.newsID}");
                newsData = data;
            }
            else
            {
                Debug.LogError("新闻ID不匹配: " + newsID + " != " + data.newsID);
            }
        }
    }
}