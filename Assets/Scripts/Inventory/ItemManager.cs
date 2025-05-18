using System.Collections.Generic;
using UnityEngine;

public class ItemManager : MonoBehaviour
{
    public static ItemManager Instance { get; private set; }
    
    [SerializeField] private List<ItemData> allItems = new List<ItemData>();
    private readonly Dictionary<string, ItemData> itemLookup = new Dictionary<string, ItemData>();
    
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
                if (itemLookup.TryAdd(itemData.itemID, itemData))
                {
                    allItems.Add(itemData);
                }
            }
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public ItemData GetItem(string itemID)
    {
        itemLookup.TryGetValue(itemID, out var item);
        return item;
    }
}