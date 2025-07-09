using System;
using UnityEngine;

public enum AttackType
{
    Attack1 = 1,
    Attack2 = 2,
    Attack3 = 3,
    Attack4_1 = 4,
    Attack4_2 = 5,
    SkillAttack1_1 = 6,
    SkillAttack1_2 = 7
}
public class PlayerAttackCheckerManager:MonoBehaviour
{
    public PlayerAttackChecker SkillAttack1_1;
    public PlayerAttackChecker SkillAttack1_2;
    public PlayerAttackChecker Attack1;
    public PlayerAttackChecker Attack2;
    public PlayerAttackChecker Attack3;
    public PlayerAttackChecker Attack4_1;
    public PlayerAttackChecker Attack4_2;

    private void Awake()
    {
        // 确保所有攻击碰撞器初始状态为禁用
        DisableAllAttackColliders();
    }
    
    public void EnableAttackCollider(AttackType attackType, Vector2 direction = default(Vector2))
    {
        DisableAllAttackColliders();
        
        switch (attackType)
        {
            case AttackType.SkillAttack1_1:
                SkillAttack1_1.SetColliderState(true);
                if (direction != default(Vector2))
                {
                    SkillAttack1_1.SetColliderSize(direction);
                }
                break;
            case AttackType.SkillAttack1_2:
                SkillAttack1_2.SetColliderState(true);
                break;
            case AttackType.Attack1:
                Attack1.SetColliderState(true);
                break;
            case AttackType.Attack2:
                Attack2.SetColliderState(true);
                break;
            case AttackType.Attack3:
                Attack3.SetColliderState(true);
                break;
            case AttackType.Attack4_1:
                Attack4_1.SetColliderState(true);
                break;
            case AttackType.Attack4_2:
                Attack4_2.SetColliderState(true);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(attackType), attackType, null);
        }
    }

    public void DisableAllAttackColliders()
    {
        SkillAttack1_1.SetColliderState(false);
        SkillAttack1_2.SetColliderState(false);
        Attack1.SetColliderState(false);
        Attack2.SetColliderState(false);
        Attack3.SetColliderState(false);
        Attack4_1.SetColliderState(false);
        Attack4_2.SetColliderState(false);
    }
}