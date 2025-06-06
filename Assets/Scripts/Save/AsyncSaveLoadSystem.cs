using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using UnityEngine;
using Manager;
using News;
using Newtonsoft.Json;
using Formatting = Newtonsoft.Json.Formatting;

namespace Save
{
    public class AsyncSaveLoadSystem : MonoBehaviour
    {
        private static string SaveDirectory => Application.persistentDataPath + "/Saves/";
    
        [Header("Save Settings")]
        [SerializeField] private bool useJsonFormat = true; // JSON vs Binary
        [SerializeField] private bool showSaveProgress = true;
    
        public static AsyncSaveLoadSystem Instance { get; private set; }
    
        // 进度回调
        public static event Action<float> OnSaveProgress;
        public static event Action<float> OnLoadProgress;
        public static event Action<string> OnSaveComplete;
        public static event Action<string> OnLoadComplete;

    #region Data

    [Serializable]
    public class SaveData
    {
        // Player data
        public string playerName;
        public int playerHealth;
        public float playerMana;
        public float[] playerPosition = new float[3];
        public Dictionary<string, int> playerSkills = new Dictionary<string, int>();

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
        public string sceneName; // NPC所在场景
        public float[] position = new float[3];
        public bool isActive;
        public bool isFollowing;
        public bool canInteract;
        public List<string> dialogueIDs = new List<string>();
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
    #endregion
        
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

    #region 异步保存方法

    /// <summary>
    /// 异步保存游戏
    /// </summary>
    public static async Task<bool> SaveGameAsync(int slotIdx, IProgress<float> progress = null)
    {
        try
        {
            progress?.Report(0f);
        
            // 确保目录存在
            if (!Directory.Exists(SaveDirectory))
            {
                Directory.CreateDirectory(SaveDirectory);
            }

            progress?.Report(0.1f);

            // 创建保存数据（可能耗时）
            SaveData saveData = await CreateSaveDataAsync(progress);
        
            progress?.Report(0.8f);

            // 写入文件
            string savePath = SaveDirectory + "save_" + slotIdx + ".sav";
            bool success = await WriteSaveFileAsync(saveData, savePath);
        
            progress?.Report(1f);
        
            OnSaveComplete?.Invoke(success ? "保存成功！" : "保存失败！");
            return success;
        }
        catch (Exception e)
        {
            Debug.LogError($"异步保存失败: {e.Message}");
            OnSaveComplete?.Invoke("保存失败：" + e.Message);
            return false;
        }
    }

    /// <summary>
    /// 异步创建保存数据
    /// </summary>
    private static async Task<SaveData> CreateSaveDataAsync(IProgress<float> progress = null)
    {
        SaveData saveData = new SaveData();
    
        // 分步骤保存，每步都可以异步处理
        await Task.Run(() => SavePlayerData(saveData));
        progress?.Report(0.2f);
    
        await Task.Run(() => SaveSceneData(saveData));
        progress?.Report(0.3f);
    
        await Task.Run(() => SaveNPCData(saveData));
        progress?.Report(0.4f);
    
        await Task.Run(() => SaveInventoryData(saveData));
        progress?.Report(0.5f);
    
        await Task.Run(() => SaveQuestData(saveData));
        progress?.Report(0.6f);
    
        await Task.Run(() => SaveGameStateData(saveData));
        progress?.Report(0.7f);
    
        // 元数据
        saveData.saveName = "Save " + DateTime.Now.ToString("yyyy-MM-dd HH:mm");
        saveData.saveDate = DateTime.Now;
        saveData.gameVersion = Application.version;
    
        return saveData;
    }

    /// <summary>
    /// 异步写入保存文件
    /// </summary>
    private static async Task<bool> WriteSaveFileAsync(SaveData saveData, string savePath)
    {
        try
        {
            if (Instance.useJsonFormat)
            {
                // JSON格式 - 人类可读，便于调试
                string jsonData = await Task.Run(() => 
                    JsonConvert.SerializeObject(saveData, Formatting.Indented));
            
                await File.WriteAllTextAsync(savePath, jsonData, Encoding.UTF8);
            }
        
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"写入文件失败: {e.Message}");
            return false;
        }
    }

    #endregion

    #region 异步加载方法

