using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Manager
{
    public class PlayerManager:MonoBehaviour
    {
        public static PlayerManager Instance { get; private set; }
        [SerializeField] private GameObject playerPrefab;
        [HideInInspector] public Player player;
        
        [SerializeField]
        private List<GameObject> createPlayerPoints = new List<GameObject>();
        private void Awake()
        {
            if (!Instance)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Debug.LogWarning("Multiple PlayerManager instances found. Destroying duplicate.");
                Destroy(gameObject);
            }
            player = Instantiate(playerPrefab).GetComponent<Player>();
            player.gameObject.name = "Player";
        }

        private void Start()
        {
            player.gameObject.SetActive(false);
            
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnEnable()
        {
            DialogueManager.Instance.OnDialogueEnd += CheckDialogueID;
        }

        private void OnDisable()
        {
            DialogueManager.Instance.OnDialogueEnd -= CheckDialogueID;
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode arg1)
        {
            SetPlayer();
            // 如果是女生宿舍场景，直接显示玩家
            // if (scene.name == "女生宿舍")
            // {
            //     player.gameObject.SetActive(true);
            //     NPCManager.Instance.ShowNpc("LuXinsheng");
            // }
        }

        private void CheckDialogueID(string dialogueID)
        {
            
        }

        public void SetPlayer()
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
            
            DialogueManager.Instance.StartDialogueByID("dialogue_001");
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