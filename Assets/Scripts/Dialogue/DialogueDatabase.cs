using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class DialogueDatabase : MonoBehaviour
{
    public static DialogueDatabase Instance { get; private set; }

    [SerializeField] private List<DialogueData> dialogues = new List<DialogueData>();
    private readonly Dictionary<string, DialogueData> dialogueDictionary = new Dictionary<string, DialogueData>();

    private void Awake()
    {
        Instance = this;
        foreach (DialogueData dialogue in dialogues)
        {
            if (dialogue != null)
            {
                dialogueDictionary.TryAdd(dialogue.dialogueID, dialogue);
            }
        }
    }

    public DialogueData GetDialogue(string dialogueName)
    {
        return dialogues.Find(dialogue => dialogue.dialogueID == dialogueName);
    }

    public Dictionary<string, DialogueData> GetDialogueDictionary()
    {
        return dialogueDictionary;
    }
    
    public void SetDictionary(Dictionary<string, DialogueData> dictionary)
    {
        dialogueDictionary.Clear();
        foreach (KeyValuePair<string, DialogueData> pair in dictionary)
        {
            dialogueDictionary.TryAdd(pair.Key, pair.Value);
        }
    }
    
#if UNITY_EDITOR
    [ContextMenu("自动加载所有对话")]
    private void AutoLoadDialogues()
    {
        dialogues.Clear();
        string[] guids = AssetDatabase.FindAssets("t:DialogueData", new[] {"Assets/ScriptableObjects/Dialogues"});
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            DialogueData dialogue = AssetDatabase.LoadAssetAtPath<DialogueData>(path);
            if (dialogue != null)
            {
                dialogues.Add(dialogue);
            }
        }
        EditorUtility.SetDirty(this);
    }
#endif
}