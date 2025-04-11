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

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance { get; private set; }

    [Header("UI References")]
    [SerializeField] private GameObject inventoryPanel;
    [SerializeField] private GameObject itemSlotPrefab;
    [SerializeField] private Transform questItemContainer;
    [SerializeField] private Transform puzzleItemContainer;
    [SerializeField] private TextMeshProUGUI itemNameText;
    [SerializeField] private TextMeshProUGUI itemDescriptionText;
    [SerializeField] private Image itemImage;
    [SerializeField] private Button questTabButton;
    [SerializeField] private Button puzzleTabButton;

    private List<ItemData> questItems = new List<ItemData>();
    private List<ItemData> puzzleItems = new List<ItemData>();
    private ItemType currentTab = ItemType.QuestItem;

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
        ClearItemDetails();
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
        Time.timeScale = 0; // 当背包打开时暂停游戏
    }

    public void CloseInventory()
    {
        inventoryPanel.SetActive(false);
        Time.timeScale = 1; // 当背包关闭时继续游戏
    }

    // 添加物品
    public void AddItem(ItemData item)
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
        ItemData item = questItems.Find(i => i.itemID == itemID);
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
    public List<ItemData> GetAllItems()
    {
        List<ItemData> allItems = new List<ItemData>(questItems);
        allItems.AddRange(puzzleItems);
        return allItems; // 返回列表的副本
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

        // 更新选项卡按钮的可交互状态
        questTabButton.interactable = tabType != ItemType.QuestItem;
        puzzleTabButton.interactable = tabType != ItemType.PuzzleItem;
    }

    // 刷新背包UI
    private void RefreshInventoryUI()
    {
        // 清除现有物品槽
        foreach (Transform child in questItemContainer)
        {
            Destroy(child.gameObject);
        }
        foreach (Transform child in puzzleItemContainer)
        {
            Destroy(child.gameObject);
        }

        // 根据当前选项卡显示/隐藏容器
        questItemContainer.gameObject.SetActive(currentTab == ItemType.QuestItem);
        puzzleItemContainer.gameObject.SetActive(currentTab == ItemType.PuzzleItem);

        // 填充当前选项卡
        if (currentTab == ItemType.QuestItem)
        {
            PopulateItemContainer(questItems, questItemContainer);
        }
        else
        {
            PopulateItemContainer(puzzleItems, puzzleItemContainer);
        }
    }

    /// <summary>
    /// 填充物品容器
    /// </summary>
    /// <param name="items">物品列表</param>
    /// <param name="container">容器</param>
    private void PopulateItemContainer(List<ItemData> items, Transform container)
    {
        foreach (ItemData item in items)
        {
            GameObject slotGO = Instantiate(itemSlotPrefab, container);
            ItemSlot slot = slotGO.GetComponent<ItemSlot>();
            slot.SetItem(item);

            // 添加点击监听器
            Button button = slotGO.GetComponent<Button>();
            button.onClick.AddListener(() => ShowItemDetails(item));
        }
    }

    // 显示物品详情
    private void ShowItemDetails(ItemData item)
    {
        itemNameText.text = item.itemName;
        itemDescriptionText.text = item.description;
        itemImage.sprite = item.icon;
        itemImage.gameObject.SetActive(true);
    }

    // 清除物品详情
    private void ClearItemDetails()
    {
        itemNameText.text = "";
        itemDescriptionText.text = "";
        itemImage.gameObject.SetActive(false);
    }
}