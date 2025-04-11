using System.Collections.Generic;
using UnityEngine;

namespace Core
{
    [CreateAssetMenu(fileName = "New Base EntityData", menuName = "Characters/Entity Data")]
    public class EntityData : ScriptableObject
    {
        [Header("Basic Info")]
        public float MaxHealth;
        public float CurrentHealth;
        public float MaxMana;
        public float CurrentMana;

        [Header("Hurt Info")]
        public float InvincibleTime;
        
        [Header("Knockback Info")]
        public Vector2 knockbackDirection;
        public float KnockbackDuration;
        
        [Header("Inventory Info")]
        [SerializeField] public List<ItemData> items = new List<ItemData>();
    }
}