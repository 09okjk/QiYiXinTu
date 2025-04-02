using UnityEngine;

namespace Manager
{
    public class PlayerManager:MonoBehaviour
    {
        public static PlayerManager Instance { get; private set; }
        public Player player;
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Debug.LogWarning("Multiple PlayerManager instances found. Destroying duplicate.");
                Destroy(gameObject);
            }
        }
    
    
    }
}