using UnityEngine;

public class Skeleton : Enemy
{

    #region States

    public SkeletonIdleState IdleState { get; private set; }
    public SkeletonMoveState MoveState { get; private set; }
    public SkeletonBattleState BattleState { get; private set; }
    public SkeletonAttackState AttackState { get; private set; }
    public SkeletonStunnedState StunnedState { get; private set; }
    public SkeletonHurtState HurtState { get; private set; }
    public SkeletonDeathState DeathState { get; private set; }

    #endregion



    protected override void Awake()
    {
        base.Awake();

        IdleState = new SkeletonIdleState(this, stateMachine, "Idle", this);
        MoveState = new SkeletonMoveState(this, stateMachine, "Move", this);
        BattleState = new SkeletonBattleState(this, stateMachine, "Move", this);
        AttackState = new SkeletonAttackState(this, stateMachine, "Attack", this);
        StunnedState = new SkeletonStunnedState(this, stateMachine, "Stunned", this);
        HurtState = new SkeletonHurtState(this, stateMachine, "Hurt", this);
        DeathState = new SkeletonDeathState(this, stateMachine, "Death", this);
    }

    protected override void Start()
    {
        base.Start();
        stateMachine.Initialize(IdleState);
    }

    protected override void Update()
    {
        base.Update();

        if (Input.GetKeyDown(KeyCode.U))
        {
            stateMachine.ChangeState(StunnedState);
        }
    }

    public override bool CanBeStunned()
    {
        if (base.CanBeStunned())
        {
            stateMachine.ChangeState(StunnedState);
            return true;
        }

        return false;
    }

    public override void Damage(float damage)
    {
        base.Damage(damage);

        if (baseData.CurrentHealth <= 0)
        {
            baseData.CurrentHealth = 0;
            stateMachine.ChangeState(DeathState);
            return;
        }
        
        if (stateMachine.currentState == HurtState)
        {
            return;
        }

        stateMachine.ChangeState(HurtState);
    }
}