using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

public class SaveLoadSystem : MonoBehaviour
{
    private static string SaveDirectory => Application.persistentDataPath + "/Saves/";
    private static int slotIndex = 0; // 添加一个静态字段来保存当前使用的存档槽

    [Serializable]
    public class SaveData
    {
        // Player data
        public float playerHealth;
        public float playerMana;
        public float[] playerPosition = new float[3];
        public string currentSceneName;

        // Inventory data
        public List<string> questItems = new List<string>();
        public List<string> puzzleItems = new List<string>();

        // Quest data
        public List<string> activeQuestIDs = new List<string>();
        public List<string> completedQuestIDs = new List<string>();
        public Dictionary<string, List<string>> completedObjectives = new Dictionary<string, List<string>>();

        // Game state data
        public Dictionary<string, bool> flags = new Dictionary<string, bool>();

        // Save metadata
        public string saveName;
        public DateTime saveDate;
    }

    public static void SaveGame(int slotIdx)
    {
        // 保存当前使用的存档槽
        slotIndex = slotIdx;

        // Make sure directory exists
        if (!Directory.Exists(SaveDirectory))
        {
            Directory.CreateDirectory(SaveDirectory);
        }

        SaveData saveData = CreateSaveData();

        string savePath = SaveDirectory + "save_" + slotIndex + ".sav";

        BinaryFormatter formatter = new BinaryFormatter();
        FileStream fileStream = new FileStream(savePath, FileMode.Create);

        formatter.Serialize(fileStream, saveData);
        fileStream.Close();

        Debug.Log("Game saved to slot " + slotIndex);
    }

    private static SaveData CreateSaveData()
    {
        SaveData saveData = new SaveData();

        // Get player object
        GameObject player = GameObject.FindGameObjectWithTag("Player");

        if (player != null)
        {
            // Player position
            Vector3 position = player.transform.position;
            saveData.playerPosition[0] = position.x;
            saveData.playerPosition[1] = position.y;
            saveData.playerPosition[2] = position.z;

            // Player health/mana
            PlayerHealth health = player.GetComponent<PlayerHealth>();
            if (health != null)
            {
                saveData.playerHealth = health.GetHealthPercentage() * 100;
                saveData.playerMana = health.GetManaPercentage() * 100;
            }
        }

        // Current scene
        saveData.currentSceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;

        // Inventory data
        if (InventoryManager.Instance != null)
        {
            // Logic to get inventory items
            List<ItemData> allItems = InventoryManager.Instance.GetAllItems();

            foreach (ItemData item in allItems)
            {
                if (item.itemType == ItemType.QuestItem)
                {
                    saveData.questItems.Add(item.itemID);
                }
                else
                {
                    saveData.puzzleItems.Add(item.itemID);
                }
            }
        }

        // Quest data
        if (QuestManager.Instance != null)
        {
            saveData.activeQuestIDs = QuestManager.Instance.GetActiveQuestIDs();
            saveData.completedQuestIDs = QuestManager.Instance.GetCompletedQuestIDs();
            saveData.completedObjectives = QuestManager.Instance.GetAllCompletedObjectives();
        }

        // Game state flags
        if (GameStateManager.Instance != null)
        {
            saveData.flags = GameStateManager.Instance.GetAllFlags();
        }

        // Save metadata
        saveData.saveName = "Save " + (slotIndex + 1);
        saveData.saveDate = DateTime.Now;

        return saveData;
    }

    public static void LoadGame(int slotIdx)
    {
        // 保存当前使用的存档槽
        slotIndex = slotIdx;

        string savePath = SaveDirectory + "save_" + slotIndex + ".sav";

        if (File.Exists(savePath))
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
        else
        {
            Debug.LogWarning("Save file not found in slot " + slotIndex);
        }
    }

    private static void ApplySaveData(SaveData saveData)
    {
        // Load scene if different from current
        string currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        if (currentScene != saveData.currentSceneName)
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(saveData.currentSceneName);

            // We need to wait for the scene to load before continuing
            // This is handled by a coroutine in a persistent GameManager
            GameManager.Instance.StartCoroutine(ApplySaveDataAfterSceneLoad(saveData));
            return;
        }

        // Apply data directly if we're already in the correct scene
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
        // Get player object
        GameObject player = GameObject.FindGameObjectWithTag("Player");

        if (player != null)
        {
            // Player position
            Vector3 position = new Vector3(
                saveData.playerPosition[0],
                saveData.playerPosition[1],
                saveData.playerPosition[2]
            );
            player.transform.position = position;

            // Player health/mana
            PlayerHealth health = player.GetComponent<PlayerHealth>();
            if (health != null)
            {
                health.SetHealth(saveData.playerHealth);
                health.SetMana(saveData.playerMana);
            }
        }

        // Inventory data
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

        // Quest data
        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.ResetQuests();
            
            // Activate saved quests
            foreach (string questID in saveData.activeQuestIDs)
            {
                QuestManager.Instance.StartQuest(questID);
            }

            // Mark completed quests
            foreach (string questID in saveData.completedQuestIDs)
            {
                QuestManager.Instance.CompleteQuest(questID);
            }

            // Set completed objectives
            foreach (var kvp in saveData.completedObjectives)
            {
                string questID = kvp.Key;
                List<string> objectives = kvp.Value;

                foreach (string objectiveID in objectives)
                {
                    QuestManager.Instance.UpdateQuestObjective(questID, objectiveID);
                }
            }
        }

        // Game state flags
        if (GameStateManager.Instance != null)
        {
            GameStateManager.Instance.SetAllFlags(saveData.flags);
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
            int slotIdx = int.Parse(fileName.Substring(5, fileName.Length - 9)); // Extract slot index from "save_X.sav"

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
                    sceneName = saveData.currentSceneName
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

        // Sort by slot index
        Array.Sort(saveDataInfos, (a, b) => a.slotIndex.CompareTo(b.slotIndex));

        return saveDataInfos;
    }
}

// A lightweight class for save slot UI
public class SaveDataInfo
{
    public int slotIndex;
    public string saveName;
    public DateTime saveDate;
    public string sceneName;
}