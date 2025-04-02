using System;
using Manager;
using UnityEngine;

namespace Skills
{
    public class Skill:MonoBehaviour
    {
        [SerializeField] protected float coolDown;
        protected float coolDownTimer;

        protected Player player;

        protected virtual void Start()
        {
            player = PlayerManager.Instance.player;
        }

        protected virtual void Update()
        {
            coolDownTimer -= Time.deltaTime;
        }
        
        public virtual bool CanUseSkill()
        {
            if (coolDownTimer < 0)
            {
                return true;
            }
            Debug.Log("skill is on CoolDown");
            return false;
        }
        
        public virtual void UseSkill()
        {
            if (CanUseSkill())
            {
                coolDownTimer = coolDown;
                Debug.Log("Using skill");
            }
            else
            {
                Debug.Log("skill is on CoolDown");
            }
        }
    }
}