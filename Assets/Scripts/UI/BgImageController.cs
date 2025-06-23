using System;
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

        private void OnDestroy()
        {
            DialogueManager.Instance.OnDialogueEnd -= OnDialogueEnd;
        }

        private void OnDialogueEnd(string dialogueId)
        {
            if (dialogueId == "silence_dialogue")
            {
                bgImage.gameObject.SetActive(false);
                bgImageOpen.SetActive(true);
                bgImageGirl.SetActive(true);
                bgImageAnimator.gameObject.SetActive(true);
                
                DialogueManager.Instance.StartDialogueByID("silence_in_dialogue");
            }
            
            if (dialogueId == "silence_in_dialogue")
            {
                bgImageAnimator.SetBool("IsOpen", true);
                TimeGate.SetActive(true);
            }
        }
    }
}