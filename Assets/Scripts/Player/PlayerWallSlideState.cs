using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerWallSlideState : PlayerState
{
    public PlayerWallSlideState(Player player, PlayerStateMachine stateMachine, string animBoolName) : base(player, stateMachine, animBoolName)
    {
    }

    public override void Enter()
    {
        base.Enter();
    }

    public override void Update()
    {
        base.Update();
        
        if (Input.GetButtonDown("Jump"))
        {
            StateMachine.ChangeState(Player.WallJumpState);
            return;
        }
        
        if (yInput < 0)
        {
            Rb.linearVelocity = new Vector2(0,Rb.linearVelocity.y);
        }
        else
        {
            Rb.linearVelocity = new Vector2(0,Rb.linearVelocity.y * .7f);
        }
        
        if(xInput !=0 && Player.FacingDirection !=xInput )
        {
            StateMachine.ChangeState(Player.IdleState);
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
