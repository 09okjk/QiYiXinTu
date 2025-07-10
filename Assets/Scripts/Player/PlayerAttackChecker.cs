using System;
using System.Collections.Generic;
using Manager;
using UnityEngine;

public class PlayerAttackChecker: MonoBehaviour
{
    public BoxCollider2D attackCollider;
    public AttackType currentAttackType;
    
    private int damage;

    private Dictionary<string, string> HitNameMap = new Dictionary<string, string>();

    private void Awake()
    {

    }
    private void Start()
    {
        if (attackCollider == null)
        {
            Debug.LogError("Attack collider is not assigned in PlayerAttackChecker.");
        }
        
        switch (currentAttackType)
        {
            case AttackType.SkillAttack1_1:
            case AttackType.SkillAttack1_2:
                damage = SkillManager.Instance.attackSkill.damage;
                break;
            default:
                damage = PlayerManager.Instance.player.playerData.attackDamage;
                break;
        }
    }

    // 检测攻击碰撞器是否与敌人发生碰撞
    private void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider.CompareTag("Enemy"))
        {
            switch (currentAttackType)
            {
                case AttackType.SkillAttack1_1:
                case AttackType.SkillAttack1_2:
                    damage = SkillManager.Instance.attackSkill.damage;
                    break;
                default:
                    damage = PlayerManager.Instance.player.playerData.attackDamage;
                    break;
            }
            // 获取敌人的名称
            string enemyId = collider.gameObject.GetComponent<Enemy>().enemyData.enemyID;
            Debug.Log("Enemy hit: " + enemyId + " with damage: " + damage);
            if (HitNameMap.ContainsKey(enemyId))
            {
                // 如果敌人已经被击中，直接返回
                return;
            }
            
            HitNameMap[enemyId] = enemyId;
            if (collider.TryGetComponent<Enemy>(out Enemy enemy))
            {
                enemy.Damage(damage);
            }
        }
    }

    public void SetColliderState(bool state)
    {
        attackCollider.enabled = state;
        if (state)
        {
            HitNameMap.Clear(); // 清空已击中敌人列表
        }
    }

    public void SetColliderSize(Vector2 size)
    {
        if (attackCollider != null)
        {
            attackCollider.size = size;
        }
        else
        {
            Debug.LogWarning("Attack collider is not assigned.");
        }
    }
}