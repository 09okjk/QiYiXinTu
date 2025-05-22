using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Manager
{
    public class PlayerManager:MonoBehaviour
    {
        public static PlayerManager Instance { get; private set; }
        public Player player;
        
        [SerializeField]
        private List<GameObject> createPlayerPoints = new List<GameObject>();
        private void Awake()
        {
            if (!Instance)
            {
                Instance = this;
            }
            else
            {
                Debug.LogWarning("Multiple PlayerManager instances found. Destroying duplicate.");
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            SetPlayer();
        }

        private void SetPlayer()
        {
            createPlayerPoints.Clear();
            GameObject[] points = GameObject.FindGameObjectsWithTag("PlayerStart");
            foreach (GameObject point in points)
            {
                createPlayerPoints.Add(point);
            }
            MoveToPlayerPoint(GameStateManager.Instance.GetPlayerPointType());
        }

        public void MoveToPlayerPoint(PlayerPointType pointType)
        {
            if (createPlayerPoints.Count == 0 || pointType == PlayerPointType.None)
            {
                Debug.LogError($"No player points found. Count:{createPlayerPoints.Count}. PointType:{pointType}");
                return;
            }
            
            GameObject point = createPlayerPoints.Find(p => p.gameObject.name == $"PlayerPoint_{pointType}");
            player.transform.position = point.transform.position;
            player.gameObject.SetActive(true);

            GameStateManager.Instance.SetPlayerPointType(PlayerPointType.None);
        }

        public void ChangePlayerName(string newName)
        {
            if (string.IsNullOrEmpty(newName))
            {
                Debug.LogError("New name is null or empty.");
                return;
            }
            
            player.playerData.playerName = newName;
            Debug.Log($"Player name changed to: {newName}");
        }
    }

    public enum PlayerPointType
    {
        None,
        Left,
        Right,
        Middle1,
        Middle2,
        Middle3,
    }
}