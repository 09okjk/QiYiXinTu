using System.Collections;
using System.Collections.Generic;
using Manager;
using UnityEngine;

public class PlayerDashState : PlayerState
{
    public PlayerDashState(Player player, PlayerStateMachine stateMachine, string animBoolName) : base(player, stateMachine, animBoolName)
    {
    }

    public override void Enter()
    {
        base.Enter();
        StateTimer = SkillManager.Instance.dashSkill.dashDuration;
    }

    public override void Update()
    {
        base.Update();

        // if (!Player.IsGroundDetected() && Player.IsWallDetected())
        // {
        //     stateMachine.ChangeState(Player.WallSlideState);
        // }
        
        Player.SetVelocity(SkillManager.Instance.dashSkill.dashSpeed * Player.DashDir, 0);
        
        if (StateTimer < 0)
        {
            StateMachine.ChangeState(Player.IdleState);
        }
    }

    public override void Exit()
    {
        base.Exit();
        Player.SetVelocity(0,Rb.linearVelocity.y);
    }
}
