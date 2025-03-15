using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ItemSlot : MonoBehaviour
{
    [SerializeField] private Image iconImage;
    [SerializeField] private TextMeshProUGUI itemNameText;
    
    private ItemData item;
    
    public void SetItem(ItemData newItem)
    {
        item = newItem;
        
        if (iconImage != null)
        {
            iconImage.sprite = item.icon;
            iconImage.gameObject.SetActive(true);
        }
        
        if (itemNameText != null)
        {
            itemNameText.text = item.itemName;
        }
    }
    
    public void Clear()
    {
        item = null;
        
        if (iconImage != null)
        {
            iconImage.sprite = null;
            iconImage.gameObject.SetActive(false);
        }
        
        if (itemNameText != null)
        {
            itemNameText.text = "";
        }
    }
}