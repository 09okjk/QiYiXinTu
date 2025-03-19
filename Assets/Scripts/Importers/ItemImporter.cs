using System;
            using System.Collections;
            using System.Collections.Generic;
            using UnityEngine;
            using System.IO;
            using System.Text;
            using UnityEditor;
            
            #if UNITY_EDITOR
            public class ItemImporter : EditorWindow
            {
                private string csvFilePath = "";
                private string iconFolderPath = "Assets/Art/UI/Icons"; // 默认图标路径
                private const string SAVE_PATH = "Assets/ScriptableObjects/Items";
                
                [MenuItem("Tools/Inventory System/Import Items CSV")]
                public static void ShowWindow()
                {
                    GetWindow<ItemImporter>("Item Importer");
                }
                
                private void OnGUI()
                {
                    GUILayout.Label("导入物品数据", EditorStyles.boldLabel);
                    
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
                    
                    iconFolderPath = EditorGUILayout.TextField("图标文件夹路径:", iconFolderPath);
                    
                    if (GUILayout.Button("导入"))
                    {
                        if (string.IsNullOrEmpty(csvFilePath))
                        {
                            EditorUtility.DisplayDialog("错误", "请选择一个CSV文件。", "确定");
                            return;
                        }
                        
                        ImportItems(csvFilePath);
                    }
                    
                    // 添加使用说明
                    EditorGUILayout.Space(10);
                    EditorGUILayout.HelpBox("CSV格式要求：\n" +
                        "列1: itemID (物品ID)\n" +
                        "列2: itemName (物品名称)\n" +
                        "列3: description (物品描述)\n" +
                        "列4: iconFileName (图标文件名，不含扩展名)\n" +
                        "列5: itemType (物品类型索引值)\n" +
                        "其后的列: 每两列为一组，分别是属性键和属性值", MessageType.Info);
                }
                
                private void ImportItems(string filePath)
                {
                    try
                    {
                        string[] lines = File.ReadAllLines(filePath);
                        if (lines.Length <= 1)
                        {
                            EditorUtility.DisplayDialog("错误", "CSV文件为空或只包含标题行。", "确定");
                            return;
                        }
                        
                        // 确保保存目录存在
                        EnsureDirectoryExists(SAVE_PATH);
                        
                        // 解析标题行，了解列的含义
                        string[] headers = lines[0].Split(',');
                        
                        // 检查标题行是否符合预期格式
                        if (headers.Length < 5) // 至少需要 5 列
                        {
                            EditorUtility.DisplayDialog("错误", "CSV格式不正确。至少需要包含以下列：itemID, itemName, description, iconFileName, itemType", "确定");
                            return;
                        }
                        
                        int successCount = 0;
                        int failCount = 0;
                        List<string> failedItems = new List<string>();
                        
                        // 开始进度条
                        EditorUtility.DisplayProgressBar("导入物品数据", "准备处理...", 0f);
                        
                        // 从第二行开始处理数据
                        for (int i = 1; i < lines.Length; i++)
                        {
                            // 更新进度条
                            EditorUtility.DisplayProgressBar("导入物品数据",
                                $"正在处理行 {i}/{lines.Length-1}",
                                (float)(i-1) / (lines.Length-1));
                            
                            if (string.IsNullOrWhiteSpace(lines[i])) continue; // 跳过空行
                            
                            string[] values = ParseCSVLine(lines[i]);
                            
                            if (values.Length < 5)
                            {
                                Debug.LogWarning($"第 {i+1} 行: 数据不完整，至少需要5列，已跳过");
                                failCount++;
                                failedItems.Add($"行 {i+1}: 数据不完整");
                                continue;
                            }
                            
                            string itemID = values[0].Trim();
                            if (string.IsNullOrEmpty(itemID))
                            {
                                Debug.LogWarning($"第 {i+1} 行: 缺少物品ID，已跳过");
                                failCount++;
                                failedItems.Add($"行 {i+1}: 缺少物品ID");
                                continue;
                            }
                            
                            try
                            {
                                // 提取物品基本信息
                                string itemName = values[1];
                                string description = values[2];
                                string iconFileName = values[3];
                                
                                // 尝试解析物品类型
                                ItemType itemType;
                                if (!Enum.TryParse(values[4], out itemType) && !int.TryParse(values[4], out int itemTypeInt))
                                {
                                    Debug.LogWarning($"第 {i+1} 行: 无法解析物品类型 '{values[4]}'，已设为默认值");
                                    itemType = ItemType.QuestItem; // 默认类型
                                }
                                else if (int.TryParse(values[4], out itemTypeInt))
                                {
                                    itemType = (ItemType)itemTypeInt;
                                }
                                
                                // 创建物品数据
                                ItemData itemData = ScriptableObject.CreateInstance<ItemData>();
                                itemData.itemID = itemID;
                                itemData.itemName = itemName;
                                itemData.description = description;
                                itemData.itemType = itemType;
                                
                                // 查找图标
                                string iconPath = $"{iconFolderPath}/{iconFileName}.png";
                                Sprite icon = AssetDatabase.LoadAssetAtPath<Sprite>(iconPath);
                                if (icon != null)
                                {
                                    itemData.icon = icon;
                                }
                                else
                                {
                                    Debug.LogWarning($"物品 '{itemID}': 图标未找到: {iconPath}");
                                }
                                
                                // 处理其他属性
                                List<ItemProperty> properties = new List<ItemProperty>();
                                for (int p = 5; p < values.Length; p += 2)
                                {
                                    if (p + 1 >= values.Length) break;
                                    
                                    string key = values[p].Trim();
                                    string value = values[p + 1].Trim();
                                    
                                    if (!string.IsNullOrEmpty(key))
                                    {
                                        properties.Add(new ItemProperty { key = key, value = value });
                                    }
                                }
                                itemData.properties = properties.ToArray();
                                
                                // 保存物品数据资产
                                string assetPath = Path.Combine(SAVE_PATH, $"{itemID}.asset");
                                AssetDatabase.CreateAsset(itemData, assetPath);
                                successCount++;
                            }
                            catch (Exception ex)
                            {
                                Debug.LogError($"处理物品 '{itemID}' 时出错: {ex.Message}");
                                failCount++;
                                failedItems.Add($"物品 '{itemID}': {ex.Message}");
                            }
                        }
                        
                        // 清除进度条
                        EditorUtility.ClearProgressBar();
                        
                        AssetDatabase.SaveAssets();
                        AssetDatabase.Refresh();
                        
                        string message = $"成功导入 {successCount} 个物品！\n保存路径: {SAVE_PATH}";
                        if (failCount > 0)
                        {
                            message += $"\n\n失败: {failCount} 个物品";
                            Debug.LogWarning($"导入时有 {failCount} 个物品失败:\n{string.Join("\n", failedItems)}");
                        }
                        
                        EditorUtility.DisplayDialog("导入结果", message, "确定");
                    }
                    catch (System.Exception e)
                    {
                        // 确保进度条被清除
                        EditorUtility.ClearProgressBar();
                        
                        EditorUtility.DisplayDialog("错误", "导入物品失败: " + e.Message, "确定");
                        Debug.LogError("物品导入错误: " + e);
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