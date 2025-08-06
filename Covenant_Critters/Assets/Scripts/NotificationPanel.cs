using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class NotificationPanel : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI messageText;
    [SerializeField] private Button closeButton;
    [SerializeField] private Image backgroundPanel;
    [SerializeField] private Animator animator; // Optional animator for effects
    
    [Header("Settings")]
    [SerializeField] private float autoHideTime = 5f;
    [SerializeField] private Color normalMessageColor = Color.white;
    [SerializeField] private Color importantMessageColor = Color.yellow;
    
    private Coroutine autoHideCoroutine;
    
    private void Awake()
    {
        // Make sure panel is hidden at start
        gameObject.SetActive(false);
        
        // Set up close button
        if (closeButton != null)
        {
            closeButton.onClick.AddListener(Hide);
        }
        
        // Try to get animator if not set
        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }
    }
    
    public void ShowMessage(string message, bool isImportant = false, float customHideTime = -1)
    {
        // Set message text
        if (messageText != null)
        {
            messageText.text = message;
            messageText.color = isImportant ? importantMessageColor : normalMessageColor;
        }
        
        // Show the panel
        gameObject.SetActive(true);
        
        // Play animation if available
        if (animator != null)
        {
            animator.SetTrigger("Show");
        }
        
        // Cancel previous auto-hide if running
        if (autoHideCoroutine != null)
        {
            StopCoroutine(autoHideCoroutine);
        }
        
        // Start auto-hide timer if specified
        float hideTime = customHideTime > 0 ? customHideTime : autoHideTime;
        if (hideTime > 0)
        {
            autoHideCoroutine = StartCoroutine(AutoHideRoutine(hideTime));
        }
    }
    
    public void Hide()
    {
        // Play hide animation if available
        if (animator != null && animator.isActiveAndEnabled)
        {
            animator.SetTrigger("Hide");
            // Let the animation handle actually disabling the gameObject
            StartCoroutine(DisableAfterAnimation());
        }
        else
        {
            // Otherwise just disable immediately
            gameObject.SetActive(false);
        }
        
        // Cancel auto-hide if running
        if (autoHideCoroutine != null)
        {
            StopCoroutine(autoHideCoroutine);
            autoHideCoroutine = null;
        }
    }
    
    private IEnumerator AutoHideRoutine(float delay)
    {
        yield return new WaitForSeconds(delay);
        Hide();
    }
    
    private IEnumerator DisableAfterAnimation()
    {
        // Wait for animation to complete (use actual animation length if known)
        yield return new WaitForSeconds(0.5f);
        gameObject.SetActive(false);
    }
    
    // Static utility to show a message using any NotificationPanel in the scene
    public static void ShowGlobalMessage(string message, bool isImportant = false)
    {
        NotificationPanel panel = FindObjectOfType<NotificationPanel>();
        if (panel != null)
        {
            panel.ShowMessage(message, isImportant);
        }
        else
        {
            Debug.LogWarning("No NotificationPanel found in scene!");
        }
    }
}