using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using UnityEngine;
using Manager;
using News;
using Newtonsoft.Json;
using UnityEngine.SceneManagement;
using Formatting = Newtonsoft.Json.Formatting;

namespace Save
{
    public class AsyncSaveLoadSystem : MonoBehaviour
    {
        private static string SaveDirectory;
    
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
            SaveDirectory = Application.persistentDataPath + "/Saves/";
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
            progress?.Report(0.1f);

            // 在主线程收集所有Unity API需要的数据
            PlayerDataCache playerCache = CollectPlayerData();
            progress?.Report(0.2f);

            SceneDataCache sceneCache = CollectSceneData();
            progress?.Report(0.3f);
    
            NPCDataCache npcCache = CollectNPCData();
            progress?.Report(0.4f);
    
            InventoryDataCache inventoryCache = CollectInventoryData();
            progress?.Report(0.5f);

            // 切换到后台线程进行数据处理
            await Task.Run(() =>
            {
                ProcessPlayerData(saveData, playerCache);
                ProcessSceneData(saveData, sceneCache);
                ProcessNPCData(saveData, npcCache);
                ProcessInventoryData(saveData, inventoryCache);
                ProcessQuestData(saveData);
                ProcessGameStateData(saveData);
            });
    
            progress?.Report(0.8f);

            // 元数据（无需在异步线程处理）
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
                // 以JSON格式保存（使用UTF-8编码，带格式）
                string jsonData = JsonConvert.SerializeObject(saveData, Formatting.Indented);
                await File.WriteAllTextAsync(savePath, jsonData, Encoding.UTF8);
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
                // 直接以文本方式读取文件
                string jsonData = await File.ReadAllTextAsync(savePath);
        
                // 检查是否为空
                if (string.IsNullOrEmpty(jsonData))
                {
                    Debug.LogWarning("保存文件为空");
                    return null;
                }

