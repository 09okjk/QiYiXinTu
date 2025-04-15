using UnityEngine;

[CreateAssetMenu(fileName = "New Item", menuName = "Inventory/Item")]
public class ItemData : ScriptableObject
{
    public string itemID;
    public string itemName;
    public string description;
    public Sprite icon;
    public ItemType itemType;
    

    
    [Tooltip("Additional properties specific to this item")]
    public ItemProperty[] properties;
}

[System.Serializable]
public class ItemProperty
{
    public string key;
    public string value;
}