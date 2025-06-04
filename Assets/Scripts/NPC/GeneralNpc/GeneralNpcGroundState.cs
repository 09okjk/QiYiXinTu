using Manager;
using UnityEngine;

public class GeneralNpcGroundState:NPCState
{
    protected GeneralNpc GeneralNpc;
    protected Transform player;
    public GeneralNpcGroundState(NPC npc, NPCStateMachine stateMachine, string animBoolName,GeneralNpc generalNpc) : base(npc, stateMachine, animBoolName)
    {
        this.GeneralNpc = generalNpc;
    }
    
    public override void Enter()
    {
        base.Enter();
        player = PlayerManager.Instance.player.transform;
    }

    public override void Update()
    {
        base.Update();
    }
    
    public override void Exit()
    {
        base.Exit();
    }
}