using UnityEngine;

public class NPCState
{
    protected NPCStateMachine StateMachine;
    protected NPC Npc;
    protected Rigidbody2D Rb;
    
    private string _animBoolName;
    protected float StateTimer;
    protected bool TriggerCalled;
    
    public NPCState(NPC npc, NPCStateMachine stateMachine, string animBoolName)
    {
        this.Npc = npc;
        this.StateMachine = stateMachine;
        this._animBoolName = animBoolName;
    }
    
    public virtual void Enter()
    {
        if (Npc == null)
        {
            Debug.LogError("NPC对象为空");
            return;
        }
    
        if (Npc.Anim == null)
        {
            // Debug.LogError("NPC的Anim组件为空");
            Npc.Anim = Npc.GetComponent<Animator>();
            // return;
        }
    
        Npc.Anim.SetBool(_animBoolName, true);
        Rb = Npc.Rb;
        TriggerCalled = false;
    }
    
    public virtual void Update()
    {
        StateTimer -= Time.deltaTime;
        Npc.Anim.SetFloat("yVelocity", Rb.linearVelocity.y);
    }
    
    public virtual void Exit()
    {
        Npc.Anim.SetBool(_animBoolName, false);
    }
    
    public virtual void AnimationFinishTrigger()
    {
        TriggerCalled = true;
    }
}