using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace UI
{
    public class DialogueTrigger:MonoBehaviour
    {
        public GameObject NextLevelGameObject; // 下一个关卡的GameObject
        public GameObject NextLevelGameObject2; // 第二个下一个关卡的GameObject
        private void Start()
        {
            DialogueManager.Instance.OnDialogueEnd += OnDialogueEnd;
            NextLevelGameObject2.SetActive(false);
            if(NextLevelGameObject != null)
            {
                if (SceneManager.GetActiveScene().name == "DinnerHall" && GameStateManager.Instance.GetFlag("finish_Dinner") 
                    || SceneManager.GetActiveScene().name == "DaYinDian" && GameStateManager.Instance.GetFlag("finish_DaYinDian"))
                {
                    NextLevelGameObject2.SetActive(true);
                }
            }
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
                GameStateManager.Instance.SetFlag("close_TimeGate",true);
                GameStateManager.Instance.SetFlag("show_FangHuaigu",true);
                GameStateManager.Instance.SetFlag("canInteract_lide-05-1",true);
                NextLevelGameObject.SetActive(true);
            }else if (dialogueId == "shitang-05-2")
            {
                // 显示下一个关卡的GameObject
                GameStateManager.Instance.SetFlag("finish_Dinner",true);
                NextLevelGameObject.SetActive(true);
            }
            else if (dialogueId == "dayin-06-1")
            {
                GameStateManager.Instance.SetFlag("finish_DaYinDian",true);
                GameStateManager.Instance.SetFlag("canEnter_outside3",true);
                NextLevelGameObject.SetActive(true);
            }
        }
    }
}