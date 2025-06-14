using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Manager;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }
    
    [Header("Notifications")]
    [SerializeField] private GameObject notificationPrefab;
    [SerializeField] private Transform notificationContainer;
    [SerializeField] private float notificationDuration = 3f;
    [SerializeField] private float notificationFadeTime = 0.5f;
    
    [Header("Confirm Dialog")]
    [SerializeField] private GameObject confirmDialogPanel;
    [SerializeField] private TextMeshProUGUI confirmTitleText;
    [SerializeField] private Image confirmImage;
    [SerializeField] private TextMeshProUGUI confirmMessageText;
    [SerializeField] private Button confirmYesButton;
    [SerializeField] private Button confirmNoButton;
    
    [Header("InputField Window")]
    [SerializeField] private GameObject inputFieldWindow;
    [SerializeField] private TextMeshProUGUI inputFieldTitleText;
    [SerializeField] private TextMeshProUGUI inputFieldMessageText;
    [SerializeField] private TMP_InputField inputField;
    [SerializeField] private Button inputFieldConfirmButton;
    
    public event Action<bool> OnPopWindowEvent;
    
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
    
    private void Start()
    {
        if (confirmDialogPanel)
        {
            confirmDialogPanel.SetActive(false);
        }
    }

    private void OnEnable()
    {
        DialogueManager.Instance.OnDialogueEnd += CheckDialogueID;
    }

    private void OnDisable()
    {
        DialogueManager.Instance.OnDialogueEnd -= CheckDialogueID;
    }

    private async void CheckDialogueID(string dialogueID)
    {
        try
        {
            // 检查对话ID并显示相应的输入框
            if (dialogueID == "fight_over_dialogue")
            {
                // 在调用前验证PlayerManager状态
                if (PlayerManager.Instance == null)
                {
                    Debug.LogError("PlayerManager.Instance 为空");
                    return;
                }
            
                if (PlayerManager.Instance.player == null)
                {
                    Debug.LogError("PlayerManager.Instance.player 为空");
                    return;
                }
            
                await InputFieldWindow(
                    "你的名字是？", 
                    "请输入你的名字：", 
                    (name) => {
                        if (!string.IsNullOrEmpty(name))
                        {
                            PlayerManager.Instance.ChangePlayerName(name);
                        }
                        else
                        {
                            Debug.LogWarning("输入的名字为空");
                        }
                    }
                );
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Error in CheckDialogueID: {e.Message}\nStackTrace: {e.StackTrace}");
        }
    }

    /// <summary>
    /// 显示通知
    /// </summary>
    /// <param name="message">通知内容</param>
    public void ShowNotification(string message)
    {
        GameObject notificationGO = Instantiate(notificationPrefab, notificationContainer);
        TextMeshProUGUI notificationText = notificationGO.GetComponentInChildren<TextMeshProUGUI>();
        
        if (notificationText != null)
        {
            notificationText.text = message;
        }
        
        StartCoroutine(FadeOutNotification(notificationGO));
    }
    
    /// <summary>
    /// 逐渐消失通知
    /// </summary>
    /// <param name="notification">通知对象</param>
    /// <returns></returns>
    private IEnumerator FadeOutNotification(GameObject notification)
    {
        CanvasGroup canvasGroup = notification.GetComponent<CanvasGroup>();
        
        // 显示主要持续时间
        yield return new WaitForSeconds(notificationDuration);
        
        // 淡出
        float startTime = Time.time;
        while (Time.time < startTime + notificationFadeTime)
        {
            float normalizedTime = (Time.time - startTime) / notificationFadeTime;
            canvasGroup.alpha = 1 - normalizedTime;
            yield return null;
        }
        
        // 完成后销毁
        Destroy(notification);
    }

    /// <summary>
    /// 显示确认对话框
    /// </summary>
    /// <param name="title">对话框标题</param>
    /// <param name="message">对话框消息</param>
    /// <param name="sprite">图片</param>
    /// <param name="onYes">点击确认时执行的操作</param>
    /// <param name="onNo">点击取消时执行的操作</param>
    public void ShowConfirmDialog(string title, string message,Sprite sprite = null, Action onYes = null, Action onNo = null)
    {
        if (!confirmDialogPanel)
        {
            Debug.LogError("Confirm dialog panel not assigned!");
            return;
        }
        
        if (SceneManager.GetActiveScene().name != "MainMenu")
            PlayerManager.Instance.player.RegisterPopWindowEvent(); 
        
        // 隐藏按钮
        confirmYesButton.gameObject.SetActive(false);
        confirmNoButton.gameObject.SetActive(false);
        confirmImage.gameObject.SetActive(false);
        
        confirmTitleText.text = title;
        confirmMessageText.text = message;
        
        // 清除之前的监听器 防止重复调用
        confirmYesButton.onClick.RemoveAllListeners();
        confirmNoButton.onClick.RemoveAllListeners();

        // 设置图片
        if (sprite != null)
        {
            confirmImage.gameObject.SetActive(true);
            confirmImage.sprite = sprite;
        }

        if (onYes != null)
        {
            confirmYesButton.gameObject.SetActive(true);
        }
        
        if (onNo != null)
        {
            confirmNoButton.gameObject.SetActive(true);
        }
        
        // 添加新的监听器
        confirmYesButton.onClick.AddListener(() => 
        {
            onYes?.Invoke();
            confirmDialogPanel.SetActive(false);
            OnPopWindowEvent?.Invoke(confirmDialogPanel.activeSelf);
        });
        
        confirmNoButton.onClick.AddListener(() => 
        {
            onNo?.Invoke();
            confirmDialogPanel.SetActive(false);
            OnPopWindowEvent?.Invoke(confirmDialogPanel.activeSelf);
        });
        
        confirmDialogPanel.SetActive(true);
        OnPopWindowEvent?.Invoke(confirmDialogPanel.activeSelf);
    }

    /// <summary>
    /// 显示输入框窗口
    /// </summary>
    /// <param name="title">弹窗标题</param>
    /// <param name="message">输入提示</param>
    /// <param name="onConfirm">确认按钮</param>
    /// <returns></returns>
    public Task InputFieldWindow(string title, string message, Action<string> onConfirm)
    {
        if (!inputFieldWindow)
        {
            Debug.LogError("Input field window not assigned!");
            return Task.CompletedTask;
        }
        PlayerManager.Instance.player.RegisterPopWindowEvent();
        
        inputFieldTitleText.text = title;
        inputFieldMessageText.text = message;
        
        inputFieldWindow.SetActive(true);
        
        // 清除之前的监听器 防止重复调用
        inputFieldConfirmButton.onClick.RemoveAllListeners();
        
        // 添加新的监听器
        inputFieldConfirmButton.onClick.AddListener(() => 
        {
            string inputText = inputField.text;
            onConfirm?.Invoke(inputText);
            inputFieldWindow.SetActive(false);
            OnPopWindowEvent?.Invoke(inputFieldWindow.activeSelf);
        });
        
        OnPopWindowEvent?.Invoke(inputFieldWindow.activeSelf);
        return Task.CompletedTask;
    }
}