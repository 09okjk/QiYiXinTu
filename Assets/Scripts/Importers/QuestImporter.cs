using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Text;
using UnityEditor;
using System.Linq;

#if UNITY_EDITOR
public class QuestImporter : EditorWindow
{
    private string csvFilePath = "";
    private const string SAVE_PATH = "Assets/ScriptableObjects/Quests";

    [MenuItem("Tools/Quest System/Import Quests CSV")]
    public static void ShowWindow()
    {
        GetWindow<QuestImporter>("Quest Importer");
    }

    private void OnGUI()
    {
        GUILayout.Label("导入任务数据", EditorStyles.boldLabel);

        EditorGUILayout.BeginHorizontal();
        csvFilePath = EditorGUILayout.TextField("CSV 文件路径:", csvFilePath);
        if (GUILayout.Button("浏览", GUILayout.Width(80)))
        {
            string path = EditorUtility.OpenFilePanel("选择 CSV 文件", "", "csv");
            if (!string.IsNullOrEmpty(path))
            {
                csvFilePath = path;
            }
        }
        EditorGUILayout.EndHorizontal();

        if (GUILayout.Button("导入"))
        {
            if (string.IsNullOrEmpty(csvFilePath))
            {
                EditorUtility.DisplayDialog("错误", "请选择一个CSV文件。", "确定");
                return;
            }

            ImportQuests(csvFilePath);
        }

        // 添加使用说明
        EditorGUILayout.Space(10);
        EditorGUILayout.HelpBox("CSV格式要求：\n" +
            "列1: questID (任务ID)\n" +
            "列2: questName (任务名称)\n" +
            "列3: description (任务描述)\n" +
            "列4: objectiveID (目标ID)\n" +
            "列5: objectiveDescription (目标描述)\n" +
            "列6: rewardType (奖励类型，整数: 0=物品, 1=经验, 2=金币)\n" +
            "列7: rewardID (奖励ID或数量)\n" +
            "列8: rewardDescription (奖励描述)\n\n" +
            "注意：多个目标或奖励应分成多行，保持相同的任务ID和名称", MessageType.Info);
    }

