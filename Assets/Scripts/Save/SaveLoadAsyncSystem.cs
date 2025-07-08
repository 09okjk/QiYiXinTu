using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Manager;
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
    }
    public class SaveLoadAsyncSystem:MonoBehaviour
    {
        private static SaveData saveData;
        public static SaveLoadAsyncSystem Instance { get; private set; }
        private static string SaveDirectory => Application.persistentDataPath + "/Saves/";
       
        #region 事件
        
        public static event Action<string> OnSaveComplete;
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
            // 创建保存数据对象
            SaveData saveData = new SaveData
            {
                saveName = "Save_" + DateTime.Now.ToString("yyyyMMdd_HHmmss"),
                saveTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                saveDateTime = DateTime.Now.ToFileTimeUtc(), // 保存时间戳
                saveSlotIndex = slotIdx, // 这里可以根据实际情况设置存档槽位索引
                gameVersion = Application.version,
                sceneName = SceneManager.GetActiveScene().name, // 获取当前场景名称
                saveType = saveType,
                PlayerGameData = PlayerManager.Instance.GetPlayerGameData(), // 获取玩家游戏数据
                NpcGameDatas = NPCManager.Instance.GetAllNPCData(), // 获取所有NPC数据
                itemIDs = InventoryManager.Instance.GetAllItemIDs(), // 获取所有物品ID列表
                currentQuest = QuestManager.Instance.currentQuest, // 获取当前任务数据
                allQuests = QuestManager.Instance.GetAllQuests(), // 获取所有任务数据
                currentDialogue = DialogueManager.Instance.GetCurrentDialogueData(), // 获取当前对话数据
                allDialogues = DialogueManager.Instance.GetAllDialogues() // 获取所有对话数据
            };
            
            // 写入文件
            string savePath = SaveDirectory + "save_" + slotIdx + ".sav";
            bool success = await WriteSaveFileAsync(saveData, savePath);
            
            OnSaveComplete?.Invoke(success ? "保存成功" : "保存失败");
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

        #region 加载逻辑

        public static async Task LoadGame(int slotIdx)
        {
            string savePath = SaveDirectory + "save_" + slotIdx + ".sav";
            if (!File.Exists(savePath))
            {
                Debug.LogError("保存文件不存在: " + savePath);
                return;
            }

            try
            {
                // 异步读取保存文件
                string jsonData = await File.ReadAllTextAsync(savePath, Encoding.UTF8);
                saveData = JsonConvert.DeserializeObject<SaveData>(jsonData);
                
                // 设置玩家数据
                PlayerManager.Instance.SetPlayerGameData(saveData.PlayerGameData);
                // 设置NPC数据
                NPCManager.Instance.SetNpcDatas(saveData.NpcGameDatas);
                // 设置物品数据
                InventoryManager.Instance.SetAllItemsByIDs(saveData.itemIDs);
                // 设置当前任务
                QuestManager.Instance.SetCurrentQuest(saveData.currentQuest);
                // 设置所有任务
                QuestManager.Instance.SetAllQuests(saveData.allQuests);
                // 设置当前对话
                DialogueManager.Instance.SetCurrentDialogueData(saveData.currentDialogue);
                // 设置所有对话
                DialogueManager.Instance.SetAllDialogues(saveData.allDialogues);
                
                Debug.Log("游戏加载完成: " + saveData.saveName);
                
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
    }
}