using UnityEngine;

namespace Skills
{
    public class DashSkill:Skill
    {
        public float dashSpeed = 30f;
        public float dashDuration = 0.2f;
        
        public override void UseSkill()
        {
            base.UseSkill();
            
            Debug.Log("Dash skill used");
        }
    }
}