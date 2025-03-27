using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerIdleState : PlayerGroundState
{
    public PlayerIdleState(Player player, PlayerStateMachine stateMachine, string animBoolName) : base(player, stateMachine, animBoolName)
    {
        
    }

    public override void Enter()
    {
        base.Enter();
        
        Player.ZeroVelocity();
    }

    public override void Update()
    {
        base.Update();
        
        if (xInput != 0 && !Player.isBusy)
        {
            StateMachine.ChangeState(Player.MoveState);
        }
    }

    public override void Exit()
    {
        base.Exit();
    }
}
