using UnityEngine;
using UnityEngine.UI;

namespace News
{
    public class NewsButton : MonoBehaviour
    {
        public string newsID;
        public Button newsButton;
        public Image shadowImage; // 用于显示按钮的阴影效果
        
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
                NewsManager.Instance.OpenNewsInfo(newsID);
                shadowImage.gameObject.SetActive(false);
                gameObject.SetActive(false); // 隐藏按钮
            }
            else
            {
                Debug.LogError("NewsManager实例不存在");
            }
        }
        
        // 添加手动测试方法，可以从Inspector中调用
        public void TestButtonClick()
        {
            OnNewsButtonClicked();
        }
    }
}