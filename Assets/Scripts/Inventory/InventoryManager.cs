using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public enum ItemType
{
    QuestItem,
    PuzzleItem,
    Consumable
}
[Serializable]
public class ItemGameData
{
    public string itemID;
    public string itemName;
    public string description;
    public ItemType itemType;
    public Sprite icon;
    public ItemProperty[] properties;
}

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance { get; private set; }

    [Header("UI References")]
    [SerializeField] private GameObject inventoryPanel;
    [SerializeField] private GameObject itemSlotPrefab;
    [SerializeField] private Transform itemContainer;
    [SerializeField] private TextMeshProUGUI itemTypeText;
    [SerializeField] private GameObject itemMessageContainer;
    [SerializeField] private TextMeshProUGUI itemNameText;
    [SerializeField] private TextMeshProUGUI itemDescriptionText;
    [SerializeField] private Image itemImage;
    [SerializeField] private Button questTabButton;
    [SerializeField] private Button puzzleTabButton;

    [SerializeField] private List<ItemGameData> questItems = new List<ItemGameData>();
    [SerializeField] private List<ItemGameData> puzzleItems = new List<ItemGameData>();
    [SerializeField] private ItemType currentTab = ItemType.QuestItem;

    public event Action<bool> OnInventoryStateChanged;
    public event Action<string> OnAddItem;
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        // 设置选项卡按钮监听器
        questTabButton.onClick.AddListener(() => SwitchTab(ItemType.QuestItem));
        puzzleTabButton.onClick.AddListener(() => SwitchTab(ItemType.PuzzleItem));
    }

    private void Start()
    {
        inventoryPanel.SetActive(false);
        ItemDetailsTrigger(false);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && inventoryPanel.activeSelf)
        {
            CloseInventory();
        }
    }

    public void ToggleInventory()
    {
        if (inventoryPanel.activeSelf)
        {
            CloseInventory();
        }
        else
        {
            OpenInventory();
        }
    }

    public void OpenInventory()
    {
        inventoryPanel.SetActive(true);
        RefreshInventoryUI();
        OnInventoryStateChanged?.Invoke(true);
        Time.timeScale = 0; // 当背包打开时暂停游戏
    }

    public void CloseInventory()
    {
        inventoryPanel.SetActive(false);
        OnInventoryStateChanged?.Invoke(false);
        Time.timeScale = 1; // 当背包关闭时继续游戏
    }

    // 添加物品
    public void AddItem(ItemGameData item)
    {
        if (item.itemType == ItemType.QuestItem)
        {
            questItems.Add(item);
        }
        else
        {
            puzzleItems.Add(item);
        }

        // 如果背包打开，刷新背包
        if (inventoryPanel.activeSelf)
        {
            RefreshInventoryUI();
        }

        // 通知玩家获得新物品
        UIManager.Instance.ShowNotification($"新物品获得: {item.itemName}");
        UIManager.Instance.ShowConfirmDialog(
            "获得新物品",
            $"你获得了新物品: {item.itemName}",
            item.icon,
            () => { Debug.Log("确认按钮被点击"); }
        );
        OnAddItem?.Invoke(item.itemID);
    }

    public void AddItemById(string itemID)
    {
        var item = ItemManager.Instance.GetItem(itemID);
        if (item != null)
            AddItem(item);
        else
            Debug.LogWarning($"Item with ID {itemID} not found.");
    }

    // 检查是否拥有某个物品
    public bool HasItem(string itemID)
    {
        return questItems.Exists(item => item.itemID == itemID) ||
               puzzleItems.Exists(item => item.itemID == itemID);
    }
    
    // 移除物品
    public void RemoveItem(string itemID)
    {
        // 在两个列表中查找物品
        ItemGameData item = questItems.Find(i => i.itemID == itemID);
        if (item != null)
        {
            questItems.Remove(item);
        }
        else
        {
            item = puzzleItems.Find(i => i.itemID == itemID);
            if (item != null)
            {
                puzzleItems.Remove(item);
            }
        }

        // 如果背包打开，刷新背包
        if (inventoryPanel.activeSelf)
        {
            RefreshInventoryUI();
        }
    }

    // 获取所有物品
    public List<string> GetAllItemIDs()
    {
        List<string> allItemIDs = new List<string>();
        foreach (var item in questItems)
        {
            allItemIDs.Add(item.itemID);
        }
        foreach (var item in puzzleItems)
        {
            allItemIDs.Add(item.itemID);
        }
        return allItemIDs;
    }
    
    // 设置所有物品
    public void SetAllItemsByIDs(List<string> itemIDs)
    {
        questItems.Clear();
        puzzleItems.Clear();
        
        foreach (var itemID in itemIDs)
        {
            AddItemById(itemID);
        }
    }
    // 清空背包
    public void ClearInventory()
    {
        questItems.Clear();
        puzzleItems.Clear();
    }

    // 切换选项卡
    private void SwitchTab(ItemType tabType)
    {
        currentTab = tabType;
        RefreshInventoryUI();
    }

    // 刷新背包UI
    private void RefreshInventoryUI()
    {
        // 更新选项卡按钮的可交互状态
        questTabButton.interactable = currentTab != ItemType.QuestItem;
        puzzleTabButton.interactable = currentTab != ItemType.PuzzleItem;
     
        // 清除现有物品槽
        foreach (Transform child in itemContainer)
        {
            Destroy(child.gameObject);
        }
        
        Debug.Log("currentTab: " + currentTab);
        // 根据当前选项卡显示/隐藏容器
        // questItemContainer.gameObject.SetActive(currentTab == ItemType.QuestItem);
        // puzzleItemContainer.gameObject.SetActive(currentTab == ItemType.PuzzleItem);
        
        itemTypeText.text = currentTab.ToString();
        
        // 填充物品槽
        if (currentTab == ItemType.QuestItem)
        {
            PopulateItemContainer(questItems, itemContainer);
        }
        else
        {
            PopulateItemContainer(puzzleItems, itemContainer);
        }
    }

    /// <summary>
    /// 填充物品容器
    /// </summary>
    /// <param name="items">物品列表</param>
    /// <param name="container">容器</param>
    private void PopulateItemContainer(List<ItemGameData> items, Transform container)
    {
        int itemCount = items.Count;
        for (int i = 0; i < 25; i++)
        {
            GameObject slotGO = Instantiate(itemSlotPrefab, container);
            ItemSlot slot = slotGO.GetComponent<ItemSlot>();
            if (i < itemCount)
            {
                slot.SetItem(items[i]);
            }
        }
    }

    // 控制物品详情显示
    public void ItemDetailsTrigger(bool isSelected, ItemGameData item = null)
    {
        if (item == null)
        {
            itemNameText.text = "";
            itemDescriptionText.text = "";
            itemMessageContainer.SetActive(false);
            return;
        }
        itemNameText.text = item.itemName;
        itemDescriptionText.text = item.description;
        itemImage.sprite = item.icon;
        itemMessageContainer.SetActive(isSelected);
    }
}