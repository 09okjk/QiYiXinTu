using System;
using Manager;
using UnityEngine;

namespace UI
{
    public class UIAnimationTrigger:MonoBehaviour
    {
        public void OnMoveAnimationFinished()
        {
            MainMenuManager.Instance.EnableAnimator();
        }
        
        public void OnAllAnimationFinished()
        {
            Debug.Log("All animation finished - 准备打开存档面板");
            try {
                MenuManager.Instance.OpenSavePanel();
                Debug.Log("存档面板打开完成");
            }
            catch (Exception e) {
                Debug.LogError($"打开存档面板时出错: {e.Message}\n{e.StackTrace}");
            }
        }
        
        public void TriggerDialogue(string dialogueID)
        {
            Debug.Log($"TriggerDialogue: {dialogueID}");
            DialogueManager.Instance.StartDialogueByID(dialogueID);
            GameUIManager.Instance.StopSceneAnimation();
            if (dialogueID == "lu_first_dialogue")
            {
                GameUIManager.Instance.StopLuWeekUpAnimation();
            }
        }

        public void ActivatePlayerAndNpc(string npcID)
        {
            GameStateManager.Instance.SetPlayerPointType(PlayerPointType.Right);
            PlayerManager.Instance.player.gameObject.SetActive(true);
            GameUIManager.Instance.PlayLuSleepAnimation();
        }
    }
}