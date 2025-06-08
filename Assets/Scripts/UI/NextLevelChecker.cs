using Manager;
using UnityEngine;

namespace UI
{
    public class NextLevelChecker:MonoBehaviour
    {
        public string nextSceneName;
        public PlayerPointType nextScenePointType;
        private BoxCollider2D boxCollider;
        private bool hasTriggered = false;

         
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
            if (other.CompareTag("Player") && !hasTriggered)
            {
                Debug.Log("OnTriggerEnter2D");
                hasTriggered = true;
                boxCollider.enabled = false; // 禁用碰撞体，防止重复触发
                GameStateManager.Instance.SetPlayerPointType(nextScenePointType);
                // 触发场景切换逻辑
                GameManager.Instance.LoadScene(nextSceneName);
            }
        }
    }
}