using System;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Text;
using UnityEditor;
using Core;

#if UNITY_EDITOR
public class NPCImporter : EditorWindow
{
    private string csvFilePath = "";
    private const string SAVE_PATH = "Assets/Resources/ScriptableObjects/NPCs";

    [MenuItem("Tools/Character System/Import NPCs CSV")]
    public static void ShowWindow()
    {
        GetWindow<NPCImporter>("NPC Importer");
    }

    private void OnGUI()
    {
        GUILayout.Label("导入NPC数据", EditorStyles.boldLabel);

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

            ImportNPCs(csvFilePath);
        }

        EditorGUILayout.Space(10);
        EditorGUILayout.HelpBox("CSV格式要求：\n" +
            "列1: npcID\n" +
            "列2: npcName\n" +
            "列3: description\n" +
            "列4: spriteID\n" +
            "列5: MaxHealth\n" +
            "列6: CurrentHealth\n" +
            "列7: MaxMana\n" +
            "列8: CurrentMana\n" +
            "列9: InvincibleTime\n" +
            "列10: knockbackDirectionX\n" +
            "列11: knockbackDirectionY\n" +
            "列12: KnockbackDuration\n" +
            "列13: itemIDs (分号分隔)\n" +
            "列14: dialogueIDs (分号分隔)\n" +
            "其后的列: 每两列为一组，分别是属性键和属性值", MessageType.Info);
    }

    private void ImportNPCs(string filePath)
    {
        try
        {
            string[] lines = File.ReadAllLines(filePath);
            if (lines.Length <= 1)
            {
                EditorUtility.DisplayDialog("错误", "CSV文件为空或只包含标题行。", "确定");
                return;
            }

            EnsureDirectoryExists(SAVE_PATH);

            string[] headers = lines[0].Split(',');

            if (headers.Length < 14)
            {
                EditorUtility.DisplayDialog("错误", "CSV格式不正确。至少需要包含14列基本NPC属性。", "确定");
                return;
            }

            int successCount = 0;
            int failCount = 0;
            List<string> failedNPCs = new List<string>();

            EditorUtility.DisplayProgressBar("导入NPC数据", "准备处理...", 0f);

            for (int i = 1; i < lines.Length; i++)
            {
                EditorUtility.DisplayProgressBar("导入NPC数据",
                    $"正在处理行 {i}/{lines.Length-1}",
                    (float)(i-1) / (lines.Length-1));

                if (string.IsNullOrWhiteSpace(lines[i])) continue;

                string[] values = ParseCSVLine(lines[i]);

                if (values.Length < 14)
                {
                    Debug.LogWarning($"第 {i+1} 行: 数据不完整，至少需要14列，已跳过");
                    failCount++;
                    failedNPCs.Add($"行 {i+1}: 数据不完整");
                    continue;
                }

                string npcID = values[0].Trim();
                if (string.IsNullOrEmpty(npcID))
                {
                    Debug.LogWarning($"第 {i+1} 行: 缺少NPC ID，已跳过");
                    failCount++;
                    failedNPCs.Add($"行 {i+1}: 缺少NPC ID");
                    continue;
                }

                try
                {
                    NPCData npcData = ScriptableObject.CreateInstance<NPCData>();
                    npcData.npcID = npcID;
                    npcData.npcName = values[1].Trim();
                    npcData.spriteID = values[3].Trim();

                    // EntityData字段
                    int maxHealth = 100;
                    int.TryParse(values[4], out maxHealth);
                    npcData.MaxHealth = maxHealth;

                    int currentHealth = maxHealth;
                    int.TryParse(values[5], out currentHealth);
                    npcData.CurrentHealth = currentHealth;

                    float maxMana = 0f;
                    float.TryParse(values[6], out maxMana);
                    npcData.MaxMana = maxMana;

                    float currentMana = maxMana;
                    float.TryParse(values[7], out currentMana);
                    npcData.CurrentMana = currentMana;

                    float invincibleTime = 0f;
                    float.TryParse(values[8], out invincibleTime);
                    npcData.InvincibleTime = invincibleTime;

                    float knockbackX = 0f, knockbackY = 0f;
                    float.TryParse(values[9], out knockbackX);
                    float.TryParse(values[10], out knockbackY);
                    npcData.knockbackDirection = new Vector2(knockbackX, knockbackY);

                    float knockbackDuration = 0f;
                    float.TryParse(values[11], out knockbackDuration);
                    npcData.KnockbackDuration = knockbackDuration;

                    // itemIDs
                    npcData.itemIDs = new List<string>();
                    string itemIDsString = values[12];
                    if (!string.IsNullOrEmpty(itemIDsString))
                    {
                        string[] itemIDs = itemIDsString.Split(';');
                        foreach (var id in itemIDs)
                        {
                            if (!string.IsNullOrEmpty(id.Trim()))
                                npcData.itemIDs.Add(id.Trim());
                        }
                    }

                    // dialogueIDs
                    npcData.dialogueIDs = new List<string>();
                    string dialogueIDsString = values[13];
                    if (!string.IsNullOrEmpty(dialogueIDsString))
                    {
                        string[] dialogueIDs = dialogueIDsString.Split(';');
                        foreach (var id in dialogueIDs)
                        {
                            if (!string.IsNullOrEmpty(id.Trim()))
                                npcData.dialogueIDs.Add(id.Trim());
                        }
                    }

                    // 额外属性
                    List<NPCProperty> properties = new List<NPCProperty>();
                    for (int p = 14; p < values.Length; p += 2)
                    {
                        if (p + 1 >= values.Length) break;
                        string key = values[p].Trim();
                        string value = values[p + 1].Trim();
                        if (!string.IsNullOrEmpty(key))
                        {
                            properties.Add(new NPCProperty { key = key, value = value });
                        }
                    }
                    npcData.properties = properties.ToArray();

                    // 保存
                    string assetPath = Path.Combine(SAVE_PATH, $"{npcID}.asset");
                    AssetDatabase.CreateAsset(npcData, assetPath);
                    successCount++;
                }
                catch (Exception ex)
                {
                    Debug.LogError($"处理NPC '{npcID}' 时出错: {ex.Message}");
                    failCount++;
                    failedNPCs.Add($"NPC '{npcID}': {ex.Message}");
                }
            }

            EditorUtility.ClearProgressBar();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            string message = $"成功导入 {successCount} 个NPC！\n保存路径: {SAVE_PATH}";
            if (failCount > 0)
            {
                message += $"\n\n失败: {failCount} 个NPC";
                Debug.LogWarning($"导入时有 {failCount} 个NPC失败:\n{string.Join("\n", failedNPCs)}");
            }

            EditorUtility.DisplayDialog("导入结果", message, "确定");
        }
        catch (System.Exception e)
        {
            EditorUtility.ClearProgressBar();
            EditorUtility.DisplayDialog("错误", "导入NPC失败: " + e.Message, "确定");
            Debug.LogError("NPC导入错误: " + e);
        }
    }

    private void EnsureDirectoryExists(string path)
    {
        if (!AssetDatabase.IsValidFolder(path))
        {
            string parentFolder = Path.GetDirectoryName(path);
            string newFolderName = Path.GetFileName(path);
            if (!AssetDatabase.IsValidFolder(parentFolder))
            {
                EnsureDirectoryExists(parentFolder);
            }
            AssetDatabase.CreateFolder(parentFolder, newFolderName);
        }
    }

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
        result.Add(field.ToString());
        return result.ToArray();
    }
}
#endif