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
    }
}