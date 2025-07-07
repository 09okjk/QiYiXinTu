using System;
using Skills;
using UnityEngine;

namespace Manager
{
    public class SkillManager : MonoBehaviour
    {
        public static SkillManager Instance { get; private set; }

        public DashSkill dashSkill { get; private set; }
        public AttackSkill attackSkill { get; private set; }
        public HealSkill healSkill { get; private set; }

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Debug.LogWarning("Multiple SkillManager instances found. Destroying duplicate.");
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            dashSkill = GetComponent<DashSkill>();
            attackSkill = GetComponent<AttackSkill>();
            healSkill = GetComponent<HealSkill>();
        }
    }

}