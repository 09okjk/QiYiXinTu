using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerWallJumpState : PlayerState
{
    public PlayerWallJumpState(Player player, PlayerStateMachine stateMachine, string animBoolName) : base(player, stateMachine, animBoolName)
    {
    }

    public override void Enter()
    {
        base.Enter();
        StateTimer = .4f;
        Player.SetVelocity(Player.wallJumpForce * -Player.FacingDirection, Player.jumpForce);
    }

    public override void Update()
    {
        base.Update();

        if (StateTimer < 0)
        {
            StateMachine.ChangeState(Player.AirState);
        }

        if (Player.IsGroundDetected())
        {
            StateMachine.ChangeState(Player.IdleState);
        }
    }

    public override void Exit()
    {
        base.Exit();
    }
}
