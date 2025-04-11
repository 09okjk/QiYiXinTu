using Core;
using UnityEngine;

[CreateAssetMenu(fileName = "New Enemy", menuName = "Characters/Enemy Data")]
public class EnemyData:EntityData
{
    [Header("Enemy Info")]
    public string enemyID;
    public string enemyName;
    [TextArea] public string description;
    //public Sprite avatar;
    public EnemyType enemyType;
    
    [Header("Movement Info")]
    public float moveSpeed = 2f;
    public float idleTime = 2f;
    public float battleTime = 1f;
    
    
    [Header("Attack Info")]
    public float attackDistance =1.8f;
    public float attackCooldown = 2f;
    public float attackDamage = 10f;
    public float stunnedDuration = 1f;
    public Vector2 stunnedDirection;
}