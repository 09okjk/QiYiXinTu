using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Dialogue", menuName = "Dialogue/Dialogue Data")]
public class DialogueData : ScriptableObject
{
    public List<DialogueNode> nodes = new List<DialogueNode>();
}

[Serializable]
public class DialogueNode
{
    public string text;
    public List<DialogueChoice> choices = new List<DialogueChoice>();
}

[Serializable]
public class DialogueChoice
{
    public string text;
    public int nextNodeIndex;
}