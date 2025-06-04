public class GeneralNpc:NPC
{
    # region States
    internal GeneralNpcIdleState IdleState { get; set; }
    # endregion
    
    protected override void Awake()
    {
        base.Awake();
        
        IdleState = new GeneralNpcIdleState(this, stateMachine, "Idle", this);
    }
    
    protected void OnEnable()
    {
        DialogueManager.Instance.OnDialogueEnd += OnDialogueEnd;
    }
    
    
    protected void OnDisable()
    {
        DialogueManager.Instance.OnDialogueEnd -= OnDialogueEnd;
    }
}