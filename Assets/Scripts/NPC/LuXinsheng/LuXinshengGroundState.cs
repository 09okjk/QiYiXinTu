using Manager;
using UnityEngine;

public class LuXinshengGroundState: NPCState
{
    protected LuXinsheng LuXinsheng;
    protected Transform player;
    
    public LuXinshengGroundState(NPC npc, NPCStateMachine stateMachine, string animBoolName,LuXinsheng luXinsheng) : base(npc, stateMachine, animBoolName)
    {
        this.LuXinsheng = luXinsheng;
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