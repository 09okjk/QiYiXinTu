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
    
    private IEnumerator FadeOutNotification(GameObject notification)
    {
        CanvasGroup canvasGroup = notification.GetComponent<CanvasGroup>();
        
        // Show for the main duration
        yield return new WaitForSeconds(notificationDuration);
        
        // Fade out
        float startTime = Time.time;
        while (Time.time < startTime + notificationFadeTime)
        {
            float normalizedTime = (Time.time - startTime) / notificationFadeTime;
            canvasGroup.alpha = 1 - normalizedTime;
            yield return null;
        }
        
        // Destroy when done
        Destroy(notification);
    }
    
    public void ShowConfirmDialog(string title, string message, Action onYes, Action onNo)
    {
        if (confirmDialogPanel == null)
        {
            Debug.LogError("Confirm dialog panel not assigned!");
            return;
        }
        
        confirmTitleText.text = title;
        confirmMessageText.text = message;
        
        // Clear previous listeners
        confirmYesButton.onClick.RemoveAllListeners();
        confirmNoButton.onClick.RemoveAllListeners();
        
        // Add new listeners
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