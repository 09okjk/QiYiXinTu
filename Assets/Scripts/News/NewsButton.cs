using Audio;
using UnityEngine;
using UnityEngine.UI;

namespace News
{
    public class NewsButton : MonoBehaviour
    {
        public string newsID;
        public Button newsButton;
        public Image shadowImage; // 用于显示按钮的阴影效果
        public GameObject NewsUI; // 新闻信息UI
        
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
            if (GameStateManager.Instance.GetFlag("newsIsRead_"+newsID))
                gameObject.SetActive(false);
        }

        private void OnNewsButtonClicked()
        {
            Debug.Log($"按钮 {gameObject.name} 被点击，ID: {newsID}");
            AudioManager.Instance.PlayEffectAudio("news_audio");
            // 确保NewsManager实例存在
            if (NewsManager.Instance != null)
            {
                NewsManager.Instance.OpenNewsInfo(newsID);
                NewsUI.gameObject.SetActive(false);
            }
            else
            {
                Debug.LogError("NewsManager实例不存在");
            }
            GameStateManager.Instance.SetFlag("newsIsRead_"+newsID, true);
        }
    }
}