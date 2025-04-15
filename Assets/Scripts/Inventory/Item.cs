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
    public void PickUp(Player player)
    {
        if (_itemData == null)
        {
            Debug.LogWarning("尝试拾取的物品没有ItemData");
            Destroy(gameObject);
            return;
        }

        // 根据物品类型执行不同逻辑
        switch (_itemData.itemType)
        {
            case ItemType.Consumable:
                // 处理消耗品逻辑：直接应用效果
                ApplyConsumableEffect(player);
                break;
                
            case ItemType.QuestItem:
            case ItemType.PuzzleItem:
                // 任务道具和解谜道具添加到背包
                InventoryManager.Instance.AddItem(_itemData);
                Debug.Log($"已添加{_itemData.itemName}到背包");
                break;
                
            default:
                InventoryManager.Instance.AddItem(_itemData);
                break;
        }
        
        // 销毁物品实例
        Destroy(gameObject);
    }

    private void ApplyConsumableEffect(Player player)
    {
        float healthRestore = 0;
        float manaRestore = 0;
        foreach (var property in _itemData.properties)
        {
            if (property.key == "healthRestore")
            {
                healthRestore = Convert.ToSingle(property.value);
            }
            if(property.key == "manaRestore")
            {
                manaRestore = Convert.ToSingle(property.value);
            }
        }
        
        // 检查消耗品属性
        if (healthRestore > 0)
        {
            // 恢复生命值
            player.AddHealth(healthRestore);
            Debug.Log($"恢复了{healthRestore}点生命值");
        }
        
        if (manaRestore > 0)
        {
            // 恢复魔法值
            player.AddMana(manaRestore);
            Debug.Log($"恢复了{manaRestore}点魔法值");
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Player player = other.GetComponent<Player>();
            if (player != null)
            {
                PickUp(player);
            }
            else
            {
                Debug.LogWarning("检测到Player标签但没有找到Player组件");
            }
        }
    }

    private void OnValidate()
    {
        // 自动设置碰撞器为触发器
        Collider2D col = GetComponent<Collider2D>();
        if (col == null)
        {
            col = gameObject.AddComponent<CircleCollider2D>();
            col.isTrigger = true;
        }
        else
        {
            col.isTrigger = true;
        }
    }
}