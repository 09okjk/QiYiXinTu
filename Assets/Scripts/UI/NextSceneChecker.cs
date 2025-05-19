using Manager;
using UnityEngine;

namespace UI
{
    public class NextSceneChecker:MonoBehaviour
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
                GameStateManager.Instance.SetPlayerPointType(nextScenePointType);
                // 触发场景切换逻辑
                GameManager.Instance.LoadScene(nextSceneName);
            }
        }
    }
}