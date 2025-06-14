using UnityEngine;

namespace Utils
{
    public static class ScriptableObjectUtils
    {
        /// <summary>
        /// 创建 ScriptableObject 的运行时副本
        /// </summary>
        /// <typeparam name="T">ScriptableObject 类型</typeparam>
        /// <param name="original">原始对象</param>
        /// <returns>运行时副本</returns>
        public static T CreateRuntimeCopy<T>(T original) where T : ScriptableObject
        {
            if (original == null) return null;
            
            var copy = Object.Instantiate(original);
            copy.name = original.name + "_RuntimeCopy";
            return copy;
        }
        
        /// <summary>
        /// 深度复制对话数据，包括所有节点和选择
        /// </summary>
        /// <param name="original">原始对话数据</param>
        /// <returns>深度复制的对话数据</returns>
        public static DialogueData CreateDialogueDataCopy(DialogueData original)
        {
            if (original == null) return null;
            
            var copy = ScriptableObject.CreateInstance<DialogueData>();
            copy.name = original.name + "_RuntimeCopy";
            copy.dialogueID = original.dialogueID;
            copy.state = DialogueState.WithOutStart; // 重置状态
            copy.currentNodeID = string.Empty; // 重置当前节点ID
            copy.nodes = new System.Collections.Generic.List<DialogueNode>();
            
            // 深度复制所有节点
            foreach (var originalNode in original.nodes)
            {
                var nodeCopy = new DialogueNode
                {
                    nodeID = originalNode.nodeID,
                    text = originalNode.text,
                    nextNodeID = originalNode.nextNodeID,
                    questID = originalNode.questID,
                    isFollow = originalNode.isFollow,
                    conditionType = originalNode.conditionType,
                    conditionValue = originalNode.conditionValue,
                    rewardIDs = new System.Collections.Generic.List<string>(originalNode.rewardIDs),
                    choices = new System.Collections.Generic.List<DialogueChoice>(),
                    speaker = new DialogueSpeaker
                    {
                        speakerID = originalNode.speaker.speakerID,
                        speakerName = originalNode.speaker.speakerName,
                        speakerType = originalNode.speaker.speakerType,
                        emotion = originalNode.speaker.emotion
                    }
                };
                
                // 深度复制选择
                foreach (var originalChoice in originalNode.choices)
                {
                    nodeCopy.choices.Add(new DialogueChoice
                    {
                        text = originalChoice.text,
                        nextNodeID = originalChoice.nextNodeID
                    });
                }
                
                copy.nodes.Add(nodeCopy);
            }
            
            return copy;
        }
    }
}