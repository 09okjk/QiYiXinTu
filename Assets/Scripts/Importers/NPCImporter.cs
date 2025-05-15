using System;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Text;
using UnityEditor;

#if UNITY_EDITOR
public class NPCImporter : EditorWindow
{
    // private string csvFilePath = "";
    // private string avatarFolderPath = "Assets/Art/Characters/Portraits"; // 默认头像路径
    // private const string SAVE_PATH = "Assets/ScriptableObjects/NPCs";
    //
    // [MenuItem("Tools/Character System/Import NPCs CSV")]
    // public static void ShowWindow()
    // {
    //     GetWindow<NPCImporter>("NPC Importer");
    // }
    //
    // private void OnGUI()
    // {
    //     GUILayout.Label("导入NPC数据", EditorStyles.boldLabel);
    //     
    //     EditorGUILayout.BeginHorizontal();
    //     csvFilePath = EditorGUILayout.TextField("CSV 文件路径:", csvFilePath);
    //     if (GUILayout.Button("浏览", GUILayout.Width(80)))
    //     {
    //         string path = EditorUtility.OpenFilePanel("选择 CSV 文件", "", "csv");
    //         if (!string.IsNullOrEmpty(path))
    //         {
    //             csvFilePath = path;
    //         }
    //     }
    //     EditorGUILayout.EndHorizontal();
    //     
    //     avatarFolderPath = EditorGUILayout.TextField("头像文件夹路径:", avatarFolderPath);
    //     
    //     if (GUILayout.Button("导入"))
    //     {
    //         if (string.IsNullOrEmpty(csvFilePath))
    //         {
    //             EditorUtility.DisplayDialog("错误", "请选择一个CSV文件。", "确定");
    //             return;
    //         }
    //         
    //         ImportNPCs(csvFilePath);
    //     }
    //     
    //     // 添加使用说明
    //     EditorGUILayout.Space(10);
    //     EditorGUILayout.HelpBox("CSV格式要求：\n" +
    //         "列1: npcID (NPC ID)\n" +
    //         "列2: npcName (NPC名称)\n" +
    //         "列3: description (NPC描述)\n" +
    //         "列4: avatarFileName (头像文件名，不含扩展名)\n" +
    //         "列5: npcType (NPC类型索引值)\n" +
    //         "列6: dialogueID (对话ID)\n" +
    //         "列7: questIDs (任务ID列表，用分号分隔)\n" +
    //         "列8: isMerchant (是否商人，1或0)\n" +
    //         "列9: soldItemIDs (出售物品ID列表，用分号分隔)\n" +
    //         "其后的列: 每两列为一组，分别是属性键和属性值", MessageType.Info);
    // }
    //
    // private void ImportNPCs(string filePath)
    // {
    //     try
    //     {
    //         string[] lines = File.ReadAllLines(filePath);
    //         if (lines.Length <= 1)
    //         {
    //             EditorUtility.DisplayDialog("错误", "CSV文件为空或只包含标题行。", "确定");
    //             return;
    //         }
    //         
    //         // 确保保存目录存在
    //         EnsureDirectoryExists(SAVE_PATH);
    //         
    //         // 解析标题行，了解列的含义
    //         string[] headers = lines[0].Split(',');
    //         
    //         // 检查标题行是否符合预期格式
    //         if (headers.Length < 9) // 至少需要9列
    //         {
    //             EditorUtility.DisplayDialog("错误", "CSV格式不正确。至少需要包含基本NPC属性列。", "确定");
    //             return;
    //         }
    //         
    //         int successCount = 0;
    //         int failCount = 0;
    //         List<string> failedNPCs = new List<string>();
    //         
    //         // 开始进度条
    //         EditorUtility.DisplayProgressBar("导入NPC数据", "准备处理...", 0f);
    //         
    //         // 从第二行开始处理数据
    //         for (int i = 1; i < lines.Length; i++)
    //         {
    //             // 更新进度条
    //             EditorUtility.DisplayProgressBar("导入NPC数据",
    //                 $"正在处理行 {i}/{lines.Length-1}",
    //                 (float)(i-1) / (lines.Length-1));
    //             
    //             if (string.IsNullOrWhiteSpace(lines[i])) continue; // 跳过空行
    //             
    //             string[] values = ParseCSVLine(lines[i]);
    //             
    //             if (values.Length < 9)
    //             {
    //                 Debug.LogWarning($"第 {i+1} 行: 数据不完整，至少需要9列，已跳过");
    //                 failCount++;
    //                 failedNPCs.Add($"行 {i+1}: 数据不完整");
    //                 continue;
    //             }
    //             
    //             string npcID = values[0].Trim();
    //             if (string.IsNullOrEmpty(npcID))
    //             {
    //                 Debug.LogWarning($"第 {i+1} 行: 缺少NPC ID，已跳过");
    //                 failCount++;
    //                 failedNPCs.Add($"行 {i+1}: 缺少NPC ID");
    //                 continue;
    //             }
    //             
    //             try
    //             {
    //                 // 提取NPC基本信息
    //                 string npcName = values[1];
    //                 string description = values[2];
    //                 string avatarFileName = values[3];
    //                 
    //                 // 尝试解析NPC类型
    //                 NPCType npcType;
    //                 if (!Enum.TryParse(values[4], out npcType) && !int.TryParse(values[4], out int npcTypeInt))
    //                 {
    //                     Debug.LogWarning($"第 {i+1} 行: 无法解析NPC类型 '{values[4]}'，已设为默认值");
    //                     npcType = NPCType.Villager; // 默认类型
    //                 }
    //                 else if (int.TryParse(values[4], out npcTypeInt))
    //                 {
    //                     npcType = (NPCType)npcTypeInt;
    //                 }
    //                 
    //                 // 创建NPC数据
    //                 NPCData npcData = ScriptableObject.CreateInstance<NPCData>();
    //                 npcData.npcID = npcID;
    //                 npcData.npcName = npcName;
    //                 npcData.description = description;
    //                 npcData.npcType = npcType;
    //                 npcData.dialogueID = values[5];
    //                 
    //                 // 解析任务ID列表
    //                 string questIDsString = values[6];
    //                 if (!string.IsNullOrEmpty(questIDsString))
    //                 {
    //                     string[] questIDs = questIDsString.Split(';');
    //                     foreach (string questID in questIDs)
    //                     {
    //                         if (!string.IsNullOrEmpty(questID.Trim()))
    //                             npcData.availableQuestIDs.Add(questID.Trim());
    //                     }
    //                 }
    //                 
    //                 // 解析是否商人
    //                 npcData.isMerchant = values[7] == "1" || values[7].ToLower() == "true";
    //                 
    //                 // 解析出售物品ID列表
    //                 string soldItemIDsString = values[8];
    //                 if (!string.IsNullOrEmpty(soldItemIDsString))
    //                 {
    //                     string[] soldItemIDs = soldItemIDsString.Split(';');
    //                     foreach (string itemID in soldItemIDs)
    //                     {
    //                         if (!string.IsNullOrEmpty(itemID.Trim()))
    //                             npcData.soldItemIDs.Add(itemID.Trim());
    //                     }
    //                 }
    //                 
    //                 // 查找头像
    //                 string avatarPath = $"{avatarFolderPath}/{avatarFileName}.png";
    //                 Sprite avatar = AssetDatabase.LoadAssetAtPath<Sprite>(avatarPath);
    //                 if (avatar != null)
    //                 {
    //                     npcData.avatar = avatar;
    //                 }
    //                 else
    //                 {
    //                     Debug.LogWarning($"NPC '{npcID}': 头像未找到: {avatarPath}");
    //                 }
    //                 
    //                 // 处理其他属性
    //                 List<NPCProperty> properties = new List<NPCProperty>();
    //                 for (int p = 9; p < values.Length; p += 2)
    //                 {
    //                     if (p + 1 >= values.Length) break;
    //                     
    //                     string key = values[p].Trim();
    //                     string value = values[p + 1].Trim();
    //                     
    //                     if (!string.IsNullOrEmpty(key))
    //                     {
    //                         properties.Add(new NPCProperty { key = key, value = value });
    //                     }
    //                 }
    //                 npcData.properties = properties.ToArray();
    //                 
    //                 // 保存NPC数据资产
    //                 string assetPath = Path.Combine(SAVE_PATH, $"{npcID}.asset");
    //                 AssetDatabase.CreateAsset(npcData, assetPath);
    //                 successCount++;
    //             }
    //             catch (Exception ex)
    //             {
    //                 Debug.LogError($"处理NPC '{npcID}' 时出错: {ex.Message}");
    //                 failCount++;
    //                 failedNPCs.Add($"NPC '{npcID}': {ex.Message}");
    //             }
    //         }
    //         
    //         // 清除进度条
    //         EditorUtility.ClearProgressBar();
    //         
    //         AssetDatabase.SaveAssets();
    //         AssetDatabase.Refresh();
    //         
    //         string message = $"成功导入 {successCount} 个NPC！\n保存路径: {SAVE_PATH}";
    //         if (failCount > 0)
    //         {
    //             message += $"\n\n失败: {failCount} 个NPC";
    //             Debug.LogWarning($"导入时有 {failCount} 个NPC失败:\n{string.Join("\n", failedNPCs)}");
    //         }
    //         
    //         EditorUtility.DisplayDialog("导入结果", message, "确定");
    //     }
    //     catch (System.Exception e)
    //     {
    //         // 确保进度条被清除
    //         EditorUtility.ClearProgressBar();
    //         
    //         EditorUtility.DisplayDialog("错误", "导入NPC失败: " + e.Message, "确定");
    //         Debug.LogError("NPC导入错误: " + e);
    //     }
    // }
    //
    // // 确保目录存在
    // private void EnsureDirectoryExists(string path)
    // {
    //     if (!AssetDatabase.IsValidFolder(path))
    //     {
    //         string parentFolder = Path.GetDirectoryName(path);
    //         string newFolderName = Path.GetFileName(path);
    //         
    //         // 递归确保父目录存在
    //         if (!AssetDatabase.IsValidFolder(parentFolder))
    //         {
    //             EnsureDirectoryExists(parentFolder);
    //         }
    //         
    //         AssetDatabase.CreateFolder(parentFolder, newFolderName);
    //     }
    // }
    //
    // // CSV行解析，处理引号中的逗号
    // private string[] ParseCSVLine(string line)
    // {
    //     List<string> result = new List<string>();
    //     bool inQuotes = false;
    //     StringBuilder field = new StringBuilder();
    //     
    //     for (int i = 0; i < line.Length; i++)
    //     {
    //         char c = line[i];
    //         
    //         if (c == '"')
    //         {
    //             inQuotes = !inQuotes;
    //             continue;
    //         }
    //         
    //         if (c == ',' && !inQuotes)
    //         {
    //             result.Add(field.ToString());
    //             field.Clear();
    //             continue;
    //         }
    //         
    //         field.Append(c);
    //     }
    //     
    //     // 添加最后一个字段
    //     result.Add(field.ToString());
    //     
    //     return result.ToArray();
    // }
}
#endif