    /// <summary>
    /// 异步加载游戏
    /// </summary>
    public static async Task<bool> LoadGameAsync(int slotIdx, IProgress<float> progress = null)
    {
        try
        {
            progress?.Report(0f);
        
            string savePath = SaveDirectory + "save_" + slotIdx + ".sav";
        
            if (!File.Exists(savePath))
            {
                Debug.LogWarning($"存档文件不存在: {savePath}");
                OnLoadComplete?.Invoke("存档文件不存在！");
                return false;
            }

            progress?.Report(0.1f);

            // 读取文件
            SaveData saveData = await ReadSaveFileAsync(savePath);
            if (saveData == null)
            {
                OnLoadComplete?.Invoke("存档文件损坏！");
                return false;
            }

            progress?.Report(0.5f);

            // 应用数据
            await ApplySaveDataAsync(saveData, progress);
        
            progress?.Report(1f);
            OnLoadComplete?.Invoke("加载完成！");
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"异步加载失败: {e.Message}");
            OnLoadComplete?.Invoke("加载失败：" + e.Message);
            return false;
        }
    }

    /// <summary>
    /// 异步读取保存文件
    /// </summary>
    private static async Task<SaveData> ReadSaveFileAsync(string savePath)
    {
        try
        {
            if (Instance.useJsonFormat)
            {
                string jsonData = await File.ReadAllTextAsync(savePath, Encoding.UTF8);
                return await Task.Run(() => 
                    JsonConvert.DeserializeObject<SaveData>(jsonData));
            }
            else
            {
                // 二进制格式 - 更高效，但不易调试
                using (FileStream fs = new FileStream(savePath, FileMode.Open, FileAccess.Read))
                {
                    BinaryReader reader = new BinaryReader(fs);
                    string jsonData = reader.ReadString();
                    return await Task.Run(() => 
                        JsonConvert.DeserializeObject<SaveData>(jsonData));
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"读取文件失败: {e.Message}");
            return null;
        }
    }

    /// <summary>
    /// 异步应用保存数据
    /// </summary>
    private static async Task ApplySaveDataAsync(SaveData saveData, IProgress<float> progress = null)
    {
        // 检查是否需要切换场景
        string currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        if (currentScene != saveData.currentSceneName)
        {
            // 异步场景加载
            var sceneLoad = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(saveData.currentSceneName);
        
            while (!sceneLoad.isDone)
            {
                progress?.Report(0.5f + (sceneLoad.progress * 0.3f));
                await Task.Yield(); // 等待一帧
            }
        
            // 等待场景初始化
            await Task.Delay(100);
        }

        progress?.Report(0.8f);

        // 应用各种数据
        await Task.Run(() => LoadPlayerData(saveData));
        await Task.Run(() => LoadNPCData(saveData));
        await Task.Run(() => LoadInventoryData(saveData));
        await Task.Run(() => LoadQuestData(saveData));
        await Task.Run(() => LoadGameStateData(saveData));
        // await Task.Run(() => LoadEnemyData(saveData));
        // await Task.Run(() => LoadNewsData(saveData));
        // await Task.Run(() => LoadPuzzleData(saveData));
    }

    #endregion

    #region 辅助方法

    /// <summary>
    /// 获取存档信息（异步）
    /// </summary>
    public static async Task<SaveDataInfo[]> GetSaveDataInfosAsync()
    {
        try
        {
            if (!Directory.Exists(SaveDirectory))
            {
                return new SaveDataInfo[0];
            }

            string[] saveFiles = await Task.Run(() => 
                Directory.GetFiles(SaveDirectory, "save_*.sav"));
        
            List<SaveDataInfo> saveInfos = new List<SaveDataInfo>();

            foreach (string filePath in saveFiles)
            {
                try
                {
                    string fileName = Path.GetFileName(filePath);
                    int slotIdx = int.Parse(fileName.Substring(5, fileName.Length - 9));
                
                    SaveData saveData = await ReadSaveFileAsync(filePath);
                    if (saveData != null)
                    {
                        saveInfos.Add(new SaveDataInfo
                        {
                            slotIndex = slotIdx,
                            saveName = saveData.saveName,
                            saveDate = saveData.saveDate,
                            sceneName = saveData.currentSceneName,
                            playerName = saveData.playerName,
                            gameVersion = saveData.gameVersion
                        });
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError($"读取存档信息失败: {e.Message}");
                }
            }

            return saveInfos.ToArray();
        }
        catch (Exception e)
        {
            Debug.LogError($"获取存档列表失败: {e.Message}");
            return new SaveDataInfo[0];
        }
    }

    #endregion
        
    // 原有的数据处理方法保持不变
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
        else
        {
            Debug.LogError("Player not found or PlayerManager is null. Skipping player data save.");
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
                npcSaveData.sceneName = npc.npcData.sceneName; // NPC所在场景
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
            List<NPCSaveData> npcSaveDataList = new List<NPCSaveData>(saveData.npcData.Values);
            foreach (var npcPair in saveData.npcData)
            {
                string npcID = npcPair.Key;
                NPCSaveData npcSaveData = npcPair.Value;
                npcSaveDataList.Add(npcSaveData);
            }
            NPCManager.Instance.InitializeNPCManager(npcSaveDataList); // Ensure NPCs are initialized
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
    }
}