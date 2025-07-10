using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Audio;
using Manager;
using News;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

namespace Save
{
    [Serializable]
    public class SaveData
    {
        public string saveName;
        public string saveTime;
        public float saveDateTime; // 保存时间戳
        public int saveSlotIndex; // 用于标识存档槽位
        public string gameVersion; // 游戏版本号
        public string sceneName; // 当前场景名称
        public bool saveType; // true for quick save, false for normal save
        public PlayerGameData PlayerGameData;
        public Dictionary<string, NpcGameData> NpcGameDatas = new();
        public List<string> itemIDs = new(); // 存储物品ID列表
        public QuestGameData currentQuest; // 当前任务数据
        public Dictionary<string, QuestGameData> allQuests = new(); // 所有任务数据
        public DialogueGameData currentDialogue;
        public Dictionary<string, DialogueGameData> allDialogues = new(); // 所有对话数据
        public Dictionary<string, NewsGameData> allNewsData = new(); // 所有新闻数据
        public Dictionary<string, bool> allGameFlags = new(); // 游戏状态标志
        public AudioGameData audioGameData; // 音频数据（如果需要保存音频状态）
    }
    public class SaveLoadAsyncSystem:MonoBehaviour
    {
        private static SaveData saveData;
        public static SaveLoadAsyncSystem Instance { get; private set; }
        private static string SaveDirectory => Application.persistentDataPath + "/Saves/";
       
        #region 事件
        
        // public static event Action<string> OnSaveComplete;
        public static event Action<string> OnLoadComplete;

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
        
        private void Start()
        {
            // 确保保存目录存在
            if (!Directory.Exists(SaveDirectory))
            {
                Directory.CreateDirectory(SaveDirectory);
            }
        }

        #region 保存逻辑
        
        // 保存游戏数据
        public static async Task SaveGame(bool saveType,int slotIdx)
        {
            Debug.Log("SaveGame");
            // 创建保存数据对象
            try
            {
                SaveData saveData = new SaveData
                {
                    saveName = "Save_" + DateTime.Now.ToString("yyyyMMdd_HHmmss"),
                    saveTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                    saveDateTime = DateTime.Now.ToFileTimeUtc(), // 保存时间戳
                    saveSlotIndex = slotIdx, // 这里可以根据实际情况设置存档槽位索引
                    gameVersion = Application.version,
                    sceneName = SceneManager.GetActiveScene().name, // 获取当前场景名称
                    saveType = saveType,
                    PlayerGameData = PlayerManager.Instance?.GetPlayerGameData(), // 获取玩家游戏数据
                    NpcGameDatas = NPCManager.Instance?.GetAllNPCData() ?? new Dictionary<string, NpcGameData>(), // 获取所有NPC数据
                    itemIDs = InventoryManager.Instance?.GetAllItemIDs() ?? new List<string>(), // 获取所有物品ID列表
                    currentQuest = QuestManager.Instance?.currentQuest, // 获取当前任务数据
                    allQuests = QuestManager.Instance?.GetAllQuests() ?? new Dictionary<string, QuestGameData>(), // 获取所有任务数据
                    currentDialogue = DialogueManager.Instance?.GetCurrentDialogueData(), // 获取当前对话数据
                    allDialogues = DialogueManager.Instance?.GetAllDialogues() ?? new Dictionary<string, DialogueGameData>(), // 获取所有对话数据
                    allNewsData = NewsManager.Instance?.GetAllNewsData() ?? new Dictionary<string, NewsGameData>(), // 获取所有新闻数据
                    allGameFlags = GameStateManager.Instance?.GetAllFlags() ?? new Dictionary<string, bool>(), // 获取所有游戏状态标志
                    audioGameData = AudioManager.Instance?.GetAudioGameData() // 获取音频数据
                };
                
                // 写入文件
                string savePath = SaveDirectory + "save_" + slotIdx + ".sav";
            
                await WriteSaveFileAsync(saveData, savePath);
                
                GameManager.Instance?.OnGameEvent("SaveGameComplete");
            }
            catch (Exception e)
            {
                Debug.LogError($"创建保存数据失败: {e.Message}\n{e.StackTrace}");
            }
        }
        
