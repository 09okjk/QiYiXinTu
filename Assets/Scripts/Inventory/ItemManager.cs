using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class ItemManager : MonoBehaviour
{
    public static ItemManager Instance;
    
    [SerializeField] private List<ItemData> allItems = new List<ItemData>();
    private Dictionary<string, ItemData> itemLookup = new Dictionary<string, ItemData>();
    
    private void Awake()
    {
        Instance = this;
        ItemData[] itemDatas = Resources.LoadAll<ItemData>("ScriptableObjects/Items");
        foreach (var itemData in itemDatas)
        {
            if (itemLookup.TryAdd(itemData.itemID, itemData))
            {
                allItems.Add(itemData);
            }
        }
    }

    public ItemData GetItem(string itemID)
    {
        return itemLookup.GetValueOrDefault(itemID);
    }
}