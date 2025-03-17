using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
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
    [SerializeField] private TextMeshProUGUI confirmMessageText;
    [SerializeField] private Button confirmYesButton;
    [SerializeField] private Button confirmNoButton;
    
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
        if (confirmDialogPanel != null)
        {
            confirmDialogPanel.SetActive(false);
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
    /// <param name="onYes">点击确认时执行的操作</param>
    /// <param name="onNo">点击取消时执行的操作</param>
    public void ShowConfirmDialog(string title, string message, Action onYes, Action onNo)
    {
        if (confirmDialogPanel == null)
        {
            Debug.LogError("Confirm dialog panel not assigned!");
            return;
        }
        
        confirmTitleText.text = title;
        confirmMessageText.text = message;
        
        // 清除之前的监听器 防止重复调用
        confirmYesButton.onClick.RemoveAllListeners();
        confirmNoButton.onClick.RemoveAllListeners();
        
        // 添加新的监听器
        confirmYesButton.onClick.AddListener(() => 
        {
            onYes?.Invoke();
            confirmDialogPanel.SetActive(false);
        });
        
        confirmNoButton.onClick.AddListener(() => 
        {
            onNo?.Invoke();
            confirmDialogPanel.SetActive(false);
        });
        
        confirmDialogPanel.SetActive(true);
    }
}