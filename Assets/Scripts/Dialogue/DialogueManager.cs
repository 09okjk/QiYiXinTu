using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance { get; private set; }
    
    [Header("UI References")]
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private TextMeshProUGUI playerNameText; // 新增：Player姓名
    [SerializeField] private TextMeshProUGUI playerDialogueText;
    [SerializeField] private TextMeshProUGUI nPCNameText; // 新增：Player姓名
    [SerializeField] private TextMeshProUGUI nPCDialogueText;
    [SerializeField] private GameObject choiceButtonPrefab;
    [SerializeField] private Transform choiceButtonContainer;
    [SerializeField] private GameObject role1;
    [SerializeField] private GameObject role2;
    [SerializeField] private float typewriterSpeed = 0.05f;
    
    [Header("Audio")]
    [SerializeField] private AudioSource dialogueAudioSource; // 可选：对话音效
    
    // 当前对话数据和节点索引
    private DialogueData currentDialogue;
    // 当前对话节点索引
    private int currentNodeIndex;
    // 是否正在打字
    private bool isTyping = false;
    // 打字协程
    private Coroutine typingCoroutine;
    // 存储对话完成后的回调
    private Action onDialogueCompleteCallback;
    // 当前对话文本
    private TextMeshProUGUI currentDialogueText;
    
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
    public void StartDialogue(DialogueData dialogue, Action onComplete = null)
    {
        if (dialogue == null || dialogue.nodes == null || dialogue.nodes.Count == 0)
        {
            Debug.LogError("尝试开始无效的对话数据");
            return;
        }
        
        currentDialogue = dialogue;
        currentNodeIndex = 0;
        onDialogueCompleteCallback = onComplete;
        dialoguePanel.SetActive(true);
        DisplayCurrentNode();
    }
    
    // 显示当前对话节点
    private void DisplayCurrentNode()
    {
        // 检查节点索引是否有效
        if (currentNodeIndex < 0 || currentNodeIndex >= currentDialogue.nodes.Count) 
        {
            EndDialogue();
            return;
        }
        
        // 获取当前节点
        DialogueNode node = currentDialogue.nodes[currentNodeIndex];
        
        // 设置角色显示
        if (node.speakerPosition == "left")
        {
            playerNameText.text = string.IsNullOrEmpty(node.speakerName) ? node.speakerID : node.speakerName;
            // playerDialogueText.text = node.text;
            role1.SetActive(true);
            playerNameText.gameObject.SetActive(true);
            playerDialogueText.gameObject.SetActive(true);
            role2.SetActive(false);
            nPCNameText.gameObject.SetActive(false);
            nPCDialogueText.gameObject.SetActive(false);
            
            currentDialogueText = playerDialogueText;
        }
        else
        {
            nPCNameText.text = string.IsNullOrEmpty(node.speakerName) ? node.speakerID : node.speakerName;
            // nPCDialogueText.text = node.text;
            role1.SetActive(false);
            playerNameText.gameObject.SetActive(false);
            playerDialogueText.gameObject.SetActive(false);
            role2.SetActive(true);
            nPCNameText.gameObject.SetActive(true);
            nPCDialogueText.gameObject.SetActive(true);
            
            currentDialogueText = nPCDialogueText;
        }
        
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
        
        // 处理对话分支逻辑 - 根据 nextNodeIndex 检查是否应自动前进
        if (node.choices.Count == 0 && node.nextNodeIndex >= 0)
        {
            // 开始打字
            typingCoroutine = StartCoroutine(TypeText(currentDialogueText,node.text, () => {
                // 文本打字完成后，等待玩家点击继续
            }));
        }
        else
        {
            // 开始打字
            typingCoroutine = StartCoroutine(TypeText(currentDialogueText,node.text, DisplayChoices));
        }
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
        // 获取当前节点
        DialogueNode currentNode = currentDialogue.nodes[currentNodeIndex];
        
        if (currentNode.choices.Count == 0)
        {
            // 如果没有选择，但有下一个节点，创建一个"继续"按钮
            if (currentNode.nextNodeIndex >= 0)
            {
                GameObject buttonGO = Instantiate(choiceButtonPrefab, choiceButtonContainer);
                Button button = buttonGO.GetComponent<Button>();
                TextMeshProUGUI buttonText = buttonGO.GetComponentInChildren<TextMeshProUGUI>();
                
                buttonText.text = "继续";
                button.onClick.AddListener(() => {
                    currentNodeIndex = currentNode.nextNodeIndex;
                    DisplayCurrentNode();
                });
            }
            return;
        }
        
        // 创建选择按钮，每个按钮对应一个选择
        for (int i = 0; i < currentNode.choices.Count; i++)
        {
            DialogueChoice choice = currentNode.choices[i];
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
        DialogueNode currentNode = currentDialogue.nodes[currentNodeIndex];
        
        // 检查是否有有效的选项索引
        if (choiceIndex < 0 || choiceIndex >= currentNode.choices.Count)
        {
            Debug.LogError($"选择索引无效: {choiceIndex}");
            return;
        }
        
        int nextNodeIndex = currentNode.choices[choiceIndex].nextNodeIndex;
        
        // 清除选择
        foreach (Transform child in choiceButtonContainer)
        {
            Destroy(child.gameObject);
        }
        
        // 检查nextNodeIndex是否有效
        if (nextNodeIndex < 0 || nextNodeIndex >= currentDialogue.nodes.Count)
        {
            // 选择结束对话
            EndDialogue();
            return;
        }
        
        // 移动到下一个对话节点
        currentNodeIndex = nextNodeIndex;
        // 显示下一个节点
        DisplayCurrentNode();
    }
    
    // 点击对话面板事件
    public void OnDialoguePanelClicked()
    {
        if (isTyping)
        {
            // 跳过打字动画并显示完整文本
            StopCoroutine(typingCoroutine);
            currentDialogueText.text = currentDialogue.nodes[currentNodeIndex].text;
            isTyping = false;
            DisplayChoices();
        }
        else if (currentDialogue.nodes[currentNodeIndex].choices.Count == 0)
        {
            // 获取当前节点
            DialogueNode currentNode = currentDialogue.nodes[currentNodeIndex];
            
            if (currentNode.nextNodeIndex >= 0)
            {
                // 如果有下一个节点，前进到该节点
                currentNodeIndex = currentNode.nextNodeIndex;
                DisplayCurrentNode();
            }
            else
            {
                // 如果没有下一个节点，结束对话
                EndDialogue();
            }
        }
        // 另外，什么都不做（等待玩家选择）
    }
    
    // 获取当前说话者ID
    public string GetCurrentSpeakerID()
    {
        if (currentDialogue != null && currentNodeIndex >= 0 && currentNodeIndex < currentDialogue.nodes.Count)
        {
            return currentDialogue.nodes[currentNodeIndex].speakerID;
        }
        return string.Empty;
    }
    
    // 结束对话
    private void EndDialogue()
    {
        dialoguePanel.SetActive(false);
        // 对话结束时的回调或事件
        OnDialogueEnd?.Invoke();
        
        // 执行对话完成后的回调
        if (onDialogueCompleteCallback != null)
        {
            Action tempCallback = onDialogueCompleteCallback;
            onDialogueCompleteCallback = null; // 清空回调防止多次触发
            tempCallback();
        }
    }
    
    // 检查对话是否正在进行
    public bool IsDialogueActive()
    {
        return dialoguePanel.activeSelf;
    }
    
    // 对话结束事件
    public event Action OnDialogueEnd;
}