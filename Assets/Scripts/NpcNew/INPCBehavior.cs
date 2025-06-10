using UnityEngine;
using System.Collections.Generic;

namespace NpcNew
{
    /// <summary>
    /// NPC行为接口 - 定义NPC的基本行为
    /// </summary>
    public interface INPCBehavior
    {
        string NPCID { get; }
        bool CanInteract { get; set; }
        bool IsFollowing { get; set; }
        bool IsActive { get; set; }

        void Initialize(NPCDataNew data);
        void ActivateNPC();
        void DeactivateNPC();
        void StartInteraction();
        void StartFollowing();
        void StopFollowing();
    }

    /// <summary>
    /// NPC交互接口
    /// </summary>
    public interface INPCInteractable
    {
        /// <summary>
        /// 检查玩家是否在交互范围内
        /// </summary>
        /// <param name="position">玩家位置</param>
        /// <returns>如果在交互范围内返回true，否则返回false</returns>
        bool IsInInteractionRange(Vector3 position);
        /// <summary>
        /// 玩家进入交互范围时调用
        /// </summary>
        void OnPlayerEnterRange();
        /// <summary>
        /// 玩家离开交互范围时调用
        /// </summary>
        void OnPlayerExitRange();
        /// <summary>
        /// 触发交互事件
        /// </summary>
        void OnInteractionTrigger();
    }

    /// <summary>
    /// NPC状态接口
    /// </summary>
    public interface INPCStateController
    {
        void ChangeState(NPCStateType stateType);
        NPCStateType CurrentStateType { get; }
    }

    /// <summary>
    /// NPC对话接口
    /// </summary>
    public interface INPCDialogue
    {
        void StartDialogue(string dialogueID = null);
        void OnDialogueComplete(string dialogueID);
        bool HasAvailableDialogue();
    }
}