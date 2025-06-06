using System;
using UnityEngine;

public class EnemyManager:MonoBehaviour
{
    public static EnemyManager Instance { get; private set; } // 单例实例
    public Enemy[] enemies; // 存储所有敌人

    
    public event Action<string> OnEnemyDeath; // 敌人死亡事件
    public event Action<EnemyType> OnEnemyTypeCleared; // 敌人类型清除事件
    public event Action<EnemyType> OnEnemyActivatedByType; // 敌人激活事件
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this; // 设置单例实例
        }
        else
        {
            Destroy(gameObject); // 如果实例已存在，则销毁当前对象
            return;
        }
    }

    private void OnEnable()
    {
        OnEnemyTypeCleared += OnEnemyTypeClearedHandler; // 订阅敌人类型清除事件
    }
    
    private void OnDisable()
    {
        OnEnemyTypeCleared -= OnEnemyTypeClearedHandler; // 取消订阅敌人类型清除事件
    }


    private void Start()
    {
        InitializeEnemies(); // 初始化
    }

    private void InitializeEnemies()
    {
        enemies = GetComponentsInChildren<Enemy>(); // 获取场景中的所有
        foreach (Enemy enemy in enemies)
        {
            enemy.DeactivateEnemy(); // 禁用所有
        }
    }
    
    // 激活所有指定指定类型的敌人
    public void ActivateEnemy(EnemyType type)
    {
        foreach (Enemy enemy in enemies)
        {
            if (enemy.enemyData.enemyType == type && !enemy.isActiveAndEnabled) // 检查敌人类型和是否未激活
            {
                enemy.ActivateEnemy(); // 激活敌人
                OnEnemyActivatedByType?.Invoke(type); // 触发敌人激活事件
                Debug.Log($"Activated enemy of type: {type} with ID: {enemy.enemyData.enemyID}");
            }
        }
    }
    
    // 检测场景中是否有激活的敌人
    public bool HasActiveEnemies()
    {
        foreach (Enemy enemy in enemies)
        {
            if (enemy.isActiveAndEnabled) // 检查敌人是否激活
            {
                return true; // 如果有一个敌人激活，返回true
            }
        }
        return false; // 如果没有激活的敌人，返回false
    }
    
    public void EnemyDied(Enemy enemy)
    {
        if (OnEnemyDeath != null)
        {
            OnEnemyDeath.Invoke(enemy.enemyData.enemyID); // 触发敌人死亡事件
            Debug.Log($"Enemy {enemy.enemyData.enemyID} has died.");
        }
        
        enemies = Array.FindAll(enemies, e => e.enemyData.enemyID != enemy.enemyData.enemyID); // 从列表中移除死亡的敌人
        if (enemies.Length == 0)
        {
            Debug.Log("All enemies have been defeated.");
            OnEnemyTypeCleared?.Invoke(enemy.enemyData.enemyType); // 触发敌人类型清除事件
            // TODO: 追加触发所有敌人死亡的事件
        }
        else
        {
            var enemiesByType = Array.FindAll(enemies, e => e.enemyData.enemyType == enemy.enemyData.enemyType); // 更新敌人类型列表
            if (enemiesByType.Length == 0)
            {
                Debug.Log($"All enemies of type {enemy.enemyData.enemyType} have been defeated.");
                OnEnemyTypeCleared?.Invoke(enemy.enemyData.enemyType); // 触发敌人类型清除事件
            }
        }
    }
    
    public bool CheckActiveEnemyType(EnemyType type)
    {
        foreach (Enemy enemy in enemies)
        {
            if (enemy.isActiveAndEnabled && enemy.enemyData.enemyType == type) // 检查敌人类型
            {
                return true; // 如果有一个敌人类型匹配，返回true
            }
        }
        return false; // 如果没有匹配的敌人类型，返回false
    }

    private void OnEnemyTypeClearedHandler(EnemyType obj)
    {
        if (obj == EnemyType.Enemy2)
        {
            DialogueManager.Instance.StartDialogueByID("lide_fight_over_dialogue");
        }
        
        if (obj == EnemyType.Enemy3)
        {
            DialogueManager.Instance.StartDialogueByID("lide_fight_over_xi_dialogue");
        }
        if (obj == EnemyType.Enemy4)
        {
            DialogueManager.Instance.StartDialogueByID("lide_fight_over_xin_dialogue");
        }
    }
}
