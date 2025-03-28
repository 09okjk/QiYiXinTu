using UnityEngine;

public class EnemyState
{
    protected EnemyStateMachine stateMachine;
    protected Enemy enemy;
    
    private string animBoolName;
    
    protected float stateTimer;
    protected bool triggerCalled;
    
    
    public EnemyState(Enemy enemy, EnemyStateMachine stateMachine, string animBoolName)
    {
        this.enemy = enemy;
        this.stateMachine = stateMachine;
        this.animBoolName = animBoolName;
    }
    
    public virtual void Enter()
    {
        triggerCalled = false;
        enemy.animator.SetBool(animBoolName, true);
    }
    
    public virtual void Update()
    {
        stateTimer -= Time.deltaTime;
    }
    
    public virtual void Exit()
    {
        enemy.animator.SetBool(animBoolName, false);
    }
}