using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Manager;
using News;

public class SaveLoadSystem : MonoBehaviour
{
    public static SaveLoadSystem Instance;
    private static string SaveDirectory => Application.persistentDataPath + "/Saves/";
    private static int slotIndex = 0;
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        Debug.Log(SaveDirectory);
    }

    [Serializable]
    public class SaveData
    {
        // Player data
        public string playerName;
        public int playerHealth;
        public float playerMana;
        public float[] playerPosition = new float[3];
        public Dictionary<string, int> playerSkills = new Dictionary<string, int>();
        public int playerLevel;
        public int playerExperience;

        // Scene data
        public string currentSceneName;
        public PlayerPointType lastPlayerPointType;

        // NPC data
        public Dictionary<string, NPCSaveData> npcData = new Dictionary<string, NPCSaveData>();

        // Inventory data
        public List<string> questItems = new List<string>();
        public List<string> puzzleItems = new List<string>();
        public Dictionary<string, int> itemQuantities = new Dictionary<string, int>();

        // Quest data
        public List<string> activeQuestIDs = new List<string>();
        public List<string> completedQuestIDs = new List<string>();
        public Dictionary<string, List<string>> completedObjectives = new Dictionary<string, List<string>>();
        public string currentQuestID;

        // Game state data
        public Dictionary<string, bool> flags = new Dictionary<string, bool>();

        // Enemy data
        public Dictionary<string, EnemySaveData> enemyData = new Dictionary<string, EnemySaveData>();

        // News data
        public List<string> readNewsIDs = new List<string>();

        // Puzzle data
        public Dictionary<string, PuzzleSaveData> puzzleData = new Dictionary<string, PuzzleSaveData>();

        // Game time data
        public float totalPlayTime;
        public DateTime gameStartTime;

        // Save metadata
        public string saveName;
        public DateTime saveDate;
        public string gameVersion;
    }

    [Serializable]
    public class NPCSaveData
    {
        public string npcID;
        public float[] position = new float[3];
        public bool isActive;
        public bool isFollowing;
        public bool canInteract;
        public List<string> completedDialogues = new List<string>();
        public Dictionary<string, bool> npcFlags = new Dictionary<string, bool>();
    }

    [Serializable]
    public class EnemySaveData
    {
        public string enemyID;
        public float[] position = new float[3];
        public bool isActive;
        public bool isDead;
        public int currentHealth;
        public EnemyType enemyType;
    }

    [Serializable]
    public class PuzzleSaveData
    {
        public string puzzleID;
        public bool isCompleted;
        public bool isActive;
        public Dictionary<string, bool> puzzleStates = new Dictionary<string, bool>();
        public List<string> solvedSteps = new List<string>();
    }

    public static void SaveGame(int slotIdx)
    {
        slotIndex = slotIdx;

        if (!Directory.Exists(SaveDirectory))
        {
            Directory.CreateDirectory(SaveDirectory);
        }

        SaveData saveData = CreateSaveData();
        string savePath = SaveDirectory + "save_" + slotIndex + ".sav";

        try
        {
            BinaryFormatter formatter = new BinaryFormatter();
            FileStream fileStream = new FileStream(savePath, FileMode.Create);
            formatter.Serialize(fileStream, saveData);
            fileStream.Close();
            Debug.Log("Game saved to slot " + slotIndex);
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to save game: {e.Message}");
        }
    }

    private static SaveData CreateSaveData()
    {
        SaveData saveData = new SaveData();

        // Save player data
        SavePlayerData(saveData);

        // Save scene data
        SaveSceneData(saveData);

        // Save NPC data
        SaveNPCData(saveData);

        // Save inventory data
        SaveInventoryData(saveData);

        // Save quest data
        SaveQuestData(saveData);

        // Save game state data
        SaveGameStateData(saveData);

        // Save enemy data
        SaveEnemyData(saveData);

        // Save news data
        SaveNewsData(saveData);

        // Save puzzle data
        SavePuzzleData(saveData);

        // Save metadata
        saveData.saveName = "Save " + (slotIndex + 1);
        saveData.saveDate = DateTime.Now;
        saveData.gameVersion = Application.version;

        return saveData;
    }

    private static void SavePlayerData(SaveData saveData)
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null && PlayerManager.Instance != null)
        {
            // Position
            Vector3 position = player.transform.position;
            saveData.playerPosition[0] = position.x;
            saveData.playerPosition[1] = position.y;
            saveData.playerPosition[2] = position.z;

            // Basic stats
            saveData.playerName = PlayerManager.Instance.player.playerData.playerName;
            saveData.playerHealth = PlayerManager.Instance.player.playerData.CurrentHealth;
            saveData.playerMana = PlayerManager.Instance.player.playerData.CurrentMana;

            // Additional player data can be added here
        }
    }

    private static void SaveSceneData(SaveData saveData)
    {
        saveData.currentSceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        saveData.lastPlayerPointType = GameStateManager.Instance.GetPlayerPointType();
    }

    private static void SaveNPCData(SaveData saveData)
    {
        if (NPCManager.Instance != null)
        {
            foreach (NPC npc in NPCManager.Instance.npcList)
            {
                NPCSaveData npcSaveData = new NPCSaveData();
                npcSaveData.npcID = npc.npcData.npcID;
                
                Vector3 position = npc.transform.position;
                npcSaveData.position[0] = position.x;
                npcSaveData.position[1] = position.y;
                npcSaveData.position[2] = position.z;
                
                npcSaveData.isActive = npc.gameObject.activeSelf;
                npcSaveData.isFollowing = npc.isFollowing;

                // Save NPC-specific flags
                foreach (var flag in GameStateManager.Instance.GetAllFlags())
                {
                    if (flag.Key.Contains(npc.npcData.npcID))
                    {
                        npcSaveData.npcFlags[flag.Key] = flag.Value;
                    }
                }

                saveData.npcData[npc.npcData.npcID] = npcSaveData;
            }
        }
    }

    private static void SaveInventoryData(SaveData saveData)
    {
        if (InventoryManager.Instance != null)
        {
            List<ItemData> allItems = InventoryManager.Instance.GetAllItems();

            foreach (ItemData item in allItems)
            {
                if (item.itemType == ItemType.QuestItem)
                {
                    saveData.questItems.Add(item.itemID);
                }
                else if (item.itemType == ItemType.PuzzleItem)
                {
                    saveData.puzzleItems.Add(item.itemID);
                }

                // Save item quantities if needed
                saveData.itemQuantities[item.itemID] = 1; // Modify based on your inventory system
            }
        }
    }

    private static void SaveQuestData(SaveData saveData)
    {
        if (QuestManager.Instance != null)
        {
            // Current quest
            if (!string.IsNullOrEmpty(QuestManager.Instance.currentQuestID))
            {
                saveData.currentQuestID = QuestManager.Instance.currentQuestID;
                saveData.activeQuestIDs.Add(QuestManager.Instance.currentQuestID);
            }

            // You'll need to implement methods to get completed quests
            // saveData.completedQuestIDs = QuestManager.Instance.GetCompletedQuestIDs();
            // saveData.completedObjectives = QuestManager.Instance.GetAllCompletedObjectives();
        }
    }

    private static void SaveGameStateData(SaveData saveData)
    {
        if (GameStateManager.Instance != null)
        {
            saveData.flags = GameStateManager.Instance.GetAllFlags();
        }
    }

    private static void SaveEnemyData(SaveData saveData)
    {
        if (EnemyManager.Instance != null)
        {
            foreach (Enemy enemy in EnemyManager.Instance.enemies)
            {
                EnemySaveData enemySaveData = new EnemySaveData();
                enemySaveData.enemyID = enemy.enemyData.enemyID;
                enemySaveData.enemyType = enemy.enemyData.enemyType;
                
                Vector3 position = enemy.transform.position;
                enemySaveData.position[0] = position.x;
                enemySaveData.position[1] = position.y;
                enemySaveData.position[2] = position.z;
                
                enemySaveData.isActive = enemy.gameObject.activeSelf;
                enemySaveData.isDead = !enemy.isActiveAndEnabled;
                enemySaveData.currentHealth = enemy.enemyData.CurrentHealth; // Adjust based on your enemy system

                saveData.enemyData[enemy.enemyData.enemyID] = enemySaveData;
            }
        }
    }

    private static void SaveNewsData(SaveData saveData)
    {
        if (NewsManager.Instance != null)
        {
            foreach (var newsData in NewsManager.Instance.checkedNewsDataArray)
            {
                if (newsData.isRead)
                {
                    saveData.readNewsIDs.Add(newsData.newsID);
                }
            }
        }
    }

    private static void SavePuzzleData(SaveData saveData)
    {
        // Add puzzle saving logic based on your puzzle system
        // This is a placeholder - implement based on your specific puzzle mechanics
        
        // Example:
        // GameObject[] puzzles = GameObject.FindGameObjectsWithTag("Puzzle");
        // foreach (GameObject puzzle in puzzles)
        // {
        //     PuzzleSaveData puzzleSaveData = new PuzzleSaveData();
        //     // Save puzzle state
        //     saveData.puzzleData[puzzle.name] = puzzleSaveData;
        // }
    }

    public static void LoadGame(int slotIdx)
    {
        slotIndex = slotIdx;
        string savePath = SaveDirectory + "save_" + slotIndex + ".sav";

        if (File.Exists(savePath))
        {
            try
            {
                BinaryFormatter formatter = new BinaryFormatter();
                FileStream fileStream = new FileStream(savePath, FileMode.Open);
                SaveData saveData = formatter.Deserialize(fileStream) as SaveData;
                fileStream.Close();

                if (saveData != null)
                {
                    ApplySaveData(saveData);
                    Debug.Log("Game loaded from slot " + slotIndex);
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to load game: {e.Message}");
            }
        }
        else
        {
            Debug.LogWarning("Save file not found in slot " + slotIndex);
        }
    }

    private static void ApplySaveData(SaveData saveData)
    {
        string currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        if (currentScene != saveData.currentSceneName)
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(saveData.currentSceneName);
            GameManager.Instance.StartCoroutine(ApplySaveDataAfterSceneLoad(saveData));
            return;
        }

        ApplySaveDataDirect(saveData);
    }

    private static IEnumerator ApplySaveDataAfterSceneLoad(SaveData saveData)
    {
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();
        ApplySaveDataDirect(saveData);
    }

    private static void ApplySaveDataDirect(SaveData saveData)
    {
        // Load player data
        LoadPlayerData(saveData);

        // Load scene data
        LoadSceneData(saveData);

        // Load NPC data
        LoadNPCData(saveData);

        // Load inventory data
        LoadInventoryData(saveData);

        // Load quest data
        LoadQuestData(saveData);

        // Load game state data
        LoadGameStateData(saveData);

        // Load enemy data
        LoadEnemyData(saveData);

        // Load news data
        LoadNewsData(saveData);

        // Load puzzle data
        LoadPuzzleData(saveData);
    }

    private static void LoadPlayerData(SaveData saveData)
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null && PlayerManager.Instance != null)
        {
            // Position
            Vector3 position = new Vector3(
                saveData.playerPosition[0],
                saveData.playerPosition[1],
                saveData.playerPosition[2]
            );
            player.transform.position = position;

            // Basic stats
            if (!string.IsNullOrEmpty(saveData.playerName))
            {
                PlayerManager.Instance.player.playerData.playerName = saveData.playerName;
            }
            
            PlayerManager.Instance.player.playerData.CurrentHealth = saveData.playerHealth;
            PlayerManager.Instance.player.playerData.CurrentMana = saveData.playerMana;
        }
    }

    private static void LoadSceneData(SaveData saveData)
    {
        GameStateManager.Instance.SetPlayerPointType(saveData.lastPlayerPointType);
    }

    private static void LoadNPCData(SaveData saveData)
    {
        if (NPCManager.Instance != null)
        {
            foreach (var npcPair in saveData.npcData)
            {
                string npcID = npcPair.Key;
                NPCSaveData npcSaveData = npcPair.Value;
                
                NPC npc = NPCManager.Instance.GetNpc(npcID);
                if (npc != null)
                {
                    // Position
                    Vector3 position = new Vector3(
                        npcSaveData.position[0],
                        npcSaveData.position[1],
                        npcSaveData.position[2]
                    );
                    npc.transform.position = position;

                    // State
                    if (npcSaveData.isActive)
                    {
                        npc.ActivateNpc();
                    }
                    else
                    {
                        npc.DeactivateNpc();
                    }

                    if (npcSaveData.isFollowing)
                    {
                        npc.FollowTargetPlayer();
                    }

                    npc.SetCanInteract(npcSaveData.canInteract);

                    // Restore NPC-specific flags
                    foreach (var flag in npcSaveData.npcFlags)
                    {
                        GameStateManager.Instance.SetFlag(flag.Key, flag.Value);
                    }
                }
            }
        }
    }

    private static void LoadInventoryData(SaveData saveData)
    {
        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.ClearInventory();

            // Add quest items
            foreach (string itemID in saveData.questItems)
            {
                ItemData item = Resources.Load<ItemData>($"Items/{itemID}");
                if (item != null)
                {
                    InventoryManager.Instance.AddItem(item);
                }
            }

            // Add puzzle items
            foreach (string itemID in saveData.puzzleItems)
            {
                ItemData item = Resources.Load<ItemData>($"Items/{itemID}");
                if (item != null)
                {
                    InventoryManager.Instance.AddItem(item);
                }
            }
        }
    }

    private static void LoadQuestData(SaveData saveData)
    {
        if (QuestManager.Instance != null)
        {
            // Restore current quest
            if (!string.IsNullOrEmpty(saveData.currentQuestID))
            {
                QuestManager.Instance.StartQuest(saveData.currentQuestID);
            }

            // Restore completed quests
            foreach (string questID in saveData.completedQuestIDs)
            {
                // You'll need to implement a method to mark quests as completed
                // QuestManager.Instance.MarkQuestAsCompleted(questID);
            }
        }
    }

    private static void LoadGameStateData(SaveData saveData)
    {
        if (GameStateManager.Instance != null)
        {
            GameStateManager.Instance.SetAllFlags(saveData.flags);
        }
    }

    private static void LoadEnemyData(SaveData saveData)
    {
        if (EnemyManager.Instance != null)
        {
            foreach (var enemyPair in saveData.enemyData)
            {
                string enemyID = enemyPair.Key;
                EnemySaveData enemySaveData = enemyPair.Value;

                // Find enemy by ID (you might need to implement this method)
                Enemy[] allEnemies = EnemyManager.Instance.enemies;
                Enemy enemy = System.Array.Find(allEnemies, e => e.enemyData.enemyID == enemyID);
                
                if (enemy != null)
                {
                    // Position
                    Vector3 position = new Vector3(
                        enemySaveData.position[0],
                        enemySaveData.position[1],
                        enemySaveData.position[2]
                    );
                    enemy.transform.position = position;

                    // State
                    if (enemySaveData.isDead)
                    {
                        enemy.DeactivateEnemy();
                    }
                    else if (enemySaveData.isActive)
                    {
                        enemy.ActivateEnemy();
                    }
                }
            }
        }
    }

    private static void LoadNewsData(SaveData saveData)
    {
        if (NewsManager.Instance != null)
        {
            foreach (string newsID in saveData.readNewsIDs)
            {
                // You might need to implement a method to mark news as read
                // NewsManager.Instance.MarkNewsAsRead(newsID);
            }
        }
    }

    private static void LoadPuzzleData(SaveData saveData)
    {
        // Implement puzzle loading based on your puzzle system
        foreach (var puzzlePair in saveData.puzzleData)
        {
            string puzzleID = puzzlePair.Key;
            PuzzleSaveData puzzleSaveData = puzzlePair.Value;
            
            // Restore puzzle state
            // Implementation depends on your puzzle system
        }
    }

    public static SaveDataInfo[] GetSaveDataInfos()
    {
        if (!Directory.Exists(SaveDirectory))
        {
            return new SaveDataInfo[0];
        }

        string[] saveFiles = Directory.GetFiles(SaveDirectory, "save_*.sav");
        SaveDataInfo[] saveDataInfos = new SaveDataInfo[saveFiles.Length];

        for (int i = 0; i < saveFiles.Length; i++)
        {
            string fileName = Path.GetFileName(saveFiles[i]);
            int slotIdx = int.Parse(fileName.Substring(5, fileName.Length - 9));

            try
            {
                BinaryFormatter formatter = new BinaryFormatter();
                FileStream fileStream = new FileStream(saveFiles[i], FileMode.Open);
                SaveData saveData = formatter.Deserialize(fileStream) as SaveData;
                fileStream.Close();

                saveDataInfos[i] = new SaveDataInfo
                {
                    slotIndex = slotIdx,
                    saveName = saveData.saveName,
                    saveDate = saveData.saveDate,
                    sceneName = saveData.currentSceneName,
                    playerName = saveData.playerName,
                    gameVersion = saveData.gameVersion
                };
            }
            catch (Exception e)
            {
                Debug.LogError($"Error reading save file {fileName}: {e.Message}");
                saveDataInfos[i] = new SaveDataInfo
                {
                    slotIndex = slotIdx,
                    saveName = "Corrupted Save",
                    saveDate = DateTime.MinValue,
                    sceneName = "Unknown"
                };
            }
        }

        System.Array.Sort(saveDataInfos, (a, b) => a.slotIndex.CompareTo(b.slotIndex));
        return saveDataInfos;
    }

    public static bool DeleteSave(int slotIdx)
    {
        string savePath = SaveDirectory + "save_" + slotIdx + ".sav";
        
        if (File.Exists(savePath))
        {
            try
            {
                File.Delete(savePath);
                Debug.Log($"Save file {slotIdx} deleted successfully");
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to delete save file: {e.Message}");
                return false;
            }
        }
        
        return false;
    }
}

