using System;
using Manager;
using Save;
using UnityEngine;
using UnityEngine.SceneManagement;

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
        
        private async void OnTriggerEnter2D(Collider2D other)
        {
            try
            {
                if (other.CompareTag("Player") && !hasTriggered)
                {
                    Debug.Log("OnTriggerEnter2D");
                    hasTriggered = true;
                    boxCollider.enabled = false; // 禁用碰撞体，防止重复触发
                    GameStateManager.Instance.SetPlayerPointType(nextScenePointType);
                    GameStateManager.Instance.SetFlag("FirstEntry_" + SceneManager.GetActiveScene().name, false);
                    await AsyncSaveLoadSystem.SaveGameAsync(0);
                    // 触发场景切换逻辑
                    GameManager.Instance.LoadScene(nextSceneName);
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Error in NextLevelChecker.OnTriggerEnter2D: {e.Message}\n{e.StackTrace}");
                throw;
            }
        }
    }
}