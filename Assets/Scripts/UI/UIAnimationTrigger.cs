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
            MainMenuManager.Instance.EnterGame();
        }
        
        public void TriggerDialogue(string dialogueID)
        {
            Debug.Log($"TriggerDialogue: {dialogueID}");
            DialogueManager.Instance.StartDialogueByID(dialogueID);
            GameUIManager.Instance.StopSceneAnimation();
        }

        public void ActivatePlayerAndNpc(string npcID)
        {
            GameStateManager.Instance.SetPlayerPointType(PlayerPointType.Right);
            PlayerManager.Instance.SetPlayer();
            ShowNpc(npcID);
        }

        public void ShowNpc(string npcID)
        {
            NPCManager.Instance.ShowNpc(npcID);
        }
    }
}