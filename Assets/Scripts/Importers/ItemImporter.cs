using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEditor;

#if UNITY_EDITOR
public class ItemImporter : EditorWindow
{
    private string csvFilePath = "";
    private string iconFolderPath = "Assets/Icons/Items"; // Default path
    
    [MenuItem("Tools/Inventory System/Import Items CSV")]
    public static void ShowWindow()
    {
        GetWindow<ItemImporter>("Item Importer");
    }
    
    private void OnGUI()
    {
        GUILayout.Label("Import Items from CSV", EditorStyles.boldLabel);
        
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
        
        iconFolderPath = EditorGUILayout.TextField("Icon Folder Path:", iconFolderPath);
        
        if (GUILayout.Button("Import"))
        {
            if (string.IsNullOrEmpty(csvFilePath))
            {
                EditorUtility.DisplayDialog("Error", "Please select a CSV file.", "OK");
                return;
            }
            
            ImportItems(csvFilePath);
        }
    }
    
    private void ImportItems(string filePath)
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
            string outputFolder = "Assets/ScriptableObjects/Items";
            if (!Directory.Exists(outputFolder))
            {
                Directory.CreateDirectory(outputFolder);
            }
            
            // Process each line (skip header)
            for (int i = 1; i < lines.Length; i++)
            {
                string[] values = lines[i].Split(',');
                
                // Format: ItemID, ItemName, Description, IconFileName, ItemType (0=Quest, 1=Puzzle), Properties
                if (values.Length < 5) continue; // Skip invalid lines
                
                string itemID = values[0];
                string itemName = values[1];
                string description = values[2];
                string iconFileName = values[3];
                ItemType itemType = (ItemType)int.Parse(values[4]);
                
                // Create item data
                ItemData itemData = ScriptableObject.CreateInstance<ItemData>();
                itemData.itemID = itemID;
                itemData.itemName = itemName;
                itemData.description = description;
                itemData.itemType = itemType;
                
                // Look for icon
                string iconPath = $"{iconFolderPath}/{iconFileName}.png";
                Sprite icon = AssetDatabase.LoadAssetAtPath<Sprite>(iconPath);
                if (icon != null)
                {
                    itemData.icon = icon;
                }
                else
                {
                    Debug.LogWarning($"Icon not found: {iconPath}");
                }
                
                // Process additional properties if any
                List<ItemProperty> properties = new List<ItemProperty>();
                for (int p = 5; p < values.Length; p += 2)
                {
                    if (p + 1 >= values.Length) break;
                    
                    string key = values[p];
                    string value = values[p + 1];
                    
                    if (!string.IsNullOrEmpty(key))
                    {
                        properties.Add(new ItemProperty { key = key, value = value });
                    }
                }
                itemData.properties = properties.ToArray();
                
                // Save the item data asset
                string assetPath = $"{outputFolder}/{itemID}.asset";
                AssetDatabase.CreateAsset(itemData, assetPath);
            }
            
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            
            EditorUtility.DisplayDialog("Success", "Items imported successfully!", "OK");
        }
        catch (System.Exception e)
        {
            EditorUtility.DisplayDialog("Error", "Failed to import items: " + e.Message, "OK");
            Debug.LogError("Item import error: " + e);
        }
    }
}
#endif