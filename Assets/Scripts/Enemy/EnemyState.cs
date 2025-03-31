using UnityEngine;

public class EnemyState
{
    protected EnemyStateMachine stateMachine;
    protected Enemy enemyBase;
    protected Rigidbody2D rb;
    
    private string animBoolName;
    
    protected float stateTimer;
    protected bool triggerCalled;
    
    
    public EnemyState(Enemy enemyBase, EnemyStateMachine stateMachine, string animBoolName)
    {
        this.enemyBase = enemyBase;
        this.stateMachine = stateMachine;
        this.animBoolName = animBoolName;
    }
    
    public virtual void Enter()
    {
        triggerCalled = false;
        rb = enemyBase.Rb;
        enemyBase.Anim.SetBool(animBoolName, true);
    }
    
    public virtual void Update()
    {
        stateTimer -= Time.deltaTime;
    }
    
    public virtual void Exit()
    {
        enemyBase.Anim.SetBool(animBoolName, false);
    }

    public virtual void AnimationFinishTrigger()
    {
        triggerCalled = true;
    }
}