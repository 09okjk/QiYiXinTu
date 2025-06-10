namespace NpcNew
{
    /// <summary>
    /// NPC状态类型
    /// </summary>
    public enum NPCStateType
    {
        Idle,       // 空闲
        Move,       // 移动
        Interact,   // 交互中
        Follow,     // 跟随
        Anxious,    // 焦虑(特殊状态)
        Disabled    // 禁用
    }

    /// <summary>
    /// NPC类型
    /// </summary>
    public enum NPCType
    {
        General,    // 通用NPC
        Story,      // 剧情NPC
        Merchant,   // 商人NPC
        Guide       // 向导NPC
    }

    /// <summary>
    /// 交互类型
    /// </summary>
    public enum InteractionType
    {
        Dialogue,   // 对话
        Trade,      // 交易
        Quest,      // 任务
        Follow      // 跟随
    }
}