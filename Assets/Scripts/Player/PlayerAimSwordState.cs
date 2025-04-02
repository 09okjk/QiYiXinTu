using UnityEngine;

public class PlayerAimSwordState: PlayerGroundState
{
    public PlayerAimSwordState(Player player, PlayerStateMachine stateMachine, string animBoolName) : base(player, stateMachine, animBoolName)
    {
    }

    public override void Enter()
    {
        base.Enter();
        Player.skillManager.swordSkill.DotsActive(true);
    }

    public override void Update()
    {
        base.Update();
        
        if (Input.GetKeyUp(KeyCode.Mouse1))
        {
            StateMachine.ChangeState(Player.ThrowSwordState);
        }
    }

    public override void Exit()
    {
        base.Exit();
    }
}