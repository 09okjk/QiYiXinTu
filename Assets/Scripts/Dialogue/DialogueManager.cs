using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Core;
using Manager;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance { get; private set; }
    
    [Header("UI References")]
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private Button continueButton;
    [SerializeField] private GameObject choiceButtonPrefab;
    [SerializeField] private Transform choiceButtonContainer;
    [SerializeField] private GameObject playerDialoguePanel; // 新增：Player对话面板
    [SerializeField] private Image playerImage; // 新增：Player头像
    [SerializeField] private TextMeshProUGUI playerNameText; // 新增：Player姓名
    [SerializeField] private TextMeshProUGUI playerDialogueText;
    [SerializeField] private GameObject nPCDialoguePanel; // 新增：NPC对话面板
    [SerializeField] private Image nPCImage; // 新增：NPC头像
    [SerializeField] private TextMeshProUGUI nPCNameText; // 新增：NPC姓名
    [SerializeField] private TextMeshProUGUI nPCDialogueText;
    [SerializeField] private GameObject systemDialoguePanel; // 新增：系统对话面板
    [SerializeField] private TextMeshProUGUI systemDialogueText; // 新增：系统对话文本
    [SerializeField] private float typewriterSpeed = 0.05f;
    
    [Header("Audio")]
    [SerializeField] private AudioSource dialogueAudioSource; // 可选：对话音效
    
    // 当前对话数据和节点索引
    private DialogueData currentDialogue;
    // 当前对话节点
    private DialogueNode currentDialogueNode;
    // 当前对话节点索引
    private string currentNodeID;
    // 是否正在打字
    private bool isTyping = false;
    // 打字协程
    private Coroutine typingCoroutine;
    // 存储对话完成后的回调
    private Action<bool> onDialogueCompleteCallback;
    // 当前对话文本
    private TextMeshProUGUI currentDialogueText;
    // Npc对象
    private NPC currentNpc;
    
    // 对话结束事件
    public event Action<string> OnDialogueEnd;
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    // 开始对话，可选择性地添加完成回调
    public async Task StartDialogue(DialogueData dialogue, Action<bool> onComplete = null)
    {
        if (dialogue == null || dialogue.nodes == null || dialogue.nodes.Count == 0)
        {
            Debug.LogError("尝试开始无效的对话数据");
            return;
        }
        
        if (dialogue.state == DialogueState.Finished)
        {
            Debug.LogError("对话已结束，无法重新开始");
            return;
        }
        
        currentDialogue = dialogue;
        if (currentDialogue.state == DialogueState.WithOutStart)
        {
            currentNodeID = currentDialogue.nodes[0].nodeID;
            currentDialogue.state = DialogueState.Ongoing;
        }
        else
        {
            currentNodeID = currentDialogue.currentNodeID;
        }
        
        dialoguePanel.SetActive(true);
        await DisplayCurrentNode();
        onDialogueCompleteCallback = onComplete;
    }
    
    // 一些动画触发的对话和特定场景触发的对话，需要通过ID来开始
    public void StartDialogueByID(string dialogueID, Action<bool> onComplete = null)
    {
        DialogueData dialogue = GetDialogueData(dialogueID);
        if (dialogue)
        {
            _ = StartDialogue(dialogue, onComplete);
        }
        else
        {
            Debug.LogError($"无法找到对话数据: {dialogueID}");
        }
    }
    
    // 显示当前对话节点
    private async Task DisplayCurrentNode()
    {
        // 检查节点索引是否有效
        if (string.IsNullOrEmpty(currentNodeID) )
        {
            currentDialogue.state = DialogueState.Finished;
            EndDialogue();
            return;
        }
        
        // 获取当前节点
        currentDialogueNode = currentDialogue.nodes.Find(n => n.nodeID == currentNodeID);
        // 记录当前节点ID，保存到对话数据中
        currentDialogue.currentNodeID = currentNodeID;
        
        // 检查对话条件
        if (!CheckCondition()) return;
        
        // 获取说话者类型
        SpeakerType speakerType = currentDialogueNode.speaker.speakerType;
        
        // 设置角色显示
        ChangeSpeaker(speakerType);
    }

    private void ChangeSpeaker(SpeakerType speakerType)
    {
        Debug.Log($"show speaker: {speakerType}");
        playerDialoguePanel.SetActive(false);
        nPCDialoguePanel.SetActive(false);
        systemDialoguePanel.SetActive(false);
        
        switch (speakerType)
        {
            case SpeakerType.PlayerChoice:
            case SpeakerType.Player:
                try
                {
                    playerNameText.text = currentDialogueNode.speaker.speakerName != "???" ? PlayerManager.Instance.player.playerData.playerName : currentDialogueNode.speaker.speakerName;
                    CurrentDialogueTextCheck(playerDialogueText);
                    playerImage.sprite =
                        Resources.Load<Sprite>(
                            $"Art/Player/{currentDialogueNode.speaker.speakerID}_{currentDialogueNode.speaker.emotion.ToString()}");
                    playerDialoguePanel.SetActive(true);
                }
                catch (Exception e)
                {
                    Debug.LogError($"设置玩家对话面板时出错: {e.Message}");
                    throw;
                }
                break;
            case SpeakerType.NpcNotice:
            case SpeakerType.Npc:
                currentNpc = NPCManager.Instance.GetNpc(currentDialogueNode.speaker.speakerID);
                nPCNameText.text = string.IsNullOrEmpty(currentDialogueNode.speaker.speakerName) ? currentDialogueNode.speaker.speakerID : currentDialogueNode.speaker.speakerName;
                CurrentDialogueTextCheck(nPCDialogueText);
                nPCImage.sprite = Resources.Load<Sprite>($"Art/NPCs/{currentDialogueNode.speaker.speakerID}_{currentDialogueNode.speaker.emotion.ToString()}");
                nPCDialoguePanel.SetActive(true);
                break;
            case SpeakerType.System:
                CurrentDialogueTextCheck(systemDialogueText);
                systemDialoguePanel.SetActive(true);
                break;
        }
        
        if (speakerType == SpeakerType.PlayerChoice || speakerType == SpeakerType.NpcNotice)
            return;
        
        // 清除任何现有的选择按钮
        foreach (Transform child in choiceButtonContainer)
        {
            Destroy(child.gameObject);
        }
        
        // 如果正在打字，停止当前协程
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
        }
        typingCoroutine = StartCoroutine(TypeText(currentDialogueText,currentDialogueNode.text, () => {
            // 文本打字完成后，等待玩家点击继续
            
            // 发布任务
            if (!string.IsNullOrEmpty(currentDialogueNode.questID))
            {
                Debug.Log($"发布任务: {currentDialogueNode.questID}");
                QuestManager.Instance.StartQuest(currentDialogueNode.questID);
            }
            //提供奖励
            if (currentDialogueNode.rewardIDs.Count > 0)
            {
                foreach (string rewardID in currentDialogueNode.rewardIDs)
                {
                    ItemData itemData = ItemManager.Instance.GetItem(rewardID);
                    if (itemData != null)
                    {
                        InventoryManager.Instance.AddItem(itemData);
                    }
                    else
                    {
                        Debug.LogWarning($"无法找到奖励物品: {rewardID}");
                    }
                }
            }
            //TODO: 提供跟随
            if (currentNpc && currentDialogueNode.isFollow)
            {
                currentNpc.FollowPlayer();
            }
            
            continueButton.onClick.AddListener(OnDialoguePanelClicked);
        }));
    }
    
    // 打字机效果，完成后调用回调
    private IEnumerator TypeText(TextMeshProUGUI dialogueText,string text, Action onComplete = null)
    {
        isTyping = true;
        dialogueText.text = "";
        
        // 逐字符显示文本
        foreach (char c in text)
        {
            dialogueText.text += c;
            
            // 可选：在每个字符播放声音
            if (dialogueAudioSource != null && c != ' ' && c != '\n')
            {
                dialogueAudioSource.pitch = UnityEngine.Random.Range(0.9f, 1.1f); // 轻微变化音高
                dialogueAudioSource.Play();
            }
            
            yield return new WaitForSeconds(typewriterSpeed);
        }
        
        isTyping = false;
        onComplete?.Invoke();
    }
    
    // 显示选择按钮
    private void DisplayChoices()
    {
        // 清除任何现有的选择按钮
        foreach (Transform child in choiceButtonContainer)
        {
            Destroy(child.gameObject);
        }
        // 创建选择按钮，每个按钮对应一个选择
        for (int i = 0; i < currentDialogueNode.choices.Count; i++)
        {
            DialogueChoice choice = currentDialogueNode.choices[i];
            GameObject buttonGO = Instantiate(choiceButtonPrefab, choiceButtonContainer);
            
            // 设置按钮文本和点击事件
            Button button = buttonGO.GetComponent<Button>();
            TextMeshProUGUI buttonText = buttonGO.GetComponentInChildren<TextMeshProUGUI>();
            
            buttonText.text = choice.text;
            
            int choiceIndex = i; // 需要捕获索引以供lambda使用
            button.onClick.AddListener(() => OnChoiceSelected(choiceIndex));
        }
    }
    
    // 选择按钮点击事件，移动到下一个节点，并显示文本或选择
    private void OnChoiceSelected(int choiceIndex)
    {
        // DialogueNode currentNode = currentDialogue.nodes.Find(n => n.nodeID == currentNodeID);
        
        // 检查是否有有效的选项索引
        if (choiceIndex < 0 || choiceIndex >= currentDialogueNode.choices.Count)
        {
            Debug.LogError($"选择索引无效: {choiceIndex}");
            return;
        }
        
        // // 获取按钮文本，并显示打字效果在对话框上
        // DialogueChoice selectedChoice = currentDialogueNode.choices[choiceIndex];
        // ChangeSpeaker(SpeakerType.PlayerChoice);
        // typingCoroutine = StartCoroutine(TypeText(currentDialogueText,selectedChoice.text, () =>
        // {
        // }));
        string nextNodeID = currentDialogueNode.choices[choiceIndex].nextNodeID;
    
        // 清除选择
        foreach (Transform child in choiceButtonContainer)
        {
            Destroy(child.gameObject);
        }
    
        // 移动到下一个对话节点
        currentNodeID = nextNodeID;
        // 显示下一个节点
        _ = DisplayCurrentNode();
    }
    
    // 点击对话面板事件
    public void OnDialoguePanelClicked()
    {
        // 跳过打字动画并显示完整文本
        StopCoroutine(typingCoroutine);
        isTyping = false;
        currentDialogueText.text = currentDialogueNode.text;
        if (currentDialogueNode.choices.Count > 0)
        {
            // 如果有选择，显示选择按钮
            DisplayChoices();
            return;
        }
        
        currentNodeID = currentDialogueNode.nextNodeID;
        DisplayCurrentNode();
    }
    
    // 检查对话条件
    private bool CheckCondition()
    {
        Debug.Log($"当前对话ID: {currentDialogue.dialogueID}");
        Debug.Log($"当前对话节点ID: {currentDialogueNode.nodeID}");
        Debug.Log($"当前对话条件类型: {currentDialogueNode.conditionType}");
        switch (currentDialogueNode.conditionType)
        {
            case DialogueConditionType.None:
                break;
            case DialogueConditionType.QuestCompleted:
                //调用任务管理器检查任务是否完成
                if(!QuestManager.Instance.IsQuestCompleted(currentDialogueNode.conditionValue))
                    return false;
                break;
            case DialogueConditionType.ItemAcquired:
                //调用物品管理器检查物品是否获得
                if (!string.IsNullOrEmpty(currentDialogueNode.conditionValue))
                {
                    // 拆分条件值
                    string[] conditionValues = currentDialogueNode.conditionValue.Split(';');

                    // 所有缺失物品的名称
                    string missingItemNames = "";
                    foreach (var value in conditionValues)
                    {
                        ItemData itemData = ItemManager.Instance.GetItem(currentDialogueNode.conditionValue);
                        if (!itemData) continue;
                        missingItemNames += itemData.itemName;
                    }

                    if (missingItemNames != "")
                    {
                        Debug.LogWarning($"缺少物品: {missingItemNames}");
                        ChangeSpeaker(SpeakerType.NpcNotice);
                        typingCoroutine = StartCoroutine(TypeText(currentDialogueText, $"缺少物品: {missingItemNames}", EndDialogue));
                        return false;
                    }
                }
                break;
            case DialogueConditionType.SceneName:
                //调用场景管理器检查场景名称
                if (!string.IsNullOrEmpty(currentDialogueNode.conditionValue))
                {
                    if (currentDialogueNode.conditionValue != SceneManager.GetActiveScene().name)
                    {
                        Debug.LogWarning($"当前场景名称不匹配: {currentDialogueNode.conditionValue}");
                        EndDialogue();
                        return false;
                    }
                }
                break;
            case DialogueConditionType.NpcCheck:
                // TODO: 调用NPC管理器检查条件Npc是否存在
                break;
            case DialogueConditionType.DialogueCompleted:
                if (!IsDialogueFinished(currentDialogueNode.conditionValue))
                {
                    Debug.LogWarning($"未完成对话: {currentDialogueNode.conditionValue}");
                    EndDialogue();
                    return false;
                }
                break;
        }
        return true;
    }
    
    // 结束对话
    private void EndDialogue()
    {
        dialoguePanel.SetActive(false);
        // 对话结束时的回调或事件
        OnDialogueEnd?.Invoke(currentDialogue.dialogueID);// 全局事件
        
        // 执行对话完成后的回调 一次性回调，只针对特定对话，在对话结束后自动清除
        if (onDialogueCompleteCallback != null)
        {
            Action<bool> tempCallback = onDialogueCompleteCallback;
            onDialogueCompleteCallback = null; // 清空回调防止多次触发
            tempCallback(currentDialogue.state == DialogueState.Finished);
        }
        Debug.Log($"对话结束: {currentDialogue.dialogueID}");
    }
    
    // 检查对话是否正在进行
    public bool IsDialogueActive()
    {
        return dialoguePanel.activeSelf;
    }
    

    private void CurrentDialogueTextCheck(TextMeshProUGUI dialogueText)
    {
        try
        {
            currentDialogueText = dialogueText;
            string playerName = PlayerManager.Instance.player.playerData.playerName;
            currentDialogueText.text = currentDialogueText.text.Replace("{playerName}", playerName);
        }
        catch (Exception e)
        {
            Debug.LogError($"设置对话文本时出错: {e.Message}");
            throw;
        }
    }
    
    // 获取对话数据
    public DialogueData GetDialogueData(string dialogueID)
    {
        if (string.IsNullOrEmpty(dialogueID))
        {
            Debug.LogWarning("传入的dialogueID为空");
            return null;
        }
        
        // 从指定路径加载对话数据
        DialogueData dialogue = Resources.Load<DialogueData>($"ScriptableObjects/Dialogues/{dialogueID}");
        
        if (dialogue == null)
        {
            Debug.LogWarning($"无法找到对话数据: {dialogueID}");
            return null;
        }
        
        return dialogue;
    }
    
    public bool IsDialogueFinished(string dialogueID)
    {
        return GetDialogueData(dialogueID)?.state == DialogueState.Finished;
    }
}