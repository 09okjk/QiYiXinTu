using UnityEngine;

public class SkeletonStunnedState:EnemyState
{
    private Skeleton skeleton;
    public SkeletonStunnedState(Enemy enemyBase, EnemyStateMachine stateMachine, string animBoolName,Skeleton skeleton)
        : base(enemyBase, stateMachine, animBoolName)
    {
        this.skeleton = skeleton;
    }

    public override void Enter()
    {
        base.Enter();
        
        skeleton.EntityFX.InvokeRepeating("RedColorBlink", 0, 0.1f);
        
        stateTimer = skeleton.stunnedDuration;
        rb.linearVelocity = new Vector2(-skeleton.FacingDirection * skeleton.stunnedDirection.x, skeleton.stunnedDirection.y);
    }

    public override void Update()
    {
        base.Update();
        
        if (stateTimer <= 0)
        {
            stateMachine.ChangeState(skeleton.IdleState);
        }
    }

    public override void Exit()
    {
        base.Exit();
        
        skeleton.EntityFX.Invoke("CancelRedColorBlink", 0);
    }
}