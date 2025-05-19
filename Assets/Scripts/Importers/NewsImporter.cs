using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;
using News;

#if UNITY_EDITOR
public class NewsImporter : EditorWindow
{
    private string csvFilePath = "";
    private string imageFolderPath = "Assets/Resources/Art/News"; // 默认图片路径
    private const string SAVE_PATH = "Assets/Resources/ScriptableObjects/News";

    [MenuItem("Tools/News System/Import News CSV")]
    public static void ShowWindow()
    {
        GetWindow<NewsImporter>("News Importer");
    }

    private void OnGUI()
    {
        GUILayout.Label("导入新闻数据", EditorStyles.boldLabel);

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

        imageFolderPath = EditorGUILayout.TextField("图片文件夹路径:", imageFolderPath);

        if (GUILayout.Button("导入"))
        {
            if (string.IsNullOrEmpty(csvFilePath))
            {
                EditorUtility.DisplayDialog("错误", "请选择一个CSV文件。", "确定");
                return;
            }
            ImportNews(csvFilePath);
        }

        EditorGUILayout.Space(10);
        EditorGUILayout.HelpBox("CSV格式要求：\n" +
            "列1: newsID\n" +
            "列2: newsTitle\n" +
            "列3: newsContent\n" +
            "列4: newsImageFileName (不含扩展名)\n" +
            "列5: isRead (true/false)", MessageType.Info);
    }

    private void ImportNews(string filePath)
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
            if (headers.Length < 5)
            {
                EditorUtility.DisplayDialog("错误", "CSV格式不正确。至少需要5列。", "确定");
                return;
            }

            int successCount = 0;
            int failCount = 0;
            List<string> failedNews = new List<string>();

            EditorUtility.DisplayProgressBar("导入新闻数据", "准备处理...", 0f);

            for (int i = 1; i < lines.Length; i++)
            {
                EditorUtility.DisplayProgressBar("导入新闻数据",
                    $"正在处理行 {i}/{lines.Length-1}",
                    (float)(i-1) / (lines.Length-1));

                if (string.IsNullOrWhiteSpace(lines[i])) continue;

                string[] values = ParseCSVLine(lines[i]);
                if (values.Length < 5)
                {
                    Debug.LogWarning($"第 {i+1} 行: 数据不完整，已跳过");
                    failCount++;
                    failedNews.Add($"行 {i+1}: 数据不完整");
                    continue;
                }

                string newsID = values[0].Trim();
                if (string.IsNullOrEmpty(newsID))
                {
                    Debug.LogWarning($"第 {i+1} 行: 缺少新闻ID，已跳过");
                    failCount++;
                    failedNews.Add($"行 {i+1}: 缺少新闻ID");
                    continue;
                }

                try
                {
                    string newsTitle = values[1];
                    string newsContent = values[2];
                    string imageFileName = values[3];
                    bool isRead = false;
                    bool.TryParse(values[4], out isRead);

                    NewsData newsData = ScriptableObject.CreateInstance<NewsData>();
                    newsData.newsID = newsID;
                    newsData.newsTitle = newsTitle;
                    newsData.newsContent = newsContent;
                    newsData.isRead = isRead;

                    string imagePath = $"{imageFolderPath}/{imageFileName}.png";
                    Sprite newsImage = AssetDatabase.LoadAssetAtPath<Sprite>(imagePath);
                    if (newsImage != null)
                    {
                        newsData.newsImage = newsImage;
                    }
                    else
                    {
                        Debug.LogWarning($"新闻 '{newsID}': 图片未找到: {imagePath}");
                    }

                    string assetPath = Path.Combine(SAVE_PATH, $"{newsID}.asset");
                    AssetDatabase.CreateAsset(newsData, assetPath);
                    successCount++;
                }
                catch (Exception ex)
                {
                    Debug.LogError($"处理新闻 '{newsID}' 时出错: {ex.Message}");
                    failCount++;
                    failedNews.Add($"新闻 '{newsID}': {ex.Message}");
                }
            }

            EditorUtility.ClearProgressBar();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            string message = $"成功导入 {successCount} 条新闻！\n保存路径: {SAVE_PATH}";
            if (failCount > 0)
            {
                message += $"\n\n失败: {failCount} 条新闻";
                Debug.LogWarning($"导入时有 {failCount} 条新闻失败:\n{string.Join("\n", failedNews)}");
            }
            EditorUtility.DisplayDialog("导入结果", message, "确定");
        }
        catch (Exception e)
        {
            EditorUtility.ClearProgressBar();
            EditorUtility.DisplayDialog("错误", "导入新闻失败: " + e.Message, "确定");
            Debug.LogError("新闻导入错误: " + e);
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