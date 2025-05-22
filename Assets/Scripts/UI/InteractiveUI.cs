using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public enum InteractionType
    {
        None, // 无交互 
        Talk, // 交谈
        PickUp, // 拾取物品
        Use, // 使用物品
        Open, // 打开
        Close // 关闭
    }
    
    public class InteractiveUI:MonoBehaviour
    {
        public Image interactImage; // 交互按钮
        public InteractionType interactionType = InteractionType.None; // 交互类型
        public string interactionValue; // 交互值
        
        private void Start()
        {
            interactImage.gameObject.SetActive(false); // 初始隐藏交互按钮
        }
        
        void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
            {
                // 显示交互按钮
                interactImage.gameObject.SetActive(true);
            }
        }
        
        void OnTriggerExit2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
            {
                // 隐藏交互按钮
                interactImage.gameObject.SetActive(false);
            }
        }

        private void OnInteractButtonClicked()
        {
            switch (interactionType)
            {
                case InteractionType.Talk:
                    // 处理交谈逻辑
                    Debug.Log("Talk interaction triggered.");
                    // 这里可以添加对话框的显示逻辑
                    DialogueManager.Instance.StartDialogueByID(interactionValue);
                    break;
                case InteractionType.PickUp:
                    // 处理���取物品逻辑
                    Debug.Log("Pick up interaction triggered.");
                    break;
                case InteractionType.Use:
                    // 处理使用物品逻辑
                    Debug.Log("Use interaction triggered.");
                    break;
                case InteractionType.Open:
                    // 处理打开逻辑
                    Debug.Log("Open interaction triggered.");
                    break;
                case InteractionType.Close:
                    // 处理关闭逻辑
                    Debug.Log("Close interaction triggered.");
                    break;
                default:
                    Debug.Log("No valid interaction type selected.");
                    break;
            }
        }
    }
}