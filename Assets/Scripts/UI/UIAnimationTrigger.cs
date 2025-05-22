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
        
        public void OnSceneAnimationFinished(int animationIndex)
        {
            Debug.Log($"OnSceneAnimationFinished: {animationIndex}");
            GameUIManager.Instance.PlaySceneAnimation(animationIndex);
        }
        
        public void TriggerDialogue(string dialogueID)
        {
            Debug.Log($"TriggerDialogue: {dialogueID}");
            DialogueManager.Instance.StartDialogueByID(dialogueID);
        }

        public void ActivatePlayer()
        {
            GameStateManager.Instance.SetPlayerPointType(PlayerPointType.Right);
            PlayerManager.Instance.SetPlayer();
        }
    }
}