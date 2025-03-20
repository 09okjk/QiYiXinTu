using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class QuestDatabase : MonoBehaviour
{
    public static QuestDatabase Instance;
    [SerializeField] private List<QuestData> quests = new List<QuestData>();
    private readonly Dictionary<string, QuestData> questDictionary = new Dictionary<string, QuestData>();
    
    private void Awake()
    {
        Instance = this;
        foreach (QuestData quest in quests)
        {
            if (quest != null && !questDictionary.ContainsKey(quest.questID))
            {
                questDictionary.Add(quest.questID, quest);
            }
        }
    }
    
    public QuestData GetQuest(string questID)
    {
        return questDictionary.GetValueOrDefault(questID);
    }
    
#if UNITY_EDITOR
    [ContextMenu("自动加载所有任务")]
    private void AutoLoadQuests()
    {
        quests.Clear();
        string[] guids = AssetDatabase.FindAssets("t:QuestData", new[] {"Assets/ScriptableObjects/Quests"});
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            QuestData quest = AssetDatabase.LoadAssetAtPath<QuestData>(path);
            if (quest != null)
            {
                quests.Add(quest);
            }
        }
        EditorUtility.SetDirty(this);
    }
#endif
}