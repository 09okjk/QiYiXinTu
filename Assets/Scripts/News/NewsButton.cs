using UnityEngine;
using UnityEngine.UI;

namespace News
{
    public class NewsButton:MonoBehaviour
    {
        public string newsID;
        private Button newsButton;
        private Animator animator;
        
        private void Awake()
        {
            newsButton = GetComponent<Button>();
            animator = GetComponentInChildren<Animator>();
        }
        
        private void Start()
        {
            if (newsButton != null)
            {
                newsButton.onClick.AddListener(OnNewsButtonClicked);
            }
        }
        
        public void InitializeNewsButton(string id)
        {
            newsID = id;
        }

        private void OnNewsButtonClicked()
        {
            NewsManager.Instance.OpenNewsInfo(newsID);
            gameObject.SetActive(false); // 隐藏按钮
        }
    }
}