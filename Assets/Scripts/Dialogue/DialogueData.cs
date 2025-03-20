using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Dialogue", menuName = "Dialogue/Dialogue Data")]
public class DialogueData : ScriptableObject
{
    public string dialogueID;
    public List<DialogueNode> nodes = new List<DialogueNode>();
}

[Serializable]
public class DialogueNode
{
    public string text;
    public string speakerID; // 新增：对话发言者的ID
    public string speakerName; // 新增：对话发言者的名字
    public int nextNodeIndex; // 新增：下一个对话节点的索引
    public string speakerPosition; // 新增：对话发言者的位置
    public List<DialogueChoice> choices = new List<DialogueChoice>();
}

[Serializable]
public class DialogueChoice
{
    public string text;
    public int nextNodeIndex;
}