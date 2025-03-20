using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class ItemDatabase : MonoBehaviour
{
    public static ItemDatabase Instance;
    
    [SerializeField] private List<ItemData> allItems = new List<ItemData>();
    private Dictionary<string, ItemData> itemLookup = new Dictionary<string, ItemData>();
    
    private void Awake()
    {
        Instance = this;
        foreach (var item in allItems)
        {
            itemLookup[item.itemID] = item;
        }
    }
    
    public ItemData GetItem(string itemID)
    {
        if (itemLookup.TryGetValue(itemID, out ItemData item))
            return item;
        return null;
    }
    
#if UNITY_EDITOR
    [ContextMenu("自动加载所有物品")]
    private void AutoLoadItems()
    {
        allItems.Clear();
        string[] guids = AssetDatabase.FindAssets("t:ItemData", new[] {"Assets/ScriptableObjects/Items"});
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            ItemData item = AssetDatabase.LoadAssetAtPath<ItemData>(path);
            if (item != null)
            {
                allItems.Add(item);
            }
        }
        EditorUtility.SetDirty(this);
    }
#endif
}