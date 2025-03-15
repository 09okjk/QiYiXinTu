using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEditor;

#if UNITY_EDITOR
public class QuestImporter : EditorWindow
{
    private string csvFilePath = "";
    
    [MenuItem("Tools/Quest System/Import Quests CSV")]
    public static void ShowWindow()
    {
        GetWindow<QuestImporter>("Quest Importer");
    }
    
    private void OnGUI()
    {
        GUILayout.Label("Import Quests from CSV", EditorStyles.boldLabel);
        
        EditorGUILayout.BeginHorizontal();
        csvFilePath = EditorGUILayout.TextField("CSV File Path:", csvFilePath);
        if (GUILayout.Button("Browse", GUILayout.Width(80)))
        {
            string path = EditorUtility.OpenFilePanel("Select CSV File", "", "csv");
            if (!string.IsNullOrEmpty(path))
            {
                csvFilePath = path;
            }
        }
        EditorGUILayout.EndHorizontal();
        
        if (GUILayout.Button("Import"))
        {
            if (string.IsNullOrEmpty(csvFilePath))
            {
                EditorUtility.DisplayDialog("Error", "Please select a CSV file.", "OK");
                return;
            }
            
            ImportQuests(csvFilePath);
        }
    }
    
    private void ImportQuests(string filePath)
    {
        try
        {
            string[] lines = File.ReadAllLines(filePath);
            if (lines.Length <= 1) // Check if there's data (header + at least 1 row)
            {
                EditorUtility.DisplayDialog("Error", "CSV file is empty or contains only headers.", "OK");
                return;
            }
            
            // Create output folder if it doesn't exist
            string outputFolder = "Assets/Resources/Quests";
            if (!Directory.Exists(outputFolder))
            {
                Directory.CreateDirectory(outputFolder);
            }
            
            Dictionary<string, QuestData> quests = new Dictionary<string, QuestData>();
            
            // Process each line (skip header)
            for (int i = 1; i < lines.Length; i++)
            {
                string[] values = lines[i].Split(',');
                
                // Format: QuestID, QuestName, Description, ObjectiveID, ObjectiveDescription, RewardType, RewardID, RewardDescription
                if (values.Length < 3) continue; // Skip invalid lines
                
                string questID = values[0];
                string questName = values[1];
                string description = values[2];
                
                // Get or create quest data
                QuestData questData;
                if (!quests.TryGetValue(questID, out questData))
                {
                    questData = ScriptableObject.CreateInstance<QuestData>();
                    questData.questID = questID;
                    questData.questName = questName;
                    questData.description = description;
                    quests.Add(questID, questData);
                }
                
                // Process objective if provided
                if (values.Length >= 5 && !string.IsNullOrEmpty(values[3]))
                {
                    string objectiveID = values[3];
                    string objectiveDescription = values[4];
                    
                    questData.objectives.Add(new QuestObjective
                    {
                        objectiveID = objectiveID,
                        description = objectiveDescription
                    });
                }
                
                // Process reward if provided
                if (values.Length >= 8 && !string.IsNullOrEmpty(values[5]))
                {
                    QuestRewardType rewardType = (QuestRewardType)int.Parse(values[5]);
                    string rewardID = values[6];
                    string rewardDescription = values[7];
                    
                    questData.rewards.Add(new QuestReward
                    {
                        rewardType = rewardType,
                        rewardID = rewardID,
                        description = rewardDescription
                    });
                }
            }
            
            // Save all quest data assets
            foreach (var kvp in quests)
            {
                string questID = kvp.Key;
                QuestData questData = kvp.Value;
                
                string assetPath = $"{outputFolder}/{questID}.asset";
                AssetDatabase.CreateAsset(questData, assetPath);
            }
            
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            
            EditorUtility.DisplayDialog("Success", "Quests imported successfully!", "OK");
        }
        catch (System.Exception e)
        {
            EditorUtility.DisplayDialog("Error", "Failed to import quests: " + e.Message, "OK");
            Debug.LogError("Quest import error: " + e);
        }
    }
}
#endif