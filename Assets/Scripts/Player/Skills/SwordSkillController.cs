using System;
using Manager;
using UnityEngine;

namespace Skills
{
    public class SwordSkillController : MonoBehaviour
    {
        private Animator animator;
        private Rigidbody2D rb;
        private CircleCollider2D circleCollider2D;
        
        private Player player;

        private void Awake()
        {
            animator = GetComponentInChildren<Animator>();
            rb = GetComponent<Rigidbody2D>();
            circleCollider2D = GetComponent<CircleCollider2D>();
            player = PlayerManager.Instance.player;
        }

        public void SetUpSword(Vector2 dir, float gravity)
        {
            rb.linearVelocity = new Vector2(dir.x * player.FacingDirection, dir.y);            
            rb.gravityScale = gravity;
        }
    }
}