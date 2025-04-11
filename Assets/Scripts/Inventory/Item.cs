using System;
using UnityEngine;

public class Item:MonoBehaviour
{   
    private ItemData _itemData;
    
    public SpriteRenderer icon; 
    
    private void Awake()
    {
        // 初始化物品图标和碰撞器
        icon = GetComponent<SpriteRenderer>();
    }

    public void SetItemData(ItemData itemData)
    {
        _itemData = itemData;
        icon.sprite = itemData.icon;
    }
    
    public void PickUp()
    {
        // 处理物品拾取逻辑
        // 例如：将物品添加到玩家的背包中
        InventoryManager.Instance.AddItem(_itemData);
        
        // 销毁物品实例
        Destroy(gameObject);
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            PickUp();
        }
    }
}