    private void ImportQuests(string filePath)
    {
        try
        {
            string[] lines = File.ReadAllLines(filePath);
            if (lines.Length <= 1)
            {
                EditorUtility.DisplayDialog("错误", "CSV文件为空或只包含标题行。", "确定");
                return;
            }

            // 解析标题行，了解列的含义
            string[] headers = lines[0].Split(',');

            // 检查标题行是否符合预期格式
            if (headers.Length < 8)
            {
                EditorUtility.DisplayDialog("错误", "CSV格式不正确。至少需要包含以下列：questID, questName, description, objectiveID, objectiveDescription, rewardType, rewardID, rewardDescription", "确定");
                return;
            }

            // 确保保存目录存在
            EnsureDirectoryExists(SAVE_PATH);

            Dictionary<string, QuestData> quests = new Dictionary<string, QuestData>();
            Dictionary<string, List<QuestObjective>> questObjectives = new Dictionary<string, List<QuestObjective>>();
            Dictionary<string, List<QuestReward>> questRewards = new Dictionary<string, List<QuestReward>>();

            // 开始进度条
            EditorUtility.DisplayProgressBar("导入任务数据", "解析CSV文件...", 0f);

            // 从第二行开始处理数据
            for (int i = 1; i < lines.Length; i++)
            {
                // 更新进度条
                EditorUtility.DisplayProgressBar("导入任务数据", 
                    $"解析第 {i} 行，共 {lines.Length - 1} 行", 
                    (float)(i - 1) / (lines.Length - 1));

                if (string.IsNullOrWhiteSpace(lines[i])) continue; // 跳过空行

                string[] values = ParseCSVLine(lines[i]);

                if (values.Length < 3)
                {
                    Debug.LogWarning($"第 {i+1} 行: 数据不完整，至少需要3列基本任务信息，已跳过");
                    continue;
                }

                string questID = values[0].Trim();
                string questName = values[1].Trim();
                string description = values[2].Trim();

                if (string.IsNullOrEmpty(questID) || string.IsNullOrEmpty(questName))
                {
                    Debug.LogWarning($"第 {i+1} 行: 缺少任务ID或名称，已跳过");
                    continue;
                }

                // 获取或创建任务数据
                if (!quests.TryGetValue(questID, out QuestData questData))
                {
                    questData = ScriptableObject.CreateInstance<QuestData>();
                    questData.questID = questID;
                    questData.questName = questName;
                    questData.description = description;
                    quests.Add(questID, questData);

                    questObjectives[questID] = new List<QuestObjective>();
                    questRewards[questID] = new List<QuestReward>();
                }

                // 处理目标
                if (values.Length >= 5 && !string.IsNullOrEmpty(values[3]))
                {
                    string objectiveID = values[3].Trim();
                    string objectiveDescription = values[4].Trim();

                    // 检查是否已存在相同ID的目标
                    if (!questObjectives[questID].Exists(o => o.objectiveID == objectiveID))
                    {
                        questObjectives[questID].Add(new QuestObjective
                        {
                            objectiveID = objectiveID,
                            description = objectiveDescription
                        });
                    }
                    else
                    {
                        Debug.LogWarning($"任务 '{questID}': 目标ID '{objectiveID}' 重复，已跳过");
                    }
                }

                // 处理奖励
                if (values.Length >= 8 && !string.IsNullOrEmpty(values[5]))
                {
                    try
                    {
                        if (!int.TryParse(values[5], out int rewardTypeInt))
                        {
                            Debug.LogWarning($"任务 '{questID}': 无法解析奖励类型 '{values[5]}'，已跳过");
                            continue;
                        }

                        if (!Enum.IsDefined(typeof(QuestRewardType), rewardTypeInt))
                        {
                            Debug.LogWarning($"任务 '{questID}': 奖励类型值 '{rewardTypeInt}' 无效，已跳过");
                            continue;
                        }

                        QuestRewardType rewardType = (QuestRewardType)rewardTypeInt;
                        string rewardID = values[6].Trim();
                        string rewardDescription = values[7].Trim();

                        // 验证奖励ID
                        if (rewardType == QuestRewardType.Experience || rewardType == QuestRewardType.Gold)
                        {
                            if (!int.TryParse(rewardID, out _))
                            {
                                Debug.LogWarning($"任务 '{questID}': 经验/金币奖励ID必须是整数，值 '{rewardID}' 无效，已跳过");
                                continue;
                            }
                        }

                        questRewards[questID].Add(new QuestReward
                        {
                            rewardType = rewardType,
                            rewardID = rewardID,
                            description = rewardDescription
                        });
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"处理任务 '{questID}' 的奖励时出错: {ex.Message}");
                    }
                }
            }

            // 更新进度条
            EditorUtility.DisplayProgressBar("导入任务数据", "创建资源文件...", 0.9f);

            int successCount = 0;
            int totalQuests = quests.Count;
            int currentQuest = 0;

            // 为每个任务创建ScriptableObject资源
            foreach (var kvp in quests)
            {
                string questID = kvp.Key;
                QuestData questData = kvp.Value;

                // 更新进度条
                EditorUtility.DisplayProgressBar("导入任务数据", 
                    $"保存任务: {questData.questName} ({currentQuest+1}/{totalQuests})", 
                    0.9f + 0.1f * ((float)currentQuest / totalQuests));
                
                currentQuest++;

                // 关联目标和奖励
                questData.objectives = questObjectives[questID];
                questData.rewards = questRewards[questID];

                // 使用任务名称作为文件名，移除非法字符
                string safeQuestName = string.Join("", questData.questName.Split(Path.GetInvalidFileNameChars()));
                string savePath = Path.Combine(SAVE_PATH, safeQuestName + ".asset");

                // 检查是否已存在同名资源
                if (File.Exists(savePath))
                {
                    int suffix = 1;
                    string baseFileName = safeQuestName;
                    while (File.Exists(savePath))
                    {
                        safeQuestName = $"{baseFileName}_{suffix}";
                        savePath = Path.Combine(SAVE_PATH, safeQuestName + ".asset");
                        suffix++;
                    }
                }

                AssetDatabase.CreateAsset(questData, savePath);
                successCount++;
            }

            // 清除进度条
            EditorUtility.ClearProgressBar();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog("成功", $"成功导入 {successCount} 个任务数据！\n保存路径: {SAVE_PATH}", "确定");
        }
        catch (System.Exception e)
        {
            // 确保进度条被清除
            EditorUtility.ClearProgressBar();

            EditorUtility.DisplayDialog("错误", "导入任务失败: " + e.Message, "确定");
            Debug.LogError("任务导入错误: " + e);
        }
    }

    // 确保目录存在
    private void EnsureDirectoryExists(string path)
    {
        if (!AssetDatabase.IsValidFolder(path))
        {
            string parentFolder = Path.GetDirectoryName(path);
            string newFolderName = Path.GetFileName(path);

            // 递归确保父目录存在
            if (!AssetDatabase.IsValidFolder(parentFolder))
            {
                EnsureDirectoryExists(parentFolder);
            }

            AssetDatabase.CreateFolder(parentFolder, newFolderName);
        }
    }

    // 更强大的CSV行解析，处理引号中的逗号
    private string[] ParseCSVLine(string line)
    {
        List<string> result = new List<string>();
        bool inQuotes = false;
        StringBuilder field = new StringBuilder();

        for (int i = 0; i < line.Length; i++)
        {
            char c = line[i];

            if (c == '"')
            {
                inQuotes = !inQuotes;
                continue;
            }

            if (c == ',' && !inQuotes)
            {
                result.Add(field.ToString());
                field.Clear();
                continue;
            }

            field.Append(c);
        }

        // 添加最后一个字段
        result.Add(field.ToString());

        return result.ToArray();
    }
}
#endif