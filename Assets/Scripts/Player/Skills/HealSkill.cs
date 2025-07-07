namespace Skills
{
    public class HealSkill:Skill
    {
        public int healAmount = 1;

        public void ChangeHealAmount(int healValue)
        {
            healAmount = healValue;
        }

        public override void UseSkill()
        {
            base.UseSkill();
            player.AddHealth(healAmount);
        }
    }
}