using Core;
using UnityEngine;

[CreateAssetMenu(fileName = "New Player", menuName = "Characters/Player Data")]
public class PlayerData: EntityData
{
    [Header("Player Info")]       
    public string playerID;
    public string playerName;
    
    [Header("Movement Info")]
    public float moveSpeed = 8f;
    public float jumpForce = 12f;
    public float wallJumpForce = 5f;
    public float idleToMoveTransitionTime = 0.0001f;

    [Header("Attack Info")] 
    public float comboTimeWindow = 0.2f;
    public float counterAttackDuration = 2f;
    public float attackDamage = 10f;
}