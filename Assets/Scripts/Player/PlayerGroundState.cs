using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerGroundState : PlayerState
{
    public PlayerGroundState(Player player, PlayerStateMachine stateMachine, string animBoolName) : base(player, stateMachine, animBoolName)
    {
    }

    public override void Enter()
    {
        base.Enter();
    }

    public override void Update()
    {
        base.Update();

        if (Input.GetKeyDown(KeyCode.Q))
        {
            StateMachine.ChangeState(Player.CounterAttackState);
        }

        if (Input.GetKey(KeyCode.Mouse0))
        {
            StateMachine.ChangeState(Player.PrimaryAttackState);
        }
        
        if (!Player.IsGroundDetected())
        {
            StateMachine.ChangeState(Player.AirState);
        }
        
        if(Input.GetKeyDown(KeyCode.Space) && Player.IsGroundDetected())
        {
            StateMachine.ChangeState(Player.JumpState);
        }
    }

    public override void Exit()
    {
        base.Exit();
    }
}
