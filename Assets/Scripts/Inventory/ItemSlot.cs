using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ItemSlot : MonoBehaviour
{
    [SerializeField] private Image iconImage;
    public Button button;
    
    private bool isSelected = false;
    private ItemData item;

    private void Start()
    {
        if (button != null)
        {
            button.onClick.AddListener(OnItemClicked);
        }
    }

    private void Update()
    {
        if (isSelected)
        {
            // 这里可以添加选中状态的视觉效果，比如高亮边框等
            iconImage.color = Color.yellow; // 示例：将图标颜色改为黄色
        }
        else
        {
            iconImage.color = Color.white; // 恢复默认颜色
        }
    }

    public void SetItem(ItemData newItem)
    {
        item = newItem;
        
        if (iconImage)
        {
            iconImage.sprite = item.icon;
            iconImage.gameObject.SetActive(true);
        }
    }
    
    private void OnItemClicked()
    {
        if (!item) return;
        isSelected = !isSelected;
        InventoryManager.Instance.ItemDetailsTrigger(isSelected, item);
    }
    
    public void Clear()
    {
        item = null;
        
        if (iconImage != null)
        {
            iconImage.sprite = null;
            iconImage.gameObject.SetActive(false);
        }
    }
}