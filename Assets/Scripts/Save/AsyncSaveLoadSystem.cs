using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
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
        [SerializeField] private bool useGameManagerLoadingScreen = true; // 是否使用GameManager的加载屏幕
    
        public static AsyncSaveLoadSystem Instance { get; private set; }
    
        // 进度回调
        public static event Action<float> OnSaveProgress;
        public static event Action<float> OnLoadProgress;
        public static event Action<string> OnSaveComplete;
        public static event Action<string> OnLoadComplete;

        // ... 保持原有的数据结构定义不变 ...
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
                public List<QuestSaveData> allQuests = new List<QuestSaveData>();
                public string currentQuestID;
                
                // Dialogue data
                public List<DialogueSaveData> allDialogues = new List<DialogueSaveData>();
        
                // Enemy data
                public Dictionary<string, EnemySaveData> enemyData = new Dictionary<string, EnemySaveData>();
        
                // News data
                public Dictionary<string, bool> newsDictionary = new Dictionary<string, bool>();
        
                // Puzzle data
                public Dictionary<string, PuzzleSaveData> puzzleData = new Dictionary<string, PuzzleSaveData>();
        
                // Game state data
                public GameStateSaveData GameStateSaveData = new GameStateSaveData();
        
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
            
            [Serializable]
            public class QuestSaveData
            {
                public string questID;
                public string questName;
                public QuestCondition questConditionType; // 任务条件类型
                public string conditionValue; // 条件值
                public string questText; // 任务描述文本
                public string nextQuestID; // 下一个任务ID
                public bool isCompleted;
            }
            
            [Serializable]
            public class DialogueSaveData
            {
                public string dialogueID;
                public DialogueState dialogueState;
                public string currentNodeID;
            }
            
            [Serializable]
            public class GameStateSaveData
            {
                public Dictionary<string, bool> flags = new Dictionary<string, bool>();
                public float totalPlayTime;
                public DateTime gameStartTime;
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
                OnSaveProgress?.Invoke(0f);
            
                // 确保目录存在
                if (!Directory.Exists(SaveDirectory))
                {
                    Directory.CreateDirectory(SaveDirectory);
                }

                OnSaveProgress?.Invoke(0.1f);

                // 创建保存数据（可能耗时）
                SaveData saveData = await CreateSaveDataAsync(progress, slotIdx);
            
                OnSaveProgress?.Invoke(0.8f);

                // 写入文件
                string savePath = SaveDirectory + "save_" + slotIdx + ".sav";
                bool success = await WriteSaveFileAsync(saveData, savePath);
            
                OnSaveProgress?.Invoke(1f);
                
                if (Instance.useGameManagerLoadingScreen && GameManager.Instance != null)
                {
                    await Task.Delay(500); // 显示结果一段时间
                }
            
                OnSaveComplete?.Invoke(success ? "保存成功！" : "保存失败！");
                return success;
            }
            catch (Exception e)
            {
                Debug.LogError($"异步保存失败: {e.Message}");
                
                if (Instance.useGameManagerLoadingScreen && GameManager.Instance != null)
                {
                    await Task.Delay(1000);
                }
                
                OnSaveComplete?.Invoke("保存失败：" + e.Message);
                return false;
            }
        }
        
        // ... 保持原有的CreateSaveDataAsync和WriteSaveFileAsync方法不变 ...
        
        /// <summary>
        /// 异步创建保存数据
        /// </summary>
        private static async Task<SaveData> CreateSaveDataAsync(IProgress<float> progress = null, int slotIdx = 0)
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
            
            // TODO:其他数据收集（任务、新闻、谜题等）
            QuestDataCache questCache = CollectQuestData();
            DialogueDataCache dialogueCache = CollectDialogueData();
            NewsDataCache newsCache = CollectNewsData();
            GameStateDataCache gameStateCache = CollectGameStateData();
            // 切换到后台线程进行数据处理
            await Task.Run(() =>
            {
                ProcessPlayerData(saveData, playerCache);
                ProcessSceneData(saveData, sceneCache);
                ProcessNPCData(saveData, npcCache);
                ProcessInventoryData(saveData, inventoryCache);
                ProcessQuestData(saveData, questCache);
                ProcessDialogueData(saveData, dialogueCache);
                ProcessNewsData(saveData, newsCache);
                ProcessGameStateData(saveData, gameStateCache);
            });
    
            progress?.Report(0.8f);

            // 元数据（无需在异步线程处理）
            if(slotIdx == 0)
                saveData.saveName = "自动存档";
            else
                saveData.saveName = "存档 " + slotIdx;
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
                // 显示加载屏幕
                if (Instance.useGameManagerLoadingScreen && GameManager.Instance != null)
                {
                    GameManager.Instance.ShowLoadingScreen("正在加载游戏...");
                }
                
                progress?.Report(0f);
                OnLoadProgress?.Invoke(0f);
                
                if (Instance.useGameManagerLoadingScreen && GameManager.Instance != null)
                {
                    GameManager.Instance.UpdateLoadingProgress(0f, "正在加载游戏...");
                }
            
                string savePath = SaveDirectory + "save_" + slotIdx + ".sav";
            
                if (!File.Exists(savePath))
                {
                    Debug.LogWarning($"存档文件不存在: {savePath}");
                    
                    if (Instance.useGameManagerLoadingScreen && GameManager.Instance != null)
                    {
                        GameManager.Instance.UpdateLoadingProgress(1f, "存档文件不存在！");
                        await Task.Delay(1000);
                    }
                    
                    OnLoadComplete?.Invoke("存档文件不存在！");
                    return false;
                }

                progress?.Report(0.1f);
                OnLoadProgress?.Invoke(0.1f);
                
                if (Instance.useGameManagerLoadingScreen && GameManager.Instance != null)
                {
                    GameManager.Instance.UpdateLoadingProgress(0.1f, "读取存档文件...");
                }

                // 读取文件
                SaveData saveData = await ReadSaveFileAsync(savePath);
                if (saveData == null)
                {
                    if (Instance.useGameManagerLoadingScreen && GameManager.Instance != null)
                    {
                        GameManager.Instance.UpdateLoadingProgress(1f, "存档文件损坏！");
                        await Task.Delay(1000);
                    }
                    
                    OnLoadComplete?.Invoke("存档文件损坏！");
                    return false;
                }

                progress?.Report(0.5f);
                OnLoadProgress?.Invoke(0.5f);
                
                if (Instance.useGameManagerLoadingScreen && GameManager.Instance != null)
                {
                    GameManager.Instance.UpdateLoadingProgress(0.5f, "应用游戏数据...");
                }
                Debug.Log($"saveData.GameStateSaveData.flags[FirstEntry_女生宿舍]: {saveData.GameStateSaveData.flags["FirstEntry_女生宿舍"]}");
                // 应用数据
                await ApplySaveDataAsync(saveData, progress);
            
                progress?.Report(1f);
                OnLoadProgress?.Invoke(1f);
                
                if (Instance.useGameManagerLoadingScreen && GameManager.Instance != null)
                {
                    GameManager.Instance.UpdateLoadingProgress(1f, "加载完成！");
                    await Task.Delay(500);
                }
                
                OnLoadComplete?.Invoke("加载完成！");
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"异步加载失败: {e.Message}");
                
                if (Instance.useGameManagerLoadingScreen && GameManager.Instance != null)
                {
                    GameManager.Instance.UpdateLoadingProgress(1f, "加载失败！");
                    await Task.Delay(1000);
                }
                
                OnLoadComplete?.Invoke("加载失败：" + e.Message);
                return false;
            }
        }

        // ... 保持原有的ReadSaveFileAsync方法不变 ...
        
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
            if (saveData == null)
            {
                Debug.LogError("无法应用空的存档数据");
                return;
            }
            
            // 检查是否需要切换场景
            string currentScene = SceneManager.GetActiveScene().name;
            if (currentScene != saveData.currentSceneName || SceneManager.GetActiveScene().name == "MainMenu")
            {
                if (Instance.useGameManagerLoadingScreen && GameManager.Instance != null)
                {
                    GameManager.Instance.UpdateLoadingProgress(0.5f, $"正在切换到场景: {saveData.currentSceneName}");
                }
                
                AsyncOperation sceneLoad;

                GameStateManager.Instance.SetFlag("UseSaveLoadingScene", true);
                if (SceneManager.GetActiveScene().name == "MainMenu" && saveData.GameStateSaveData.flags["FirstEntry_女生宿舍"])
                {
                    Debug.Log($"加载女生宿舍场景，Scene.name:{SceneManager.GetActiveScene().name}, FirstEntry_女生宿舍: {GameStateManager.Instance.GetFlag("FirstEntry_女生宿舍")}");
                    sceneLoad = SceneManager.LoadSceneAsync("女生宿舍");
                }
                else
                    sceneLoad = SceneManager.LoadSceneAsync(saveData.currentSceneName);
            
                while (sceneLoad is { isDone: false })
                {
                    float sceneProgress = 0.5f + (sceneLoad.progress * 0.3f);
                    progress?.Report(sceneProgress);
                    OnLoadProgress?.Invoke(sceneProgress);
                    
                    if (Instance.useGameManagerLoadingScreen && GameManager.Instance != null)
                    {
                        GameManager.Instance.UpdateLoadingProgress(sceneProgress, 
                            $"正在切换到场景: {saveData.currentSceneName} ({sceneLoad.progress:P0})");
                    }
                    
                    await Task.Yield(); // 等待一帧
                }
            
                // 等待场景初始化
                await Task.Delay(100);
            }

            progress?.Report(0.8f);
            OnLoadProgress?.Invoke(0.8f);
            
            if (Instance.useGameManagerLoadingScreen && GameManager.Instance != null)
            {
                GameManager.Instance.UpdateLoadingProgress(0.8f, "应用玩家数据...");
            }

            try
            {
                // 直接在主线程调用所有加载方法，不使用Task.Run
                LoadPlayerData(saveData);
                await Task.Yield(); // 确保UI可以更新
                progress?.Report(0.85f);
                OnLoadProgress?.Invoke(0.85f);

                if (Instance.useGameManagerLoadingScreen && GameManager.Instance != null)
                {
                    GameManager.Instance.UpdateLoadingProgress(0.85f, "应用NPC数据...");
                }

                LoadNPCData(saveData);
                await Task.Yield();
                progress?.Report(0.9f);
                OnLoadProgress?.Invoke(0.9f);

                if (Instance.useGameManagerLoadingScreen && GameManager.Instance != null)
                {
                    GameManager.Instance.UpdateLoadingProgress(0.9f, "应用物品数据...");
                }

                LoadInventoryData(saveData);
                await Task.Yield();
                progress?.Report(0.95f);
                OnLoadProgress?.Invoke(0.95f);

                if (Instance.useGameManagerLoadingScreen && GameManager.Instance != null)
                {
                    GameManager.Instance.UpdateLoadingProgress(0.95f, "应用游戏状态...");
                }

                LoadQuestData(saveData);
                LoadDialogueData(saveData);
                LoadGameStateData(saveData);
                LoadNewsData(saveData);

                progress?.Report(1f);
                OnLoadProgress?.Invoke(1f);
            }
            catch (Exception e)
            {
                Debug.LogError($"应用存档数据失败: {e.Message}");
                throw;
            }
        }

        #endregion

        // ... 保持原有的所有辅助方法、缓存类和数据处理方法不变 ...
        
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

                // 按照存档索引排序
                saveInfos.Sort((a, b) => a.slotIndex.CompareTo(b.slotIndex));
                return saveInfos.ToArray();
            }
            catch (Exception e)
            {
                Debug.LogError($"获取存档列表失败: {e.Message}");
                return new SaveDataInfo[0];
            }
        }
        
        // 异步删除存档文件
        public static async Task<bool> DeleteSaveFileAsync(int slotIdx)
        {
            try
            {
                string savePath = SaveDirectory + "save_" + slotIdx + ".sav";
                if (File.Exists(savePath))
                {
                    await Task.Run(() => File.Delete(savePath));
                    Debug.Log($"存档 {slotIdx} 已删除");
                    return true;
                }
                Debug.LogWarning($"存档文件不存在: {savePath}");
                return false;
            }
            catch (Exception e)
            {
                Debug.LogError($"删除存档失败: {e.Message}");
                return false;
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
                foreach (var npc in NPCManager.Instance.GetActiveNPCs())
                {
                    cache.npcData[npc.npcData.npcID] = (
                        npc.npcData.npcID,
                        npc.transform.position,
                        npc.gameObject.activeSelf,
                        npc.isFollowing,
                        npc.npcData.sceneName,
                        npc.canInteract
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
        
        private static QuestDataCache CollectQuestData()
        {
            QuestDataCache cache = new QuestDataCache();
            if (QuestManager.Instance != null)
            {
                cache.allQuests = QuestManager.Instance.GetAllQuests();
            }
            return cache;
        }
        
        private static DialogueDataCache CollectDialogueData()
        {
            DialogueDataCache cache = new DialogueDataCache();
            if (DialogueManager.Instance != null)
            {
                cache.allDialogues = DialogueManager.Instance.GetDialogueDataDictionary().Values.ToList();
            }
            return cache;
        }
        
        private static NewsDataCache CollectNewsData()
        {
            NewsDataCache cache = new NewsDataCache();
            if (NewsManager.Instance != null)
            {
                cache.allNews = NewsManager.Instance.GetNewsDatas();
            }
            return cache;
        }
        
        private static GameStateDataCache CollectGameStateData()
        {
            GameStateDataCache cache = new GameStateDataCache();
            if (GameStateManager.Instance != null)
            {
                cache.flags = GameStateManager.Instance.GetAllFlags();
                cache.totalPlayTime = 1f;
                cache.gameStartTime = DateTime.Today;
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
                var (npcID, position, isActive, isFollowing, sceneName,canInteract) = npcEntry.Value;

                NPCSaveData npcSaveData = new NPCSaveData();
                npcSaveData.npcID = npcID;
                npcSaveData.sceneName = sceneName;
                npcSaveData.position[0] = position.x;
                npcSaveData.position[1] = position.y;
                npcSaveData.position[2] = position.z;
                npcSaveData.isActive = isActive;
                npcSaveData.isFollowing = isFollowing;
                npcSaveData.canInteract = canInteract;
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

        private static void ProcessQuestData(SaveData saveData, QuestDataCache cache)
        {
            saveData.allQuests.Clear();
            foreach (var cacheAllQuest in cache.allQuests)
            {
                QuestSaveData questSaveData = new QuestSaveData
                {
                    questID = cacheAllQuest.questID,
                    questName = cacheAllQuest.questName,
                    questConditionType = cacheAllQuest.questConditionType,
                    conditionValue = cacheAllQuest.conditionValue,
                    questText = cacheAllQuest.questText,
                    nextQuestID = cacheAllQuest.nextQuestID,
                    isCompleted = cacheAllQuest.isCompleted
                };
                saveData.allQuests.Add(questSaveData);
            }
            saveData.currentQuestID = QuestManager.Instance.currentQuestID;
        }
        
        private static void ProcessDialogueData(SaveData saveData, DialogueDataCache cache)
        {
            saveData.allDialogues.Clear();
            foreach (var dialogue in cache.allDialogues)
            {
                DialogueSaveData dialogueSaveData = new DialogueSaveData
                {
                    dialogueID = dialogue.dialogueID,
                    dialogueState = dialogue.state,
                    currentNodeID = dialogue.currentNodeID
                };
                saveData.allDialogues.Add(dialogueSaveData);
            }
        }

        private static void ProcessNewsData(SaveData saveData, NewsDataCache cache)
        {
            saveData.newsDictionary.Clear();
            saveData.newsDictionary = new Dictionary<string, bool>(cache.allNews);
        }

        private static void ProcessGameStateData(SaveData saveData, GameStateDataCache cache)
        {
            saveData.GameStateSaveData = new GameStateSaveData
            {
                flags = new Dictionary<string, bool>(cache.flags),
                totalPlayTime = cache.totalPlayTime,
                gameStartTime = cache.gameStartTime,
            };
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
            Debug.Log("LoadPlayerData");
        }

        private static void LoadSceneData(SaveData saveData)
        {
            GameStateManager.Instance.SetPlayerPointType(saveData.lastPlayerPointType);
        }

        private static void LoadNPCData(SaveData saveData)
        {
            if (NPCManager.Instance != null && saveData.npcData != null)
            {
                // 创建仅一次的NPC数据列表
                List<NPCSaveData> npcSaveDataList = new List<NPCSaveData>(saveData.npcData.Values);
                
                // 直接使用已创建的列表
                Debug.Log($"加载了 {npcSaveDataList.Count} 个NPC数据");
                NPCManager.Instance.InitializeNPCManager(npcSaveDataList);
            }
            else
            {
                Debug.LogWarning("NPC加载过程中，NPCManager实例或NPC数据为空");
            }
            Debug.Log("LoadNPCData");
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
            Debug.Log("LoadInventoryData");
        }

        private static void LoadDialogueData(SaveData saveData)
        {
            if (DialogueManager.Instance != null)
            {
                DialogueManager.Instance.LoadDialogueData(saveData.allDialogues);
            }
        }

        private static void LoadQuestData(SaveData saveData)
        {
            if (QuestManager.Instance != null)
            {
                QuestManager.Instance.LoadAllQuests(saveData.allQuests);
                QuestManager.Instance.currentQuestID = saveData.currentQuestID;
            }
            Debug.Log("LoadQuestData");
        }

        private static void LoadGameStateData(SaveData saveData)
        {
            if (GameStateManager.Instance != null)
            {
                GameStateManager.Instance.SetAllFlags(saveData.GameStateSaveData.flags);
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
                NewsManager.Instance.ApplyNewsDatas(saveData.newsDictionary);
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
    public Dictionary<string, (string npcID, Vector3 position, bool activeSelf, bool isFollowing, string sceneName, bool canInteract)> npcData = 
        new Dictionary<string, (string, Vector3, bool, bool, string,bool)>();
}

class InventoryDataCache
{
    public List<ItemData> allItems = new List<ItemData>();
}

class QuestDataCache
{
    public List<QuestData> allQuests = new List<QuestData>();
}

class DialogueDataCache
{
    public List<DialogueData> allDialogues = new List<DialogueData>();
}

class NewsDataCache
{
    public Dictionary<string, bool> allNews = new Dictionary<string, bool>();
}

class GameStateDataCache
{
    public Dictionary<string, bool> flags = new Dictionary<string, bool>();
    public float totalPlayTime;
    public DateTime gameStartTime;
}