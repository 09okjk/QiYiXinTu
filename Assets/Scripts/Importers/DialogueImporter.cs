using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Text;
using UnityEditor;
using System.Linq;

#if UNITY_EDITOR
public class DialogueImporter : EditorWindow
{
    private string csvFilePath = "";
    private const string SAVE_PATH = "Assets/Resources/Dialogues";

    [MenuItem("Tools/Dialogue System/Import Dialogue CSV")]
    public static void ShowWindow()
    {
        GetWindow<DialogueImporter>("Dialogue Importer");
    }

    private void OnGUI()
    {
        GUILayout.Label("Import Dialogue from CSV", EditorStyles.boldLabel);

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

            ImportDialogue(csvFilePath);
        }
        // 添加使用说明
        EditorGUILayout.Space(10);
        EditorGUILayout.HelpBox("CSV格式要求：\n" +
                                "列1: dialogueTitle (对话标题)\n" +
                                "列2: nodeID (节点ID，必须是整数)\n" +
                                "列3: speakerID (说话者ID)\n" +
                                "列4: speakerName (说话者名称)\n" +
                                "列5: text (对话文本)\n" +
                                "列6: nextNodeIndex (下一节点ID，-1表示结束)\n" +
                                "列7: speakerPosition (说话者位置，'left'或'right')\n" +
                                "其后的列: 每两列为一组，分别是选项文本和目标节点ID", MessageType.Info);
    }

    private void ImportDialogue(string filePath)
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
            if (headers.Length < 7) // 至少需要 dialogueTitle, nodeID, speakerID,speaker,text,nextNodeIndex,speakerPosition
            {
                EditorUtility.DisplayDialog("错误", "CSV格式不正确。至少需要包含以下列：dialogueTitle, nodeID, speakerID, text", "确定");
                return;
            }
            // 确保保存目录存在
            EnsureDirectoryExists(SAVE_PATH);
            
            // 按对话标题分组数据
            Dictionary<string, List<string[]>> dialogueGroups = new Dictionary<string, List<string[]>>();

            // 从第二行开始处理数据
            for (int i = 1; i < lines.Length; i++)
            {
                if (string.IsNullOrWhiteSpace(lines[i])) continue; // 跳过空行

                string[] values = ParseCSVLine(lines[i]);
                
                if (values.Length < 7)
                {
                    Debug.LogWarning($"第 {i+1} 行: 数据不完整，至少需要7列，已跳过");
                    continue;
                }

                string dialogueTitle = values[0].Trim();
                if (string.IsNullOrEmpty(dialogueTitle))
                {
                    Debug.LogWarning($"第 {i+1} 行: 缺少对话标题，已跳过");
                    continue;
                }

                if (!dialogueGroups.ContainsKey(dialogueTitle))
                {
                    dialogueGroups[dialogueTitle] = new List<string[]>();
                }
                
                dialogueGroups[dialogueTitle].Add(values);
            }

            // // 为每组对话创建对应的 DialogueData
            // string saveFolder = EditorUtility.SaveFolderPanel("选择保存对话数据的文件夹", "Assets", "");
            // if (string.IsNullOrEmpty(saveFolder))
            // {
            //     return;
            // }
            //
            // // 确保路径是相对于 Assets 的路径
            // if (!saveFolder.StartsWith(Application.dataPath))
            // {
            //     EditorUtility.DisplayDialog("错误", "请选择项目 Assets 文件夹内的位置。", "确定");
            //     return;
            // }
            //
            // string relativePath = "Assets" + saveFolder.Substring(Application.dataPath.Length);

            int successCount = 0;
            
            // 开始进度条
            EditorUtility.DisplayProgressBar("导入对话数据", "准备处理...", 0f);
            
            int totalGroups = dialogueGroups.Count;
            int currentGroup = 0;
            
            foreach (var group in dialogueGroups)
            {
                // 更新进度条
                EditorUtility.DisplayProgressBar("导入对话数据", 
                    $"正在处理: {group.Key} ({currentGroup+1}/{totalGroups})", 
                    (float)currentGroup / totalGroups);
                
                currentGroup++;
                
                string dialogueTitle = group.Key;
                List<string[]> dialogueLines = group.Value;
                
                // 创建对话数据资源
                DialogueData dialogueData = ScriptableObject.CreateInstance<DialogueData>();
                dialogueData.nodes = new List<DialogueNode>();

                Dictionary<int, int> nodeIDToIndex = new Dictionary<int, int>();
                int nodeIndex = 0;

                // 处理每一行数据
                foreach (var values in dialogueLines)
                {
                    // 尝试解析节点ID (values[1]是nodeID，因为values[0]是dialogueTitle)
                    if (!int.TryParse(values[1], out int nodeID))
                    {
                        Debug.LogError($"对话 '{dialogueTitle}': 无法将 '{values[1]}' 解析为节点ID。必须是整数。");
                        continue;
                    }

                    // 创建新节点
                    DialogueNode node = new DialogueNode
                    {
                        speakerID = values[2], // 第三列是speakerID
                        speakerName = values[3], // 第四列是speakerName
                        text = values[4], // 第五列是文本内容
                        speakerPosition = values[6], // 第七列是speakerPosition
                        choices = new List<DialogueChoice>()
                    };
                    
                    // 解析nextNodeIndex
                    if (int.TryParse(values[5], out int nextNodeIndex))
                    {
                        node.nextNodeIndex = nextNodeIndex; // 暂时存储原始nodeID，稍后更新
                    }
                    else
                    {
                        node.nextNodeIndex = -1; // 默认为-1表示没有下一个节点
                        Debug.LogWarning($"对话 '{dialogueTitle}', 节点 {nodeID}: 无法解析nextNodeIndex '{values[5]}'，已设为-1");
                    }
                    
                    // 记录节点ID到数组索引的映射
                    nodeIDToIndex[nodeID] = nodeIndex;
                    nodeIndex++;
                    
                    // 处理选择项
                    for (int c = 7; c < values.Length; c += 2)
                    {
                        if (c + 1 >= values.Length) break;

                        string choiceText = values[c].Trim();// 选项文本
                        if (!string.IsNullOrEmpty(choiceText))
                        {
                            int nextNodeID;
                            if (!int.TryParse(values[c + 1], out nextNodeID))
                            {
                                Debug.LogWarning($"对话 '{dialogueTitle}': 选项 '{choiceText}' 的目标节点ID '{values[c + 1]}' 无效，已跳过");
                                continue;
                            }

                            node.choices.Add(new DialogueChoice
                            {
                                text = choiceText,
                                nextNodeIndex = nextNodeID // 暂时存储原始nodeID，稍后更新
                            });
                        }
                    }
                    
                    dialogueData.nodes.Add(node);
                }
                
                // 更新选项中的nextNodeIndex为数组索引
                for (int i = 0; i < dialogueData.nodes.Count; i++)
                {
                    for (int j = 0; j < dialogueData.nodes[i].choices.Count; j++)
                    {
                        int targetNodeID = dialogueData.nodes[i].choices[j].nextNodeIndex;
                        if (targetNodeID >= 0 && nodeIDToIndex.ContainsKey(targetNodeID))
                        {
                            dialogueData.nodes[i].choices[j].nextNodeIndex = nodeIDToIndex[targetNodeID];
                        }
                    }
                }

                // 使用对话标题作为文件名，移除非法字符
                string safeTitleName = string.Join("", dialogueTitle.Split(Path.GetInvalidFileNameChars()));
                string savePath = Path.Combine(SAVE_PATH, safeTitleName + ".asset");
                
                AssetDatabase.CreateAsset(dialogueData, savePath);
                successCount++;
            }
            
            // 清除进度条
            EditorUtility.ClearProgressBar();
            
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            
            EditorUtility.DisplayDialog("成功", $"成功导入 {successCount} 个对话数据！", "确定");
        }
        catch (System.Exception e)
        {
            // 确保进度条被清除
            EditorUtility.ClearProgressBar();
            
            EditorUtility.DisplayDialog("错误", "导入对话失败: " + e.Message, "确定");
            Debug.LogError("对话导入错误: " + e);
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