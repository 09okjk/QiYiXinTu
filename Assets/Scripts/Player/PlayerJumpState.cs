using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerJumpState : PlayerState
{
    public PlayerJumpState(Player player, PlayerStateMachine stateMachine, string animBoolName) : base(player, stateMachine, animBoolName)
    {
    }

    public override void Enter()
    {
        base.Enter();
        Rb.linearVelocity = new Vector2(Rb.linearVelocity.x, Player.jumpForce);
    }

    public override void Update()
    {
        base.Update();
        
        if(Rb.linearVelocity.y < 0)
        {
            StateMachine.ChangeState(Player.AirState);
        }
        
        if (xInput != 0)
        {
            Player.SetVelocity(Player.moveSpeed * xInput *.8f, Rb.linearVelocity.y);
        }
    }

    public override void Exit()
    {
        base.Exit();
    }
}
