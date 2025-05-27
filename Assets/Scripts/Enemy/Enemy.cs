using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
public enum EnemyType
{
    Skeleton,
    Ranged,
    Magic,
    Boss,
    Enemy1,
}

public class Enemy : Entity
{
    public EnemyData enemyData => (EnemyData)baseData;
    
    [SerializeField] public LayerMask whatIsPlayer;
    [SerializeField] protected BoxCollider2D playerCheck;
    
    [Header("Enemy Info")]
    [SerializeField] protected GameObject counterImage; 
    public bool canBeStunned;
    public GameObject itemPrefab;
    public float stunnedDuration => enemyData.stunnedDuration;
    public Vector2 stunnedDirection => enemyData.stunnedDirection;
    public float moveSpeed => enemyData.moveSpeed;
    public float idleTime => enemyData.idleTime;
    public float battleTime => enemyData.battleTime;
    public float attackDistance => enemyData.attackDistance;
    public float attackCoolDown => enemyData.attackCooldown;
    [HideInInspector] public float lastAttackTime;
    [SerializeField] private List<ItemData> items = new List<ItemData>();
    
    public EnemyStateMachine stateMachine { get; private set; }
    
    private bool hasExecuted;

    protected override void Awake()
    {
        base.Awake();
        stateMachine = new EnemyStateMachine();
        hasExecuted = false;
    }
    
    protected override void Start()
    {
        base.Start();

        foreach (string itemID in enemyData.itemIDs)
        {
            ItemData itemData = Resources.Load<ItemData>("ScriptableObjects/Items/" + itemID);
            if (itemData != null)
            {
                items.Add(itemData);
            }
            else
            {
                Debug.LogError("Item not found: " + itemID);
            }
        }
    }
    

    protected override void Update()
    {
        base.Update();
        stateMachine.currentState.Update();
    }
    
    public virtual void AnimationFinishTrigger() => stateMachine.currentState.AnimationFinishTrigger();
    
    # region Check Functions 检查函数
    /// <summary>
    /// 检测是否有Player在检测范围内
    /// </summary>
    /// <returns>是否检测到Player</returns>
    public virtual bool IsPlayerDetected()
    {
        return Physics2D.OverlapBox(
            playerCheck.bounds.center, 
            playerCheck.bounds.size, 
            0f, 
            whatIsPlayer);
    }
    
    /// <summary>
    ///  检测是否有Player在攻击范围内
    /// </summary>
    /// <returns>是否检测到Player</returns>
    public virtual bool IsPlayerInAttackRange()
    {
        return Physics2D.OverlapBox(
            transform.position+attackDistance/2*transform.right, 
            new Vector2(attackDistance, 2), 
            0f, 
            whatIsPlayer);
    }
    
    # endregion
    
    public virtual void OpenCounterAttackWindow()
    {
        canBeStunned = true;
        //counterImage.SetActive(true);
    }
    
    public virtual void CloseCounterAttackWindow()
    {
        canBeStunned = false;
        //counterImage.SetActive(false);
    }
    
    public virtual bool CanBeStunned()
    {
        if (canBeStunned)
        {
            CloseCounterAttackWindow();
            return true;
        }
        return false;
    }
    
    public override async Task Die()
    {
        await base.Die();
        await DropItem();
        // 发布游戏事件
        EnemyManager.Instance.EnemyDied(this);
        Destroy(gameObject); 
    }
    
    public Task DropItem()
    {
        if (hasExecuted)
            return Task.CompletedTask;
        hasExecuted = true;
        // 检查是否有可掉落物品
        if (items == null || items.Count == 0)
            return Task.CompletedTask;
            
        // 分类物品
        List<ItemData> questItems = new List<ItemData>();
        List<ItemData> puzzleItems = new List<ItemData>();
        List<ItemData> consumables = new List<ItemData>();
        
        foreach (var item in items)
        {
            switch (item.itemType)
            {
                case ItemType.QuestItem:
                    questItems.Add(item);
                    break;
                case ItemType.PuzzleItem:
                    puzzleItems.Add(item);
                    break;
                case ItemType.Consumable:
                    consumables.Add(item);
                    break;
            }
        }
        
        // 处理必掉落物品 (任务和谜题物品)
        if (questItems.Count > 0)
        {
            // 随机选择一个任务物品
            ItemData selectedQuest = questItems[UnityEngine.Random.Range(0, questItems.Count)];
            SpawnItem(selectedQuest);
        }
        
        if (puzzleItems.Count > 0)
        {
            // 随机选择一个谜题物品
            ItemData selectedPuzzle = puzzleItems[UnityEngine.Random.Range(0, puzzleItems.Count)];
            SpawnItem(selectedPuzzle);
        }
        
        // 处理消耗品掉落
        if (consumables.Count > 0)
        {
            // 基础掉落数量
            int baseDropCount = UnityEngine.Random.Range(0, 2);
            int finalDropCount = baseDropCount;
            
            // 根据敌人类型调整掉落数量
            switch (enemyData.enemyType)
            {
                case EnemyType.Magic:
                    finalDropCount += UnityEngine.Random.Range(0, 2);
                    break;
                case EnemyType.Ranged:
                    if (UnityEngine.Random.value < 0.3f)
                        finalDropCount += 1;
                    break;
                case EnemyType.Boss:
                    finalDropCount += 2; // Boss至少多掉落2个物品
                    break;
            }
            Debug.Log("掉落数量: " + finalDropCount);
            // 掉落随机消耗品
            for (int i = 0; i < finalDropCount; i++)
            {
                Debug.Log("第" + (i + 1) + "次掉落");
                // 概率掉落 (70%几率)
                if (UnityEngine.Random.value <= 0.7f)
                {
                    Debug.Log("成功掉落消耗品");
                    ItemData selectedConsumable = consumables[UnityEngine.Random.Range(0, consumables.Count)];// 随机选择一个消耗品
                    SpawnItem(selectedConsumable);
                }
                else
                {
                    Debug.Log("掉落失败");
                }
            }
        }

        return Task.CompletedTask;

        // Die();
    }

    // 生成物品实例的辅助方法
    private void SpawnItem(ItemData itemData)
    {
        Debug.Log("掉落物品: " + itemData.itemName);
        // 获取物品预制体(需要实现物品预制体获取逻辑)
        GameObject itemPrefab = this.itemPrefab;
        
        if (itemPrefab != null)
        {
            // 在敌人周围随机位置生成物品，避免堆叠
            Vector2 dropPosition = (Vector2)transform.position + UnityEngine.Random.insideUnitCircle * 0.5f;
            Instantiate(itemPrefab, dropPosition, Quaternion.identity);
            itemPrefab.GetComponent<Item>().SetItemData(itemData);
        }
    }
    
    public virtual void ActivateEnemy()
    {
        gameObject.SetActive(true);
    }
    
    public virtual void DeactivateEnemy()
    {
        gameObject.SetActive(false);
        hasExecuted = false; // 重置执行状态
    }
    
    protected override void OnDrawGizmos()
    {
        base.OnDrawGizmos();
        
        // 绘制Player检测区域
        if (playerCheck != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireCube(playerCheck.bounds.center, playerCheck.bounds.size);
        }
        
        // 绘制攻击范围
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(transform.position+attackDistance/2*transform.right, new Vector3(attackDistance, 2, 0));
    }
}
