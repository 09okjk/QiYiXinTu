using UnityEngine;

namespace UI
{
    public class NextSceneChecker:MonoBehaviour
    {
        public string nextSceneName;
        private BoxCollider2D boxCollider;
        
        private void Awake()
        {
            boxCollider = GetComponent<BoxCollider2D>();
            if (!boxCollider)
            {
                Debug.LogError("BoxCollider2D component is missing on this GameObject.");
            }
        }
        
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
            {
                // 触发场景切换逻辑
                GameManager.Instance.LoadScene(nextSceneName); // 替换为实际的场景名称
            }
        }
    }
}