        /// <summary>
        /// 异步写入保存文件
        /// </summary>
        private static async Task<bool> WriteSaveFileAsync(SaveData saveData, string savePath)
        {
            try
            {
                Debug.Log("WriteSaveFileAsync");
                
                // 配置JSON序列化设置来处理循环引用
                var settings = new JsonSerializerSettings
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    Formatting = Formatting.Indented,
                    ContractResolver = new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver()
                };
                
                // 以JSON格式保存（使用UTF-8编码，带格式）
                string jsonData = JsonConvert.SerializeObject(saveData, settings);
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

        #region 加载逻辑

        public static async Task LoadGame()
        {
            // 异步加载最新的存档
            string[] files = Directory.GetFiles(SaveDirectory, "*.sav");
            if (files.Length == 0)
            {
                Debug.LogWarning("没有找到任何存档文件");
                return;
            }

            // 获取最新的存档文件
            string latestFile = files[0];
            foreach (string file in files)
            {
                if (File.GetLastWriteTime(file) > File.GetLastWriteTime(latestFile))
                {
                    latestFile = file;
                }
            }
            await SetGameData(latestFile);
        }
        
        public static async Task LoadGame(int slotIdx)
        {
            if (slotIdx < 0)
            {
                await LoadGame().ConfigureAwait(false);
                return;
            }
            string savePath = SaveDirectory + "save_" + slotIdx + ".sav";
            if (!File.Exists(savePath))
            {
                Debug.LogError("保存文件不存在: " + savePath);
                return;
            }
            await SetGameData(savePath);
        }

        private static async Task SetGameData(string savePath)
        {
            try
            {
                // 异步读取保存文件
                string jsonData = await File.ReadAllTextAsync(savePath, Encoding.UTF8);
                saveData = JsonConvert.DeserializeObject<SaveData>(jsonData);
                
                // 设置玩家数据
                if (PlayerManager.Instance.SetPlayerGameData(saveData.PlayerGameData))
                {
                    Debug.Log("玩家数据加载成功: " + saveData.saveName);
                    GameManager.Instance.UpdateLoadingProgress(0.1f,"玩家数据加载成功"); // 更新加载进度
                }
                
                // 设置NPC数据
                if (NPCManager.Instance.SetNpcDatas(saveData.NpcGameDatas))
                {
                    Debug.Log("NPC数据加载成功: " + saveData.saveName);
                    GameManager.Instance.UpdateLoadingProgress(0.2f,"NPC数据加载成功"); // 更新加载进度
                }
                
                // 设置物品数据
                if (InventoryManager.Instance.SetAllItemsByIDs(saveData.itemIDs))
                {
                    Debug.Log("物品数据加载成功: " + saveData.saveName);
                    GameManager.Instance.UpdateLoadingProgress(0.3f,"物品数据加载成功"); // 更新加载进度 
                }
                
                // 设置当前任务
                QuestManager.Instance.SetCurrentQuest(saveData.currentQuest);
                // 设置所有任务
                if (QuestManager.Instance.SetAllQuests(saveData.allQuests))
                {
                    Debug.Log("任务数据加载成功: " + saveData.saveName);
                    GameManager.Instance.UpdateLoadingProgress(0.4f,"任务数据加载成功"); // 更新加载进度
                }
                
                // 设置当前对话
                DialogueManager.Instance.SetCurrentDialogueData(saveData.currentDialogue);
                // 设置所有对话
                if (DialogueManager.Instance.SetAllDialogues(saveData.allDialogues))
                {
                    Debug.Log("对话数据加载成功: " + saveData.saveName);
                    GameManager.Instance.OnGameEvent("DialogueManagerReady");
                    GameManager.Instance.UpdateLoadingProgress(0.5f,"对话数据加载成功"); // 更新加载进度
                }
                
                // 设置所有新闻数据
                if (NewsManager.Instance.SetAllNewsData(saveData.allNewsData))
                {
                    Debug.Log("新闻数据加载成功: " + saveData.saveName);
                    GameManager.Instance.UpdateLoadingProgress(0.6f,"新闻数据加载成功"); // 更新加载进度
                }
                
                // 设置游戏状态标志
                if (GameStateManager.Instance.SetAllFlags(saveData.allGameFlags))
                {
                    Debug.Log("游戏状态标志加载成功: " + saveData.saveName);
                    GameManager.Instance.UpdateLoadingProgress(0.7f,"游戏状态标志加载成功"); // 更新加载进度
                }
                
                // 设置音频数据
                if (AudioManager.Instance.SetAudioGameData(saveData.audioGameData))
                {
                    Debug.Log("音频数据加载成功: " + saveData.saveName);
                    GameManager.Instance.UpdateLoadingProgress(0.7f,"音频数据加载成功"); // 更新加载进度
                }
                
                Debug.Log("游戏数据加载完成: " + saveData.saveName);
                
                OnLoadComplete?.Invoke("加载成功: " + saveData.saveName);
            }
            catch (Exception e)
            {
                Debug.LogError($"加载保存文件失败: {e.Message}");
            }
        }

        #endregion
        
        #region 删除存档
        
        public static void DeleteSave(int slotIdx)
        {
            string savePath = SaveDirectory + "save_" + slotIdx + ".sav";
            if (File.Exists(savePath))
            {
                try
                {
                    File.Delete(savePath);
                    Debug.Log("存档已删除: " + savePath);
                }
                catch (Exception e)
                {
                    Debug.LogError($"删除存档失败: {e.Message}");
                }
            }
            else
            {
                Debug.LogWarning("尝试删除不存在的存档: " + savePath);
            }
        }
        
        #endregion
        
        public static Task<List<SaveData>> GetAllSaves()
        {
            List<SaveData> saves = new List<SaveData>();
            string[] files = Directory.GetFiles(SaveDirectory, "*.sav");
            foreach (string file in files)
            {
                try
                {
                    string jsonData = File.ReadAllText(file, Encoding.UTF8);
                    SaveData save = JsonConvert.DeserializeObject<SaveData>(jsonData);
                    saves.Add(save);
                }
                catch (Exception e)
                {
                    Debug.LogError($"读取保存文件失败: {e.Message}");
                }
            }
            return Task.FromResult(saves);
        }

        // 获取最新的存档的levelname
        public static string GetNewestSaveLevelName()
        {
            string[] files = Directory.GetFiles(SaveDirectory, "*.sav");
            if (files.Length == 0)
            {
                Debug.LogWarning("没有找到任何存档文件");
                return null;
            }

            // 获取最新的存档文件
            string latestFile = files[0];
            foreach (string file in files)
            {
                if (File.GetLastWriteTime(file) > File.GetLastWriteTime(latestFile))
                {
                    latestFile = file;
                }
            }

            try
            {
                string jsonData = File.ReadAllText(latestFile, Encoding.UTF8);
                SaveData saveData = JsonConvert.DeserializeObject<SaveData>(jsonData);
                return saveData.sceneName;
            }
            catch (Exception e)
            {
                Debug.LogError($"读取最新存档失败: {e.Message}");
                return null;
            }
        }
    }
}