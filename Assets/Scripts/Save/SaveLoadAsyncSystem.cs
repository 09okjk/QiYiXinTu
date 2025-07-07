using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Manager;
using Newtonsoft.Json;
using UnityEngine;

namespace Save
{
    public class SaveLoadAsyncSystem:MonoBehaviour
    {
        public class SaveData
        {
            public string SaveName;
            public string SaveTime;
            public int SaveSlotIndex; // 用于标识存档槽位
            public string GameVersion; // 游戏版本号
            public bool SaveType; // true for quick save, false for normal save
            public PlayerGameData PlayerGameData;
        }
        
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
        public static async void SaveGame(bool saveType,int slotIdx)
        {
            // 创建保存数据对象
            SaveData saveData = new SaveData
            {
                SaveName = "Save_" + DateTime.Now.ToString("yyyyMMdd_HHmmss"),
                SaveTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                SaveSlotIndex = slotIdx, // 这里可以根据实际情况设置存档槽位索引
                GameVersion = Application.version,
                SaveType = saveType,
                PlayerGameData = PlayerManager.Instance.GetPlayerGameData() // 获取玩家游戏数据
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

        public static async void LoadGame(int slotIdx)
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
                SaveData saveData = JsonConvert.DeserializeObject<SaveData>(jsonData);
                
                // 设置玩家数据
                PlayerManager.Instance.SetPlayerGameData(saveData.PlayerGameData);
                
                Debug.Log("游戏加载完成: " + saveData.SaveName);
                
                OnLoadComplete?.Invoke("加载成功: " + saveData.SaveName);
            }
            catch (Exception e)
            {
                Debug.LogError($"加载保存文件失败: {e.Message}");
            }
        }

        #endregion
    }
}