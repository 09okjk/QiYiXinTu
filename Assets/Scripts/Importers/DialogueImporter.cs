using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEditor;

#if UNITY_EDITOR
public class DialogueImporter : EditorWindow
{
    private string csvFilePath = "";
    
    [MenuItem("Tools/Dialogue System/Import Dialogue CSV")]
    public static void ShowWindow()
    {
        GetWindow<DialogueImporter>("Dialogue Importer");
    }
    
    private void OnGUI()
    {
        GUILayout.Label("Import Dialogue from CSV", EditorStyles.boldLabel);
        
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
            
            ImportDialogue(csvFilePath);
        }
    }
    
    private void ImportDialogue(string filePath)
    {
        try
        {
            string[] lines = File.ReadAllLines(filePath);
            if (lines.Length <= 1) // 检查是否有数据（标题 + 至少1行）
            {
                EditorUtility.DisplayDialog("Error", "CSV file is empty or contains only headers.", "OK");
                return;
            }
            
            // 创建一个新的对话数据资源
            DialogueData dialogueData = ScriptableObject.CreateInstance<DialogueData>();
            
            // 跳过标题行（行0）并处理数据
            for (int i = 1; i < lines.Length; i++)
            {
                string[] values = lines[i].Split(',');
                
                // 应该是这样的格式：节点ID，文本，选择1文本，选择1目标节点ID，选择2文本，选择2目标节点ID，...
                if (values.Length < 2) continue; // 跳过无效行
                
                int nodeID = int.Parse(values[0]);
                string text = values[1];
                
                // 确保我们有足够的节点
                while (dialogueData.nodes.Count <= nodeID)
                {
                    dialogueData.nodes.Add(new DialogueNode());
                }
                
                // 设置节点文本
                dialogueData.nodes[nodeID].text = text;
                
                // 如果存在选择，处理它们
                for (int c = 2; c < values.Length; c += 2)
                {
                    if (c + 1 >= values.Length) break; // 没有足够的数据来完成选择
                    
                    if (!string.IsNullOrEmpty(values[c]))
                    {
                        string choiceText = values[c];
                        int nextNodeID = int.Parse(values[c + 1]);
                        
                        dialogueData.nodes[nodeID].choices.Add(new DialogueChoice
                        {
                            text = choiceText,
                            nextNodeIndex = nextNodeID
                        });
                    }
                }
            }
            
            // 保存对话数据资源
            string fileName = Path.GetFileNameWithoutExtension(filePath);
            string savePath = EditorUtility.SaveFilePanelInProject("Save Dialogue Data", fileName, "asset", "Save dialogue data as");
            
            if (!string.IsNullOrEmpty(savePath))
            {
                AssetDatabase.CreateAsset(dialogueData, savePath);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                
                EditorUtility.DisplayDialog("Success", "Dialogue data imported successfully!", "OK");
                Selection.activeObject = dialogueData; // 选择新资产
            }
        }
        catch (System.Exception e)
        {
            EditorUtility.DisplayDialog("Error", "Failed to import dialogue: " + e.Message, "OK");
            Debug.LogError("Dialogue import error: " + e);
        }
    }
}
#endif