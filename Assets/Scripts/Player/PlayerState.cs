
using UnityEngine;

public class PlayerState
{
    protected PlayerStateMachine StateMachine;
    protected Player Player;
    protected Rigidbody2D Rb;

    protected float xInput;
    protected float yInput;
    private string _animBoolName;

    protected float StateTimer;
    protected bool TriggerCalled;

    public PlayerState(Player player, PlayerStateMachine stateMachine, string animBoolName)
    {
        this.Player = player;
        this.StateMachine = stateMachine;
        this._animBoolName = animBoolName;
    }
    
    public virtual void Enter()
    {
        Player.Anim.SetBool(_animBoolName, true);
        Rb = Player.Rb;
        TriggerCalled = false;
    }

    public virtual void Update()
    {
        StateTimer -= Time.deltaTime;
        xInput = Input.GetAxisRaw("Horizontal");
        yInput = Input.GetAxisRaw("Vertical");
        Player.Anim.SetFloat("yVelocity", Rb.linearVelocity.y);
    }
    
    public virtual void Exit()
    {
        Player.Anim.SetBool(_animBoolName, false);
    }
    
    public virtual void AnimationFinishTrigger()
    {
        TriggerCalled = true;
    }
}
