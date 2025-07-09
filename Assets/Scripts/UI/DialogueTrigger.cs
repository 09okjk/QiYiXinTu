using System;
using UnityEngine;

namespace UI
{
    public class DialogueTrigger:MonoBehaviour
    {
        public GameObject NextLevelGameObject; // 下一个关卡的GameObject
        private void Start()
        {
            DialogueManager.Instance.OnDialogueEnd += OnDialogueEnd;
        }

        private void OnDestroy()
        {
            DialogueManager.Instance.OnDialogueEnd -= OnDialogueEnd;
        }

        private void OnDialogueEnd(string dialogueId)
        {
            // 检查对话ID是否为特定值
            if (dialogueId == "rift_1955_dialogue")
            {
                // 显示下一个关卡的GameObject
                NextLevelGameObject.SetActive(true);
            }
        }
    }
}