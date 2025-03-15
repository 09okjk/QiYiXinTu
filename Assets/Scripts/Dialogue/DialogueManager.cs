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
    [SerializeField] private TextMeshProUGUI dialogueText;
    [SerializeField] private GameObject choiceButtonPrefab;
    [SerializeField] private Transform choiceButtonContainer;
    [SerializeField] private float typewriterSpeed = 0.05f;
    
    // 当前对话数据和节点索引
    private DialogueData currentDialogue;
    // 当前对话节点索引
    private int currentNodeIndex;
    // 是否正在打字
    private bool isTyping = false;
    // 打字协程
    private Coroutine typingCoroutine;
    
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
    
    // 开始对话
    public void StartDialogue(DialogueData dialogue)
    {
        currentDialogue = dialogue;
        currentNodeIndex = 0;
        dialoguePanel.SetActive(true);
        DisplayCurrentNode();
    }
    
    // 显示当前对话节点
    private void DisplayCurrentNode()
    {
        // 检查节点索引是否超出范围
        if (currentNodeIndex >= currentDialogue.nodes.Count) 
        {
            EndDialogue();
            return;
        }
        
        // 获取当前节点
        DialogueNode node = currentDialogue.nodes[currentNodeIndex];
        
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
        // 开始打字
        typingCoroutine = StartCoroutine(TypeText(node.text));
    }
    
    // 打字机效果
    private IEnumerator TypeText(string text)
    {
        isTyping = true;
        dialogueText.text = "";
        
        // 逐字符显示文本
        foreach (char c in text)
        {
            dialogueText.text += c;
            yield return new WaitForSeconds(typewriterSpeed);
        }
        
        isTyping = false;
        
        // 在文本完成后，如果有选择，显示选择
        DisplayChoices();
    }
    
    // 显示选择按钮
    private void DisplayChoices()
    {
        // 获取当前节点
        DialogueNode currentNode = currentDialogue.nodes[currentNodeIndex];
        
        if (currentNode.choices.Count == 0)
        {
            // 如果没有选择，点击继续
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
        int nextNodeIndex = currentNode.choices[choiceIndex].nextNodeIndex;
        
        // 清除选择
        foreach (Transform child in choiceButtonContainer)
        {
            Destroy(child.gameObject);
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
            dialogueText.text = currentDialogue.nodes[currentNodeIndex].text;
            isTyping = false;
            DisplayChoices();
        }
        else if (currentDialogue.nodes[currentNodeIndex].choices.Count == 0)
        {
            // 如果没有选择，前进到下一个节点
            currentNodeIndex++;
            DisplayCurrentNode();
        }
        // 另外，什么都不做（等待玩家选择）
    }
    
    // 结束对话
    private void EndDialogue()
    {
        dialoguePanel.SetActive(false);
        // 对话结束时的回调或事件
        OnDialogueEnd?.Invoke();
    }
    
    // 对话结束事件
    public event Action OnDialogueEnd;
}