using UnityEngine;

public class EnemyManager:MonoBehaviour
{
    public static EnemyManager Instance { get; private set; } // 单例实例
    public Enemy[] enemies; // 存储所有敌人

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

    private void Start()
    {
        InitializeEnemies(); // 初始化敌人
    }

    private void InitializeEnemies()
    {
        enemies = GetComponentsInChildren<Enemy>(); // 获取场景中的所有敌人
        foreach (Enemy enemy in enemies)
        {
            enemy.DeactivateEnemy(); // 禁用所有敌人
        }
    }
    
    
        
}