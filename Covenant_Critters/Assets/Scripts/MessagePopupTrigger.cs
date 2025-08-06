using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MessagePopupTrigger : MonoBehaviour
{
    [Header("Popup Settings")]
    [SerializeField] private GameObject messagePanel;
    [SerializeField] private Image scrollImage;
    [SerializeField] private TMP_Text messageText;
    [SerializeField] private Button closeButton;
    [SerializeField] private LayerMask playerLayer;
    
    [Header("Trainer Requirements")]
    [SerializeField] private bool requireAllTrainersDefeated = true;
    [SerializeField] private string alternativeMessage = "You must defeat all trainers before challenging me.";
    
    [Header("Animation Settings")]
    [SerializeField] private float fadeDuration = 1.0f;
    [SerializeField] private float scaleUpDuration = 0.5f;
    [SerializeField] private float typingSpeed = 0.05f;
    [SerializeField] private bool showOnce = true;
    
    [TextArea(5, 10)]
    [SerializeField] private string message = "I am the final boss. Are you ready to challenge me?";
    
    private bool hasInteracted = false;
    
    private void Start()
    {
        // Set up the close button
        if (closeButton != null)
            closeButton.onClick.AddListener(ClosePopup);
            
        // Make sure panel is hidden initially
        if (messagePanel != null)
            messagePanel.SetActive(false);
    }
    
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Check if it's the player
        if (((1 << collision.gameObject.layer) & playerLayer) != 0)
        {
            // Only show if we haven't shown the message yet (if showOnce is true)
            if (!hasInteracted || !showOnce)
            {
                // Check if all required trainers are defeated (if that's required)
                if (requireAllTrainersDefeated)
                {
                    if (CheckAllTrainersDefeated())
                    {
                        Debug.Log("All trainers defeated! Showing final boss message.");
                        ShowPopup(message);
                    }
                    else
                    {
                        Debug.Log("Not all trainers defeated yet. Showing alternative message.");
                        ShowPopup(alternativeMessage);
                    }
                }
                else
                {
                    // No requirement, just show the message
                    Debug.Log("Showing message popup!");
                    ShowPopup(message);
                }
                
                // Mark as interacted for one-time messages
                if (showOnce)
                    hasInteracted = true;
            }
        }
    }
    
    private bool CheckAllTrainersDefeated()
    {
        // Get the TrainerProgressTracker instance and check if all trainers are defeated
        TrainerProgressTracker progressTracker = TrainerProgressTracker.Instance;
        
        if (progressTracker != null)
        {
            return progressTracker.AreAllTrainersDefeated();
        }
        
        // If we can't find the progress tracker, fallback to allow message
        Debug.LogWarning("TrainerProgressTracker not found!");
        return false;
    }
    
    public void ShowPopup(string messageToShow)
    {
        // Reset the panel state
        messagePanel.SetActive(true);
        messagePanel.transform.localScale = Vector3.zero;
        
        // Reset alpha values
        Color scrollColor = scrollImage.color;
        scrollColor.a = 0;
        scrollImage.color = scrollColor;
        
        Color textColor = messageText.color;
        textColor.a = 0;
        messageText.color = textColor;
        
        // Hide the text initially
        messageText.text = "";
        
        // Hide close button initially
        if (closeButton != null)
            closeButton.gameObject.SetActive(false);
        
        // Start the animation sequence with the provided message
        StartCoroutine(AnimatePopup(messageToShow));
    }
    
    private IEnumerator AnimatePopup(string messageToShow)
    {
        // Scale up animation
        float elapsedTime = 0;
        while (elapsedTime < scaleUpDuration)
        {
            float scale = Mathf.SmoothStep(0, 1, elapsedTime / scaleUpDuration);
            messagePanel.transform.localScale = new Vector3(scale, scale, scale);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        messagePanel.transform.localScale = Vector3.one;
        
        // Fade in the scroll image
        elapsedTime = 0;
        while (elapsedTime < fadeDuration)
        {
            float alpha = Mathf.Lerp(0, 1, elapsedTime / fadeDuration);
            Color scrollColor = scrollImage.color;
            scrollColor.a = alpha;
            scrollImage.color = scrollColor;
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        
        // Typewriter text effect
        yield return StartCoroutine(TypewriterEffect(messageToShow));
        
        // Show the close button
        if (closeButton != null)
            closeButton.gameObject.SetActive(true);
    }
    
    private IEnumerator TypewriterEffect(string message)
    {
        messageText.text = "";
        // Make text fully visible
        Color textColor = messageText.color;
        textColor.a = 1;
        messageText.color = textColor;
        
        // Type each character with a delay
        foreach (char c in message)
        {
            messageText.text += c;
            yield return new WaitForSeconds(typingSpeed);
        }
    }
    
    public void ClosePopup()
    {
        StartCoroutine(FadeOutPopup());
    }
    
    private IEnumerator FadeOutPopup()
    {
        // Fade out animation
        float elapsedTime = 0;
        Color scrollColor = scrollImage.color;
        Color textColor = messageText.color;
        
        while (elapsedTime < fadeDuration)
        {
            float alpha = Mathf.Lerp(1, 0, elapsedTime / fadeDuration);
            
            scrollColor.a = alpha;
            scrollImage.color = scrollColor;
            
            textColor.a = alpha;
            messageText.color = textColor;
            
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        
        // Hide the popup
        messagePanel.SetActive(false);
    }
}