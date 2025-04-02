using System;
using Skills;
using UnityEngine;

namespace Manager
{
    public class SkillManager : MonoBehaviour
    {
        public static SkillManager Instance { get; private set; }

        public DashSkill dashSkill { get; private set; }
        public SwordSkill swordSkill { get; private set; }

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
            swordSkill = GetComponent<SwordSkill>();
        }
    }

}