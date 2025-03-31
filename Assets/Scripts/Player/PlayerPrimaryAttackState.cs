using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerPrimaryAttackState : PlayerState
{
    private int comboCount = 0;
    
    private float lastTimeAttacked = 0;
    
    public PlayerPrimaryAttackState(Player player, PlayerStateMachine stateMachine, string animBoolName) : base(player, stateMachine, animBoolName)
    {
    }

    public override void Enter()
    {
        base.Enter();

        if (comboCount > 2 || Time.time >= lastTimeAttacked * Player.comboTimeWindow)
        {
            comboCount = 0;
        }
        
        Player.Anim.SetInteger("ComboCount", comboCount);

        #region Choose Attack Direction
        
        float attackDir = Player.FacingDirection;

        if (xInput != 0)
        {
            attackDir = xInput;
        }

        #endregion
        
        Player.SetVelocity(Player.attackMovements[comboCount].x * attackDir, Player.attackMovements[comboCount].y);
        
        StateTimer = .1f;
    }

    public override void Update()
    {
        base.Update();

        if (StateTimer < 0)
        {
            Player.SetZeroVelocity();
        }

        if (TriggerCalled)
        {
            StateMachine.ChangeState(Player.IdleState);
        }
    }

    public override void Exit()
    {
        base.Exit();
        
        Player.StartCoroutine("BusyFor", .1f);
        
        comboCount++;
        lastTimeAttacked = Time.time;
    }
}
