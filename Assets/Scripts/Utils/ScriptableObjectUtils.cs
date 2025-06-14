using UnityEngine;
using System.Collections.Generic;

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
            
            // 确保运行时副本不会被保存到资源中
            Object.DontDestroyOnLoad(copy);
            
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
        
        /// <summary>
        /// 批量创建ScriptableObject运行时副本
        /// </summary>
        /// <typeparam name="T">ScriptableObject类型</typeparam>
        /// <param name="originals">原始对象数组</param>
        /// <returns>运行时副本数组</returns>
        public static T[] CreateRuntimeCopies<T>(T[] originals) where T : ScriptableObject
        {
            if (originals == null || originals.Length == 0) return null;
            
            T[] copies = new T[originals.Length];
            for (int i = 0; i < originals.Length; i++)
            {
                copies[i] = CreateRuntimeCopy(originals[i]);
            }
            
            return copies;
        }
        
        /// <summary>
        /// 重置ScriptableObject数据到原始状态
        /// </summary>
        /// <typeparam name="T">ScriptableObject类型</typeparam>
        /// <param name="original">原始对象</param>
        /// <param name="current">当前使用的副本</param>
        public static void ResetToOriginal<T>(T original, T current) where T : ScriptableObject
        {
            if (original == null || current == null) return;
            
            // 使用JSON序列化来复制数据状态
            string originalJson = JsonUtility.ToJson(original);
            JsonUtility.FromJsonOverwrite(originalJson, current);
        }
        
        /// <summary>
        /// 专门为NPC数据创建深度副本
        /// </summary>
        /// <param name="original">原始NPC数据</param>
        /// <returns>深度复制的NPC数据</returns>
        public static NPCData CreateNPCDataCopy(NPCData original)
        {
            if (original == null) return null;
            
            var copy = CreateRuntimeCopy(original);
            
            // 重置NPC特定的运行时状态
            // 注意：这里保持原始的配置数据，但重置运行时状态
            // 具体的重置逻辑可以根据NPCData的结构进行调整
            
            return copy;
        }
        
        /// <summary>
        /// 专门为任务数据创建深度副本
        /// </summary>
        /// <param name="original">原始任务数据</param>
        /// <returns>深度复制的任务数据</returns>
        public static QuestData CreateQuestDataCopy(QuestData original)
        {
            if (original == null) return null;
            
            var copy = CreateRuntimeCopy(original);
            
            // 重置任务特定的状态
            copy.isCompleted = false; // 确保新游戏时任务未完成
            
            return copy;
        }
        
        /// <summary>
        /// 专门为新闻数据创建深度副本
        /// </summary>
        /// <param name="original">原始新闻数据</param>
        /// <returns>深度复制的新闻数据</returns>
        public static News.NewsData CreateNewsDataCopy(News.NewsData original)
        {
            if (original == null) return null;
            
            var copy = CreateRuntimeCopy(original);
            
            // 重置新闻特定的状态
            copy.isRead = false; // 确保新游戏时新闻未读
            
            return copy;
        }
        
        /// <summary>
        /// 专门为玩家数据创建深度副本
        /// </summary>
        /// <param name="original">原始玩家数据</param>
        /// <returns>深度复制的玩家数据</returns>
        public static PlayerData CreatePlayerDataCopy(PlayerData original)
        {
            if (original == null) return null;
            
            var copy = CreateRuntimeCopy(original);
            
            // 重置玩家特定的状态到默认值
            copy.CurrentHealth = copy.MaxHealth; // 重置生命值
            copy.CurrentMana = copy.MaxMana; // 重置法力值
            // 如果有其他需要重置的属性，在这里添加
            
            return copy;
        }
        
        /// <summary>
        /// 创建带有特定重置逻辑的运行时副本
        /// </summary>
        /// <typeparam name="T">ScriptableObject类型</typeparam>
        /// <param name="original">原始对象</param>
        /// <param name="resetAction">重置逻辑</param>
        /// <returns>运行时副本</returns>
        public static T CreateRuntimeCopyWithReset<T>(T original, System.Action<T> resetAction = null) where T : ScriptableObject
        {
            if (original == null) return null;
            
            var copy = CreateRuntimeCopy(original);
            
            // 应用自定义重置逻辑
            resetAction?.Invoke(copy);
            
            return copy;
        }
        
        /// <summary>
        /// 检查对象是否为运行时副本
        /// </summary>
        /// <param name="obj">要检查的对象</param>
        /// <returns>是否为运行时副本</returns>
        public static bool IsRuntimeCopy(ScriptableObject obj)
        {
            return obj != null && obj.name.EndsWith("_RuntimeCopy");
        }
        
        /// <summary>
        /// 安全销毁运行时副本
        /// </summary>
        /// <param name="runtimeCopy">运行时副本</param>
        public static void SafeDestroyRuntimeCopy(ScriptableObject runtimeCopy)
        {
            if (runtimeCopy != null && IsRuntimeCopy(runtimeCopy))
            {
                Object.DestroyImmediate(runtimeCopy);
            }
        }
        
        /// <summary>
        /// 批量安全销毁运行时副本
        /// </summary>
        /// <param name="runtimeCopies">运行时副本集合</param>
        public static void SafeDestroyRuntimeCopies<T>(IEnumerable<T> runtimeCopies) where T : ScriptableObject
        {
            if (runtimeCopies == null) return;
            
            foreach (var copy in runtimeCopies)
            {
                SafeDestroyRuntimeCopy(copy);
            }
        }
    }
}