                // 反序列化JSON数据
                return JsonConvert.DeserializeObject<SaveData>(jsonData);
            }
            catch (Exception e)
            {
                Debug.LogError($"读取文件失败: {e.Message}");
        
                // 如果JSON解析失败，尝试二进制格式
                try
                {
                    using (FileStream fs = new FileStream(savePath, FileMode.Open, FileAccess.Read))
                    {
                        BinaryFormatter formatter = new BinaryFormatter();
                        return (SaveData)formatter.Deserialize(fs);
                    }
                }
                catch (Exception binaryEx)
                {
                    Debug.LogError($"二进制读取失败: {binaryEx.Message}");
                    return null;
                }
            }
        }
        
        /// <summary>
        /// 异步应用保存数据
        /// </summary>
        private static async Task ApplySaveDataAsync(SaveData saveData, IProgress<float> progress = null)
        {
            // 检查是否需要切换场景
            string currentScene = SceneManager.GetActiveScene().name;
            if (currentScene != saveData.currentSceneName || SceneManager.GetActiveScene().name == "MainMenu")
            {
                AsyncOperation sceneLoad;
             
                if (SceneManager.GetActiveScene().name == "MainMenu")
                    sceneLoad = SceneManager.LoadSceneAsync("女生宿舍");
                else
                    sceneLoad = SceneManager.LoadSceneAsync(saveData.currentSceneName);
            
                while (sceneLoad is { isDone: false })
                {
                    progress?.Report(0.5f + (sceneLoad.progress * 0.3f));
                    await Task.Yield(); // 等待一帧
                }
            
                // 等待场景初始化
                await Task.Delay(100);
            }

            progress?.Report(0.8f);

            // 直接在主线程调用所有加载方法，不使用Task.Run
            LoadPlayerData(saveData);
            await Task.Yield(); // 确保UI可以更新
            progress?.Report(0.85f);
    
            LoadNPCData(saveData);
            await Task.Yield();
            progress?.Report(0.9f);
    
            LoadInventoryData(saveData);
            await Task.Yield();
            progress?.Report(0.95f);
    
            LoadQuestData(saveData);
            LoadGameStateData(saveData);
            // 其他加载方法...
            // await Task.Run(() => LoadEnemyData(saveData));
            // await Task.Run(() => LoadNewsData(saveData));
            // await Task.Run(() => LoadPuzzleData(saveData));
    
            progress?.Report(1f);
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
                // 确保 SaveDirectory 已被初始化
                if (string.IsNullOrEmpty(SaveDirectory))
                {
                    SaveDirectory = Application.persistentDataPath + "/Saves/";
                }
                
                // 在主线程中获取路径信息
                string saveDir = SaveDirectory;
            
                if (!Directory.Exists(saveDir))
                {
                    Directory.CreateDirectory(saveDir);
                    return new SaveDataInfo[0];
                }

                // 使用已获取的路径进行异步文件操作
                string[] saveFiles = await Task.Run(() => 
                    Directory.GetFiles(saveDir, "save_*.sav"));

                List<SaveDataInfo> saveInfos = new List<SaveDataInfo>();

                foreach (string filePath in saveFiles)
                {
                    try
                    {
                        Debug.Log("读取存档文件: " + filePath);
                        string fileName = Path.GetFileName(filePath);
                        int slotIdx = int.Parse(fileName.Substring(5, fileName.Length - 9));
                        Debug.Log("文件名: " + fileName + ", 插槽索引: " + slotIdx);
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
        
        // 检查文件是否为JSON格式
        private static async Task<bool> IsJsonFileAsync(Stream filePath)
        {
            try
            {
                // 简单读取文件的前100个字符来检查
                using (StreamReader reader = new StreamReader(filePath, Encoding.UTF8, true, 1024, true))
                {
                    char[] buffer = new char[100];
                    await reader.ReadAsync(buffer, 0, 100);
                    string content = new string(buffer);
            
                    // 简单检查是否包含JSON的开头字符
                    return content.TrimStart().StartsWith("{") || content.TrimStart().StartsWith("[");
                }
            }
            catch
            {
                return false;
            }
        }

        // 在主线程收集数据的辅助方法
        private static PlayerDataCache CollectPlayerData()
        {
            PlayerDataCache cache = new PlayerDataCache();
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null && PlayerManager.Instance != null)
            {
                cache.position = player.transform.position;
                cache.playerName = PlayerManager.Instance.player.playerData.playerName;
                cache.health = PlayerManager.Instance.player.playerData.CurrentHealth;
                cache.mana = PlayerManager.Instance.player.playerData.CurrentMana;
                
                // 如果需要收集额外的技能数据
                // cache.skills = PlayerManager.Instance.player.playerData.skills;
            }
            return cache;
        }

        private static SceneDataCache CollectSceneData()
        {
            SceneDataCache cache = new SceneDataCache();
            cache.sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            cache.pointType = GameStateManager.Instance.GetPlayerPointType();
            return cache;
        }

        private static NPCDataCache CollectNPCData()
        {
            NPCDataCache cache = new NPCDataCache();
            if (NPCManager.Instance != null)
            {
                foreach (GameObject npcObject in NPCManager.Instance.npcGameObjectList)
                {
                    var npc = npcObject.GetComponent<NPC>();
                    cache.npcData[npc.npcData.npcID] = (
                        npc.npcData.npcID,
                        npc.transform.position,
                        npc.gameObject.activeSelf,
                        npc.isFollowing,
                        npc.npcData.sceneName
                    );
                }
            }
            return cache;
        }

        private static InventoryDataCache CollectInventoryData()
        {
            InventoryDataCache cache = new InventoryDataCache();
            if (InventoryManager.Instance != null)
            {
                cache.allItems = InventoryManager.Instance.GetAllItems();
            }
            return cache;
        }

        // 在后台线程处理数据的方法
        private static void ProcessPlayerData(SaveData saveData, PlayerDataCache cache)
        {
            saveData.playerPosition[0] = cache.position.x;
            saveData.playerPosition[1] = cache.position.y;
            saveData.playerPosition[2] = cache.position.z;
            saveData.playerName = cache.playerName;
            saveData.playerHealth = cache.health;
            saveData.playerMana = cache.mana;
            
            // 如果需要处理技能数据
            // saveData.playerSkills = new Dictionary<string, int>(cache.skills);
        }

        private static void ProcessSceneData(SaveData saveData, SceneDataCache cache)
        {
            saveData.currentSceneName = cache.sceneName;
            saveData.lastPlayerPointType = cache.pointType;
        }

        private static void ProcessNPCData(SaveData saveData, NPCDataCache cache)
        {
            foreach (var npcEntry in cache.npcData)
            {
                var (npcID, position, isActive, isFollowing, sceneName) = npcEntry.Value;

                NPCSaveData npcSaveData = new NPCSaveData();
                npcSaveData.npcID = npcID;
                npcSaveData.sceneName = sceneName;
                npcSaveData.position[0] = position.x;
                npcSaveData.position[1] = position.y;
                npcSaveData.position[2] = position.z;
                npcSaveData.isActive = isActive;
                npcSaveData.isFollowing = isFollowing;

                // 从GameStateManager获取标志 - 这里需要确保GameStateManager.GetAllFlags()是线程安全的
                foreach (var flag in GameStateManager.Instance.GetAllFlags())
                {
                    if (flag.Key.Contains(npcID))
                    {
                        npcSaveData.npcFlags[flag.Key] = flag.Value;
                    }
                }

                saveData.npcData[npcID] = npcSaveData;
            }
        }

        private static void ProcessInventoryData(SaveData saveData, InventoryDataCache cache)
        {
            foreach (ItemData item in cache.allItems)
            {
                if (item.itemType == ItemType.QuestItem)
                {
                    saveData.questItems.Add(item.itemID);
                }
                else if (item.itemType == ItemType.PuzzleItem)
                {
                    saveData.puzzleItems.Add(item.itemID);
                }

                // 保存物品数量
                saveData.itemQuantities[item.itemID] = 1; // 根据你的库存系统修改
            }
        }

        private static void ProcessQuestData(SaveData saveData)
        {
            // 确保不调用Unity API
            if (QuestManager.Instance != null)
            {
                // 当前任务
                if (!string.IsNullOrEmpty(QuestManager.Instance.currentQuestID))
                {
                    saveData.currentQuestID = QuestManager.Instance.currentQuestID;
                    saveData.activeQuestIDs.Add(QuestManager.Instance.currentQuestID);
                }

                // 已完成的任务数据处理
            }
        }

        private static void ProcessGameStateData(SaveData saveData)
        {
            // 确保不调用Unity API
            if (GameStateManager.Instance != null)
            {
                saveData.flags = GameStateManager.Instance.GetAllFlags();
            }
        }
        #endregion
            

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

public class SaveDataInfo
{
    public int slotIndex;
    public string saveName;
    public DateTime saveDate;
    public string sceneName;
    public string playerName;
    public string gameVersion;
}

// 缓存类定义 - 添加在类的顶部
class PlayerDataCache
{
    public Vector3 position;
    public string playerName;
    public int health;
    public float mana;
    public Dictionary<string, int> skills = new Dictionary<string, int>();
}

class SceneDataCache
{
    public string sceneName;
    public PlayerPointType pointType;
}

class NPCDataCache
{
    public Dictionary<string, (string npcID, Vector3 position, bool isActive, bool isFollowing, string sceneName)> npcData = 
        new Dictionary<string, (string, Vector3, bool, bool, string)>();
}

class InventoryDataCache
{
    public List<ItemData> allItems = new List<ItemData>();
}