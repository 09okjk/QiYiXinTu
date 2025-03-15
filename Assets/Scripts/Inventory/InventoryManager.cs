using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public enum ItemType
{
    QuestItem,
    PuzzleItem
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

        // If inventory is open, refresh it
        if (inventoryPanel.activeSelf)
        {
            RefreshInventoryUI();
        }

        // Notify player of new item
        UIManager.Instance.ShowNotification($"新物品获得: {item.itemName}");
    }

    public bool HasItem(string itemID)
    {
        return questItems.Exists(item => item.itemID == itemID) ||
               puzzleItems.Exists(item => item.itemID == itemID);
    }

    public void RemoveItem(string itemID)
    {
        // Find item in either list
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

        // If inventory is open, refresh it
        if (inventoryPanel.activeSelf)
        {
            RefreshInventoryUI();
        }
    }

    public List<ItemData> GetAllItems()
    {
        List<ItemData> allItems = new List<ItemData>(questItems);
        allItems.AddRange(puzzleItems);
        return allItems; // 返回列表的副本
    }

    public void ClearInventory()
    {
        questItems.Clear();
        puzzleItems.Clear();
    }

    private void SwitchTab(ItemType tabType)
    {
        currentTab = tabType;
        RefreshInventoryUI();

        // Update tab button visual state
        questTabButton.interactable = tabType != ItemType.QuestItem;
        puzzleTabButton.interactable = tabType != ItemType.PuzzleItem;
    }

    private void RefreshInventoryUI()
    {
        // Clear existing item slots
        foreach (Transform child in questItemContainer)
        {
            Destroy(child.gameObject);
        }
        foreach (Transform child in puzzleItemContainer)
        {
            Destroy(child.gameObject);
        }

        // Show/hide containers based on current tab
        questItemContainer.gameObject.SetActive(currentTab == ItemType.QuestItem);
        puzzleItemContainer.gameObject.SetActive(currentTab == ItemType.PuzzleItem);

        // Populate the current tab
        if (currentTab == ItemType.QuestItem)
        {
            PopulateItemContainer(questItems, questItemContainer);
        }
        else
        {
            PopulateItemContainer(puzzleItems, puzzleItemContainer);
        }
    }

    private void PopulateItemContainer(List<ItemData> items, Transform container)
    {
        foreach (ItemData item in items)
        {
            GameObject slotGO = Instantiate(itemSlotPrefab, container);
            ItemSlot slot = slotGO.GetComponent<ItemSlot>();
            slot.SetItem(item);

            // Add click listener
            Button button = slotGO.GetComponent<Button>();
            button.onClick.AddListener(() => ShowItemDetails(item));
        }
    }

    private void ShowItemDetails(ItemData item)
    {
        itemNameText.text = item.itemName;
        itemDescriptionText.text = item.description;
        itemImage.sprite = item.icon;
        itemImage.gameObject.SetActive(true);
    }

    private void ClearItemDetails()
    {
        itemNameText.text = "";
        itemDescriptionText.text = "";
        itemImage.gameObject.SetActive(false);
    }
}