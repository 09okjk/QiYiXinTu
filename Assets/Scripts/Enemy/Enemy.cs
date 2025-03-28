using System;
using UnityEngine;

public class Enemy : Entity
{
    
    public EnemyStateMachine stateMachine { get; private set; }

    private void Awake()
    {
        base.Awake();
        stateMachine = new EnemyStateMachine();
    }

    private void Update()
    {
        base.Update();
        stateMachine.currentState.Update();
    }
}
