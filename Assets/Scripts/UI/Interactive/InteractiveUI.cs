using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public enum InteractionType
    {
        None, // 无交互 
        Talk, // 交谈
        GetItem, // 获得物品
        Use, // 使用物品
        Open, // 打开
        Close, // 关闭
        Activate, // 激活
    }
    
    public class InteractiveUI:MonoBehaviour
    {
        public string interactionName; // 交互名称
        public SpriteRenderer interactImage; // 交互按钮
        public InteractionType interactionType = InteractionType.None; // 交互类型
        public string interactionValue; // 交互后触发值
        public bool isActive = true; // 是否激活交互
        protected virtual void Awake()
        {
            isActive = GameStateManager.Instance.GetFlag("CanInteract_" + interactionName);
        }

        protected virtual void Start()
        {
            interactImage.gameObject.SetActive(false); // 初始隐藏交互按钮
        }

        protected virtual void Update()
        {
            if(!isActive)
                gameObject.SetActive(false); // 如果交互按钮被禁用，则隐藏它
        }

        protected virtual void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
            {
                // 显示交互按钮
                interactImage.gameObject.SetActive(true);
            }
        }
        
        protected virtual void OnTriggerExit2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
            {
                // 隐藏交互按钮
                interactImage.gameObject.SetActive(false);
            }
        }
        
        protected void SetActive(bool active)
        {
            isActive = active;
            GameStateManager.Instance.SetFlag("CanInteract_" + interactionName, active);
        }

        protected void OnInteractButtonClicked()
        {
            interactImage.gameObject.SetActive(false); // 隐藏交互按钮
            SetActive(false); // 禁用交互按钮
            switch (interactionType)
            {
                case InteractionType.Talk:
                    // 处理交谈逻辑
                    Debug.Log("Talk interaction triggered.");
                    // 这里可以添加对话框的显示逻辑
                    DialogueManager.Instance.StartDialogueByID(interactionValue);
                    gameObject.SetActive(false); // 隐藏交互按钮
                    break;
                case InteractionType.GetItem:
                    // 处理获得物品逻辑
                    Debug.Log("Pick up interaction triggered.");
                    InventoryManager.Instance.AddItemById(interactionValue);
                    GameStateManager.Instance.SetFlag("CanInteract_" + interactionName, false);
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
                case InteractionType.Activate:
                    Debug.Log("Activate interaction triggered.");
                    // 处理激活逻辑
                    // 找到子物体中名字是interactionValue的物体，并激活它
                    Transform targetObject = transform.Find(interactionValue);
                    if (targetObject != null)
                    {
                        targetObject.gameObject.SetActive(true);
                        Debug.Log($"Activated {interactionValue}.");
                    }
                    else
                    {
                        Debug.LogWarning($"No child object found with name {interactionValue}.");
                    }
                    break;
                default:
                    Debug.Log("No valid interaction type selected.");
                    break;
            }
        }
    }
}