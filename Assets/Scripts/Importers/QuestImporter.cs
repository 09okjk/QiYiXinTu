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
    private const string SAVE_PATH = "Assets/Resources/ScriptableObjects/Quests";

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
            "列3: questText (任务描述)\n" +
            "列4: questConditionType (任务条件类型: None, CompleteDialogue, HaveItem, CompleteQuest)\n" +
            "列5: conditionValue (条件值)\n" +
            "列6: nextQuestID (下一个任务ID)\n", MessageType.Info);
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
            if (headers.Length < 6)
            {
                EditorUtility.DisplayDialog("错误", "CSV格式不正确。至少需要包含以下列：questID, questName, questText, questConditionType, conditionValue, nextQuestID", "确定");
                return;
            }

            // 确保保存目录存在
            EnsureDirectoryExists(SAVE_PATH);

            List<QuestData> questDataList = new List<QuestData>();

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

                if (values.Length < 6)
                {
                    Debug.LogWarning($"第 {i+1} 行: 数据不完整，需要6列信息，已跳过");
                    continue;
                }

                string questID = values[0].Trim();
                string questName = values[1].Trim();
                string questText = values[2].Trim();
                string questConditionTypeStr = values[3].Trim();
                string conditionValue = values[4].Trim();
                string nextQuestID = values[5].Trim();

                if (string.IsNullOrEmpty(questID) || string.IsNullOrEmpty(questName))
                {
                    Debug.LogWarning($"第 {i+1} 行: 缺少任务ID或名称，已跳过");
                    continue;
                }

                // 解析任务条件类型
                QuestCondition questConditionType = QuestCondition.None;
                if (!string.IsNullOrEmpty(questConditionTypeStr))
                {
                    if (Enum.TryParse(questConditionTypeStr, out QuestCondition parsedCondition))
                    {
                        questConditionType = parsedCondition;
                    }
                    else
                    {
                        Debug.LogWarning($"第 {i+1} 行: 无法解析任务条件类型 '{questConditionTypeStr}'，使用默认值 None");
                    }
                }

                // 创建新的任务数据
                QuestData questData = ScriptableObject.CreateInstance<QuestData>();
                questData.questID = questID;
                questData.questName = questName;
                questData.questText = questText;
                questData.questConditionType = questConditionType;
                questData.conditionValue = conditionValue;
                questData.nextQuestID = nextQuestID;
                questData.isCompleted = false;

                questDataList.Add(questData);
            }

            // 更新进度条
            EditorUtility.DisplayProgressBar("导入任务数据", "创建资源文件...", 0.9f);

            int successCount = 0;
            int totalQuests = questDataList.Count;

            // 保存每个任务数据为ScriptableObject资源
            for (int i = 0; i < questDataList.Count; i++)
            {
                QuestData questData = questDataList[i];

                // 更新进度条
                EditorUtility.DisplayProgressBar("导入任务数据", 
                    $"保存任务: {questData.questName} ({i+1}/{totalQuests})", 
                    0.9f + 0.1f * ((float)i / totalQuests));

                // 使用任务ID作为文件名，移除非法字符
                string safeQuestID = string.Join("", questData.questID.Split(Path.GetInvalidFileNameChars()));
                string savePath = Path.Combine(SAVE_PATH, safeQuestID + ".asset");

                // 检查是否已存在同ID资源
                if (File.Exists(savePath))
                {
                    // 如果存在，尝试加载并覆盖
                    QuestData existingQuest = AssetDatabase.LoadAssetAtPath<QuestData>(savePath);
                    if (existingQuest != null)
                    {
                        existingQuest.questName = questData.questName;
                        existingQuest.questText = questData.questText;
                        existingQuest.questConditionType = questData.questConditionType;
                        existingQuest.conditionValue = questData.conditionValue;
                        existingQuest.nextQuestID = questData.nextQuestID;
                        
                        EditorUtility.SetDirty(existingQuest);
                    }
                }
                else
                {
                    // 创建新资源
                    AssetDatabase.CreateAsset(questData, savePath);
                }
                
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

    // CSV行解析，处理引号中的逗号
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