using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Core;
using Manager;
using Save;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using Utils;

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
    
    [Header("Game Pause Settings")]
    [SerializeField] private bool pauseGameDuringDialogue = true; // 是否在对话期间暂停游戏
    
    // 原始对话数据字典（只读）
    private DialogueData[] originalDialogueDataArray;
    // 运行时对话数据字典
    private Dictionary<string, DialogueData> dialogueDataDictionary = new Dictionary<string, DialogueData>();
    
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
    
    // 游戏暂停相关
    private float previousTimeScale;
    private bool wasGamePaused = false;
    
    // 对话结束事件
    public event Action<string> OnDialogueEnd;
    // 对话开始事件
    public event Action<string> OnDialogueStart;
    
    // 初始化状态标记
    private bool isInitialized = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log("DialogueManager 实例创建成功");
        }
        else
        {
            Debug.LogWarning("发现重复的DialogueManager实例，销毁当前对象");
            Destroy(gameObject);
            return;
        }
        
        // 初始化对话数据
        InitDialogueDictionary();
    }

    private void Start()
    {
    }
    
    private void OnDestroy()
    {
        if (Instance == this)
        {
            // 清理运行时副本
            ClearDialogueDictionary();
            Instance = null;
        }
    }

    /// <summary>
    /// 初始化对话数据字典
    /// </summary>
    private void InitDialogueDictionary()
    {   
        try
        {
            Debug.Log("开始加载对话数据...");
            
            // 防止重复初始化
            if (isInitialized)
            {
                Debug.LogWarning("对话数据已经初始化过，跳过重复初始化");
                return;
            }
            
            // 1. 首先加载原始数据
            originalDialogueDataArray = Resources.LoadAll<DialogueData>("ScriptableObjects/Dialogues");
            
            if (originalDialogueDataArray == null || originalDialogueDataArray.Length == 0)
            {
                Debug.LogError("未找到任何对话数据文件！请检查路径：Resources/ScriptableObjects/Dialogues");
                return;
            }
            
            Debug.Log($"找到 {originalDialogueDataArray.Length} 个原始对话数据文件");
            
            // 2. 清空现有字典（但保留引用）
            dialogueDataDictionary.Clear();
            
            // 3. 为每个原始对话数据创建运行时副本
            int successCount = 0;
            foreach (var originalDialogue in originalDialogueDataArray)
            {
                if (originalDialogue == null)
                {
                    Debug.LogWarning("发现空的对话数据引用，跳过");
                    continue;
                }
                
                if (string.IsNullOrEmpty(originalDialogue.dialogueID))
                {
                    Debug.LogWarning($"对话数据 {originalDialogue.name} 的dialogueID为空，跳过");
                    continue;
                }
                
                // 检查是否有重复的ID
                if (dialogueDataDictionary.ContainsKey(originalDialogue.dialogueID))
                {
                    Debug.LogWarning($"发现重复的对话ID: {originalDialogue.dialogueID}，跳过重复项");
                    continue;
                }
                
                try
                {
                    // 创建运行时副本
                    var runtimeCopy = Utils.ScriptableObjectUtils.CreateDialogueDataCopy(originalDialogue);
                    
                    if (runtimeCopy != null)
                    {
                        // 确保副本的ID与原始数据一致
                        runtimeCopy.dialogueID = originalDialogue.dialogueID;
                        
                        // 验证副本数据的完整性
                        if (ValidateDialogueData(runtimeCopy))
                        {
                            // 添加到字典
                            dialogueDataDictionary.Add(originalDialogue.dialogueID, runtimeCopy);
                            successCount++;
                            
                            Debug.Log($"✓ 成功创建对话运行时副本: {originalDialogue.dialogueID}");
                        }
                        else
                        {
                            Debug.LogError($"✗ 对话数据验证失败: {originalDialogue.dialogueID}");
                        }
                    }
                    else
                    {
                        Debug.LogError($"✗ 创建对话运行时副本失败（返回null）: {originalDialogue.dialogueID}");
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError($"✗ 创建对话 {originalDialogue.dialogueID} 的运行时副本时发生错误: {e.Message}\n{e.StackTrace}");
                }
            }
            
            isInitialized = true;
            Debug.Log($"对话数据初始化完成！原始文件: {originalDialogueDataArray.Length}, 成功创建副本: {successCount}, 字典大小: {dialogueDataDictionary.Count}");
            
            // 输出所有可用的对话ID用于调试
            DebugPrintAvailableDialogues();
        }
        catch (Exception e)
        {
            Debug.LogError($"初始化对话数据时发生严重错误: {e.Message}\n{e.StackTrace}");
        }
    }
    
    /// <summary>
    /// 验证对话数据的完整性
    /// </summary>
    /// <param name="dialogueData">要验证的对话数据</param>
    /// <returns>是否有效</returns>
    private bool ValidateDialogueData(DialogueData dialogueData)
    {
        if (dialogueData == null)
        {
            Debug.LogError("对话数据为null");
            return false;
        }

        if (string.IsNullOrEmpty(dialogueData.dialogueID))
        {
            Debug.LogError("对话ID为空");
            return false;
        }

        if (dialogueData.nodes == null || dialogueData.nodes.Count == 0)
        {
            Debug.LogError($"对话 {dialogueData.dialogueID} 没有节点数据");
            return false;
        }

        // 验证节点数据
        foreach (var node in dialogueData.nodes)
        {
            if (node == null)
            {
                Debug.LogError($"对话 {dialogueData.dialogueID} 包含空节点");
                return false;
            }

            if (string.IsNullOrEmpty(node.nodeID))
            {
                Debug.LogError($"对话 {dialogueData.dialogueID} 包含无ID的节点");
                return false;
            }
        }

        return true;
    }
    
    /// <summary>
    /// 输出所有可用对话ID用于调试
    /// </summary>
    private void DebugPrintAvailableDialogues()
    {
        if (dialogueDataDictionary.Count == 0)
        {
            Debug.LogWarning("没有可用的对话数据");
            return;
        }

        Debug.Log("=== 可用对话列表 ===");
        foreach (var kvp in dialogueDataDictionary)
        {
            Debug.Log($"对话ID: {kvp.Key}, 节点数: {kvp.Value.nodes?.Count ?? 0}");
        }
        Debug.Log("=== 对话列表结束 ===");
    }
    
    /// <summary>
    /// 清理对话字典（保留安全性）
    /// </summary>
    private void ClearDialogueDictionary()
    {
        if (dialogueDataDictionary != null)
        {
            // 清理运行时副本
            foreach (var runtimeDialogue in dialogueDataDictionary.Values)
            {
                if (runtimeDialogue != null)
                {
                    Utils.ScriptableObjectUtils.SafeDestroyRuntimeCopy(runtimeDialogue);
                }
            }
            
            dialogueDataDictionary.Clear();
        }
    }
    
    /// <summary>
    /// 重置所有对话数据到原始状态
    /// </summary>
    public void ResetAllDialogueData()
    {
        Debug.Log("开始重置所有对话数据...");
        
        // 清理现有的运行时副本
        ClearDialogueDictionary();
        
        // 重置初始化标记
        isInitialized = false;
        
        // 重新初始化
        InitDialogueDictionary();
        
        Debug.Log("对话数据重置完成");
    }
    
    #region 游戏暂停相关方法

    /// <summary>
    /// 暂停游戏
    /// </summary>
    private void PauseGame()
    {
        if (!pauseGameDuringDialogue) return;
        
        // 保存当前的时间缩放
        previousTimeScale = Time.timeScale;
        wasGamePaused = previousTimeScale == 0f;
        
        // 设置时间缩放为0来暂停游戏
        Time.timeScale = 0f;
        
        // 暂停音频监听器（可选）
        AudioListener.pause = true;
        
        Debug.Log("游戏已暂停 - 对话开始");
    }

    /// <summary>
    /// 恢复游戏
    /// </summary>
    private void ResumeGame()
    {
        if (!pauseGameDuringDialogue) return;
        
        // 只有当游戏之前没有被暂停时才恢复
        if (!wasGamePaused)
        {
            Time.timeScale = previousTimeScale;
        }
        
        // 恢复音频监听器（可选）
        AudioListener.pause = false;
        
        Debug.Log("游戏已恢复 - 对话结束");
    }

    /// <summary>
    /// 设置是否在对话期间暂停游戏
    /// </summary>
    /// <param name="shouldPause">是否暂停</param>
    public void SetPauseGameDuringDialogue(bool shouldPause)
    {
        pauseGameDuringDialogue = shouldPause;
    }

    /// <summary>
    /// 检查游戏是否因对话而暂停
    /// </summary>
    /// <returns>是否暂停</returns>
    public bool IsGamePausedByDialogue()
    {
        return pauseGameDuringDialogue && IsDialogueActive() && Time.timeScale == 0f;
    }

    #endregion
    
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
            Debug.LogWarning($"对话 {dialogue.dialogueID} 已结束，无法重新开始");
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
        
        // 暂停游戏
        PauseGame();
        
        // 触发对话开始事件
        OnDialogueStart?.Invoke(currentDialogue.dialogueID);
        
        dialoguePanel.SetActive(true);
        await DisplayCurrentNode();
        onDialogueCompleteCallback = onComplete;
    }

    // 一些动画触发的对话和特定场景触发的对话，需要通过ID来开始
    public void StartDialogueByID(string dialogueID, Action<bool> onComplete = null)
    {
        Debug.Log($"尝试启动对话: {dialogueID}");
        
        // 确保对话系统已初始化
        if (!isInitialized || dialogueDataDictionary == null || dialogueDataDictionary.Count == 0)
        {
            Debug.LogError("对话系统未正确初始化！尝试重新初始化...");
            InitDialogueDictionary();
            
            if (!isInitialized || dialogueDataDictionary.Count == 0)
            {
                Debug.LogError("对话系统初始化失败，无法启动对话");
                return;
            }
        }

        // 检查对话ID是否存在
        if (!dialogueDataDictionary.ContainsKey(dialogueID))
        {
            Debug.LogError($"无法找到对话数据: {dialogueID}");
            Debug.Log($"当前可用对话数量: {dialogueDataDictionary.Count}");
            
            // 输出可用的对话ID帮助调试
            if (dialogueDataDictionary.Count > 0)
            {
                Debug.Log("可用的对话ID:");
                foreach (var id in dialogueDataDictionary.Keys)
                {
                    Debug.Log($"  - {id}");
                }
            }
            return;
        }

        DialogueData dialogue = dialogueDataDictionary[dialogueID];
        if (dialogue != null)
        {
            Debug.Log($"✓ 找到对话数据: {dialogueID}, 开始启动对话");
            _ = StartDialogue(dialogue, onComplete);
        }
        else
        {
            Debug.LogError($"对话数据为null: {dialogueID}");
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
                    currentDialogueText = playerDialogueText;
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
                currentNpc = NPCManager.Instance.GetNPC(currentDialogueNode.speaker.speakerID);
                nPCNameText.text = string.IsNullOrEmpty(currentDialogueNode.speaker.speakerName) ? currentDialogueNode.speaker.speakerID : currentDialogueNode.speaker.speakerName;
                currentDialogueText = nPCDialogueText;
                nPCImage.sprite = Resources.Load<Sprite>($"Art/NPCs/{currentDialogueNode.speaker.speakerName}_{currentDialogueNode.speaker.emotion.ToString()}");
                nPCDialoguePanel.SetActive(true);
                break;
            case SpeakerType.System:
                currentDialogueText = systemDialogueText;
                systemDialoguePanel.SetActive(true);
                break;
        }
        
        CurrentDialogueTextCheck();
        
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
                    InventoryManager.Instance.AddItemById(rewardID);
                }
            }
            // 提供跟随
            // if (currentNpc && currentDialogueNode.isFollow)
            // {
            //     currentNpc.FollowTargetPlayer();
            // }
            // 在添加监听前先移除
            continueButton.onClick.RemoveAllListeners();
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
            
            // 使用 WaitForSecondsRealtime 而不是 WaitForSeconds
            // 这样即使 Time.timeScale = 0，打字效果仍然可以正常工作
            yield return new WaitForSecondsRealtime(typewriterSpeed);
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
        // 如果正在打字，停止打字动画并显示完整文本，但不继续执行后续逻辑
        if (isTyping)
        {
            // 停止打字协程
            if (typingCoroutine != null)
            {
                StopCoroutine(typingCoroutine);
                typingCoroutine = null;
            }
            
            isTyping = false;
            currentDialogueText.text = currentDialogueNode.text;
            
            // 执行打字完成后的逻辑（发布任务、提供奖励等）
            ExecuteNodeCompletionLogic();
            
            // 停止执行，等待用户再次点击
            return;
        }
        
        // 如果打字已完成，继续处理选择或下一个节点
        if (currentDialogueNode.choices.Count > 0)
        {
            // 如果有选择，显示选择按钮
            DisplayChoices();
            return;
        }
        
        currentNodeID = currentDialogueNode.nextNodeID;
        _ = DisplayCurrentNode();
    }
    
    // 执行节点完成后的逻辑（任务发布、奖励提供等）
    private void ExecuteNodeCompletionLogic()
    {
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
                InventoryManager.Instance.AddItemById(rewardID);
            }
        }
        
        // 提供跟随
        // if (currentNpc && currentDialogueNode.isFollow)
        // {
        //     currentNpc.FollowTargetPlayer();
        // }
    }
    
    // 检查对话条件
    private bool CheckCondition()
    {
        // Debug.Log($"当前对话ID: {currentDialogue.dialogueID}");
        // Debug.Log($"当前对话节点ID: {currentDialogueNode.nodeID}");
        // Debug.Log($"当前对话条件类型: {currentDialogueNode.conditionType}");
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
            case DialogueConditionType.EnemyCleared:
                if (Enum.TryParse(currentDialogueNode.conditionValue, out EnemyType enemyType) && EnemyManager.Instance.CheckActiveEnemyType(enemyType))
                {
                    Debug.LogWarning($"未清除敌人: {currentDialogueNode.conditionValue}");
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
        
        // 恢复游戏
        ResumeGame();
        
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

    private void CurrentDialogueTextCheck()
    {
        try
        {
            string playerName = PlayerManager.Instance.player.playerData.playerName;
            currentDialogueNode.text = currentDialogueNode.text.Replace("{playerName}", playerName);
            // Debug.Log($"对话文本已设置: {currentDialogueText.text}");
        }
        catch (Exception e)
        {
            Debug.LogError($"设置对话文本时出错: {e.Message}");
            throw;
        }
    }
    

    /// <summary>
    /// 获取对话数据（带详细日志）
    /// </summary>
    public DialogueData GetDialogueData(string dialogueID)
    {
        if (string.IsNullOrEmpty(dialogueID))
        {
            Debug.LogWarning("传入的dialogueID为空");
            return null;
        }

        // 确保系统已初始化
        if (!isInitialized)
        {
            Debug.LogWarning("对话系统未初始化，尝试初始化...");
            InitDialogueDictionary();
        }

        if (dialogueDataDictionary.ContainsKey(dialogueID))
        {
            var dialogue = dialogueDataDictionary[dialogueID];
            Debug.Log($"✓ 成功获取对话数据: {dialogueID}");
            return dialogue;
        }
        else
        {
            Debug.LogWarning($"✗ 无法找到对话数据: {dialogueID}");
            Debug.Log($"当前字典包含 {dialogueDataDictionary.Count} 个对话");
            return null;
        }
    }


    public Dictionary<string, DialogueData> GetDialogueDataDictionary()
    {
        return dialogueDataDictionary;
    }

    public void LoadDialogueData(List<AsyncSaveLoadSystem.DialogueSaveData> dialogueSaveDatas)
    {
        if (dialogueSaveDatas == null || dialogueSaveDatas.Count == 0)
        {
            Debug.LogWarning("没有对话保存数据可加载");
            return;
        }

        foreach (var dialogueSaveData in dialogueSaveDatas)
        {
            if (dialogueDataDictionary.ContainsKey(dialogueSaveData.dialogueID))
            {
                var dialogue = dialogueDataDictionary[dialogueSaveData.dialogueID];
                if (dialogue != null)
                {
                    // 更新对话状态
                    dialogue.state = dialogueSaveData.dialogueState;
                    // 更新当前节点ID
                    dialogue.currentNodeID = dialogueSaveData.currentNodeID;
                    Debug.Log($"✓ 加载对话数据: {dialogueSaveData.dialogueID}");
                }
                else
                {
                    Debug.LogWarning($"对话数据为null: {dialogueSaveData.dialogueID}");
                }
            }
            else
            {
                Debug.LogWarning($"无法找到对话数据: {dialogueSaveData.dialogueID}");
            }
        }
    }
    
    public bool IsDialogueFinished(string dialogueID)
    {
        var dialogue = GetDialogueData(dialogueID);
        return dialogue?.state == DialogueState.Finished;
    }

    #region 应急恢复方法

    /// <summary>
    /// 强制结束对话并恢复游戏（应急使用）
    /// </summary>
    [ContextMenu("强制结束对话")]
    public void ForceEndDialogue()
    {
        if (IsDialogueActive())
        {
            Debug.LogWarning("强制结束对话");
            EndDialogue();
        }
    }

    /// <summary>
    /// 强制恢复游戏时间（应急使用）
    /// </summary>
    [ContextMenu("强制恢复游戏时间")]
    public void ForceResumeGame()
    {
        Time.timeScale = 1f;
        AudioListener.pause = false;
        Debug.LogWarning("强制恢复游戏时间");
    }
    
    /// <summary>
    /// 重置所有对话数据（调试用）
    /// </summary>
    [ContextMenu("重置所有对话数据")]
    public void DebugResetAllDialogueData()
    {
        ResetAllDialogueData();
    }

    #endregion
    
    #region 调试方法

    [ContextMenu("重新初始化对话数据")]
    public void DebugReinitializeDialogues()
    {
        ResetAllDialogueData();
    }

    [ContextMenu("显示所有对话ID")]
    public void DebugShowAllDialogueIDs()
    {
        DebugPrintAvailableDialogues();
    }

    [ContextMenu("测试对话查找")]
    public void DebugTestDialogueLookup()
    {
        string testID = "dialogue_001"; // 替换为您要测试的对话ID
        Debug.Log($"测试查找对话: {testID}");
        var dialogue = GetDialogueData(testID);
        if (dialogue != null)
        {
            Debug.Log($"✓ 找到对话: {testID}");
        }
        else
        {
            Debug.Log($"✗ 未找到对话: {testID}");
        }
    }

    #endregion
}