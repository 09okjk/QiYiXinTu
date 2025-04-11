using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAirState : PlayerState
{
    public PlayerAirState(Player player, PlayerStateMachine stateMachine, string animBoolName) : base(player, stateMachine, animBoolName)
    {
    }

    public override void Enter()
    {
        base.Enter();
    }

    public override void Update()
    {
        base.Update();
        
        // 检测到地面时切换到Idle状态
        if (Player.IsGroundDetected())
        {
            StateMachine.ChangeState(Player.IdleState);
        }

        // 检测到墙壁时切换到墙壁滑行状态
        // if (Player.IsWallDetected())
        // {
        //     stateMachine.ChangeState(Player.WallSlideState);
        // }
        
        // 空中移动控制
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
