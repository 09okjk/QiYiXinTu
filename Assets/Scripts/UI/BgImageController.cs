using System;
using UI.Puzzle;
using UnityEngine;

namespace UI
{
    public class BgImageController:MonoBehaviour
    {
        public GameObject bgImage;
        public GameObject bgImageOpen;
        public GameObject bgImageGirl; 
        public Animator bgImageAnimator;
        public GameObject TimeGate;
        public GameObject FianlObject;
        
        private void Awake()
        {
            if (bgImage == null ||bgImageOpen == null || bgImageGirl == null || bgImageAnimator == null)
            {
                Debug.LogError("BgImageController: Missing references in the inspector.");
                return;
            }
            
            bgImage.SetActive(true);
            
            bgImageOpen.SetActive(false);
            bgImageGirl.SetActive(false);
            bgImageAnimator.gameObject.SetActive(false);
        }

        private void Start()
        {
            DialogueManager.Instance.OnDialogueEnd += OnDialogueEnd;
        }

        private void Update()
        {
            if (GameStateManager.Instance.GetFlag("close_TimeGate"))
            {
                bgImageAnimator.SetBool("IsOpen", false);
                bgImage.gameObject.SetActive(false);
                bgImageGirl.SetActive(false);
                bgImageOpen.SetActive(true);
                bgImageAnimator.gameObject.SetActive(false);
                if(GameStateManager.Instance.GetFlag("show_FangHuaigu"))
                    FianlObject.SetActive(true);   
            }
            else if (GameStateManager.Instance.GetFlag("show_TimeGate"))
            {
                bgImageAnimator.SetBool("IsOpen", true);
                bgImageGirl.SetActive(false);
                TimeGate.SetActive(true);
            }
            else if (GameStateManager.Instance.GetFlag("silence_DoorOpen"))
            {
                bgImage.gameObject.SetActive(false);
                bgImageOpen.SetActive(true);
                bgImageGirl.SetActive(true);
                bgImageAnimator.gameObject.SetActive(true);
            }
        }

        private void OnDestroy()
        {
            DialogueManager.Instance.OnDialogueEnd -= OnDialogueEnd;
        }

        private void OnDialogueEnd(string dialogueId)
        {
            if (dialogueId == "silence_dialogue")
            {
                GameStateManager.Instance.SetFlag("silence_DoorOpen", true);
                DialogueManager.Instance.StartDialogueByID("silence_in_dialogue");
            }
            
            if (dialogueId == "silence_in_dialogue")
            {
                GameStateManager.Instance.SetFlag("silence_DoorOpen",false);
                GameStateManager.Instance.SetFlag("show_TimeGate",true);
                PuzzleManager.Instance.OpenPanel();
            }
        }
    }
}