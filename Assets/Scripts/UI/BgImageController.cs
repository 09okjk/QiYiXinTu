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
        public GameObject FinalObject;
        
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
            if (GameStateManager.Instance.GetFlag("close_TimeGate"))// 静音室开门，时间之门关闭，女孩消失
            {
                bgImageAnimator.SetBool("IsOpen", false);
                bgImageAnimator.gameObject.SetActive(false);
                bgImage.gameObject.SetActive(false);
                bgImageOpen.SetActive(true);
                bgImageGirl.SetActive(false);
                TimeGate.SetActive(false);
                if(GameStateManager.Instance.GetFlag("show_FangHuaigu"))
                    FinalObject.SetActive(true);   
            }
            else if (GameStateManager.Instance.GetFlag("show_TimeGate"))// 静音室开门，时间之门开启，女孩消失
            {
                bgImageAnimator.gameObject.SetActive(true);
                bgImageAnimator.SetBool("IsOpen", true);
                bgImage.gameObject.SetActive(false);
                bgImageOpen.SetActive(true);
                bgImageGirl.SetActive(false);
                TimeGate.SetActive(true);
            }
            else if (GameStateManager.Instance.GetFlag("silence_DoorOpen"))// 静音室开门，时间之门出现，女孩出现
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