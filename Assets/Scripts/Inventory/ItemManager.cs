using System.Collections.Generic;
using UnityEngine;

public class ItemManager : MonoBehaviour
{
    public static ItemManager Instance { get; private set; }
    
    [SerializeField] private List<ItemGameData> allItems = new List<ItemGameData>();
    private readonly Dictionary<string, ItemGameData> itemLookup = new Dictionary<string, ItemGameData>();
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            allItems.Clear();
            var itemDatas = Resources.LoadAll<ItemData>("ScriptableObjects/Items");
            foreach (var itemData in itemDatas)
            {
                var itemGameData = new ItemGameData
                {
                    itemID = itemData.itemID,
                    itemName = itemData.itemName,
                    description = itemData.description,
                    itemType = itemData.itemType,
                    icon = itemData.icon,
                    properties = itemData.properties
                };
                if (itemLookup.TryAdd(itemGameData.itemID, itemGameData))
                {
                    allItems.Add(itemGameData);
                }
            }
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public ItemGameData GetItem(string itemID)
    {
        itemLookup.TryGetValue(itemID, out var item);
        return item;
    }
}