using UnityEngine;

/// <summary>
/// LuXinsheng对话配置
/// </summary>
[CreateAssetMenu(fileName = "LuXinshengDialogueConfig", menuName = "Characters/LuXinsheng Dialogue Config")]
public class LuXinshengDialogueConfig : ScriptableObject
{
    [Header("对话ID配置")]
    public string firstDialogueID = "lu_first_dialogue";
    public string fightDialogueID = "fight_dialogue";
    public string lideDialogueID = "lide_dialogue";
    
    [Header("特殊场景配置")]
    public string[] specialScenes = { "outside1" };
}