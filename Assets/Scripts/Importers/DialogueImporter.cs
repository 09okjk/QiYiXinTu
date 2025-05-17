using System;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Text;
using Dialogue;
using UnityEditor;

#if UNITY_EDITOR
public class DialogueImporter : EditorWindow
{
    private string csvFilePath = "";
    private const string SAVE_PATH = "Assets/Resources/ScriptableObjects/Dialogues";

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
        
        EditorGUILayout.Space(10);
        EditorGUILayout.HelpBox("CSV格式要求：\n" +
                                "列1: dialogueID (对话ID)\n" +
                                "列2: nodeID (节点ID)\n" +
                                "列3: speakerID (说话者ID)\n" +
                                "列4: speakerName (说话者名称)\n" +
                                "列5: speakerType (说话者类型：Player/Npc/System)\n" +
                                "列6: emotion (说话者情绪：Neutral/Happy/Sad等)\n" +
                                "列7: text (对话文本)\n" +
                                "列8: nextNodeID (下一节点ID，空表示结束)\n" +
                                "列9: questID (任务ID，可空)\n" +
                                "列10: rewardIDs (奖励ID列表，用分号分隔)\n" +
                                "列11: isFollow (是否跟随，true/false)\n" +
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

            // 解析标题行
            string[] headers = lines[0].Split(',');
            
            // 检查标题行是否符合预期格式
            if (headers.Length < 11) // 至少需要 11 个基本列（增加了 isFollow）
            {
                EditorUtility.DisplayDialog("错误", "CSV格式不正确。至少需要包含：dialogueID, nodeID, speakerID, speakerName, speakerType, emotion, text, nextNodeID, questID, rewardIDs, isFollow", "确定");
                return;
            }
            
            // 确保保存目录存在
            EnsureDirectoryExists(SAVE_PATH);
            
            // 按对话ID分组数据
            Dictionary<string, List<string[]>> dialogueGroups = new Dictionary<string, List<string[]>>();

            // 从第二行开始处理数据
            for (int i = 1; i < lines.Length; i++)
            {
                if (string.IsNullOrWhiteSpace(lines[i])) continue; // 跳过空行

                string[] values = ParseCSVLine(lines[i]);
                
                if (values.Length < 11)
                {
                    Debug.LogWarning($"第 {i+1} 行: 数据不完整，至少需要11列，已跳过");
                    continue;
                }

                string dialogueID = values[0].Trim();
                if (string.IsNullOrEmpty(dialogueID))
                {
                    Debug.LogWarning($"第 {i+1} 行: 缺少对话ID，已跳过");
                    continue;
                }

                if (!dialogueGroups.ContainsKey(dialogueID))
                {
                    dialogueGroups[dialogueID] = new List<string[]>();
                }
                
                dialogueGroups[dialogueID].Add(values);
            }

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
                
                string dialogueID = group.Key;
                List<string[]> dialogueLines = group.Value;
                
                // 创建对话数据资源
                DialogueData dialogueData = ScriptableObject.CreateInstance<DialogueData>();
                dialogueData.dialogueID = dialogueID;
                dialogueData.state = DialogueState.WithOutStart;
                dialogueData.nodes = new List<DialogueNode>();

                // 处理每一行数据
                foreach (var values in dialogueLines)
                {
                    string nodeID = values[1].Trim();
                    if (string.IsNullOrEmpty(nodeID))
                    {
                        Debug.LogError($"对话 '{dialogueID}': 节点ID不能为空。");
                        continue;
                    }

                    // 创建对话角色
                    DialogueSpeaker speaker = new DialogueSpeaker()
                    {
                        speakerID = values[2], // 第三列是speakerID
                        speakerName = values[3], // 第四列是speakerName
                        speakerType = ParseSpeakerType(values[4]), // 第五列是speakerType
                        emotion = ParseEmotion(values[5]) // 第六列是emotion
                    };
                    
                    // 解析 isFollow 值
                    bool isFollow = false;
                    if (values.Length > 10)
                    {
                        bool.TryParse(values[10].Trim().ToLower(), out isFollow);
                    }

                    // 创建新节点
                    DialogueNode node = new DialogueNode
                    {
                        nodeID = nodeID,
                        speaker = speaker,
                        text = values[6], // 第七列是文本内容
                        nextNodeID = values[7].Trim(), // 第八列是nextNodeID
                        questID = values[8].Trim(), // 第九列是questID
                        rewardIDs = ParseRewardIDs(values[9]), // 第十列是rewardIDs
                        isFollow = isFollow, // 第十一列是isFollow
                        choices = new List<DialogueChoice>()
                    };
                    
                    // 处理选择项
                    for (int c = 11; c < values.Length; c += 2)
                    {
                        if (c + 1 >= values.Length) break;

                        string choiceText = values[c].Trim();
                        string nextNodeID = values[c + 1].Trim();
                        
                        if (!string.IsNullOrEmpty(choiceText))
                        {
                            node.choices.Add(new DialogueChoice
                            {
                                text = choiceText,
                                nextNodeID = nextNodeID
                            });
                        }
                    }
                    
                    dialogueData.nodes.Add(node);
                }
                
                // 如果有节点，设置当前节点为第一个节点
                if (dialogueData.nodes.Count > 0)
                {
                    dialogueData.currentNodeID = dialogueData.nodes[0].nodeID;
                }

                // 使用对话ID作为文件名，移除非法字符
                string safeTitleName = string.Join("", dialogueID.Split(Path.GetInvalidFileNameChars()));
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

    // 解析说话者类型
    private SpeakerType ParseSpeakerType(string typeString)
    {
        typeString = typeString.Trim();
        
        if (Enum.TryParse(typeString, true, out SpeakerType speakerType))
        {
            return speakerType;
        }
        
        Debug.LogWarning($"无法解析说话者类型: {typeString}，使用默认值Npc");
        return SpeakerType.Npc;
    }

    // 解析情绪类型
    private Emotion ParseEmotion(string emotionString)
    {
        emotionString = emotionString.Trim();
        
        if (Enum.TryParse(emotionString, true, out Emotion emotion))
        {
            return emotion;
        }
        
        Debug.LogWarning($"无法解析情绪类型: {emotionString}，使用默认值Neutral");
        return Emotion.Neutral;
    }

    // 解析奖励ID列表
    private List<string> ParseRewardIDs(string rewardIDsString)
    {
        List<string> rewardIDs = new List<string>();
        if (!string.IsNullOrEmpty(rewardIDsString))
        {
            string[] ids = rewardIDsString.Split(';');
            foreach (string id in ids)
            {
                string trimmedId = id.Trim();
                if (!string.IsNullOrEmpty(trimmedId))
                {
                    rewardIDs.Add(trimmedId);
                }
            }
        }
        return rewardIDs;
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