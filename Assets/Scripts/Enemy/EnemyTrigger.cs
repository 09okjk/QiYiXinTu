using UnityEngine;

public class EnemyTrigger:MonoBehaviour
{
    public EnemyType enemyType; // 敌人类型，用于激活特定类型的敌人
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.TryGetComponent<Player>(out Player player))
        {
            // 检查敌人是否已激活
            if (EnemyManager.Instance)
            {
                EnemyManager.Instance.ActivateEnemy(enemyType); // 激活Skeleton类型的敌人
                gameObject.SetActive(false); // 禁用触发器对象
            }
            else
            {
                Debug.LogError("EnemyManager instance is not available.");
            }
        }
    }
}