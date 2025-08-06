
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ScrollPopup : MonoBehaviour
{
    [Header("Popup Settings")]
    [SerializeField] private GameObject popupPanel;
    [SerializeField] private Image scrollImage;
    [SerializeField] private TextMeshProUGUI messageText;
    [SerializeField] private Button closeButton;
    [SerializeField] private float fadeDuration = 1.0f;
    [SerializeField] private float scaleUpDuration = 0.5f;
    [SerializeField] private float typingSpeed = 0.05f;
    
    [Header("Session Control")]
    //[SerializeField] private string sessionKey = "GameSessionStarted";
    
    [TextArea(5, 10)]
    [SerializeField] private string scrollMessage = "Hear ye, hear ye!\nHere is your first pokemon, your task is to travel the island, fighting each of the trainers until you get to the final challenge.";
    
    // Static instance for easy access from other scripts
    public static ScrollPopup Instance { get; private set; }
    
    // Static flag to track if popup has been shown in this session
    private static bool popupShownThisSession = false;
    
    private void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Keep this object when changing scenes
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
            return;
        }
    }
    
    private void Start()
    {
        // Set up the close button
        closeButton.onClick.AddListener(ClosePopup);
        
        // Adjust line spacing to make gaps smaller
        messageText.lineSpacing = 0.5f; // Adjust this value (lower = smaller gap)
        
        // Make sure the popup is hidden initially
        popupPanel.SetActive(false);
        
        // Check if this is the first time in this session
        if (!popupShownThisSession)
        {
            ShowPopup();
            // Mark that we've shown the popup this session
            popupShownThisSession = true;
        }
    }

    public void ShowPopupWithMessage(string message)
    {
        // Store the message
        scrollMessage = message;
        
        // Show the popup
        ShowPopup();
    }
    
    public void ShowPopup()
    {
        // Reset the panel state
        popupPanel.SetActive(true);
        popupPanel.transform.localScale = Vector3.zero;
        
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
        closeButton.gameObject.SetActive(true);
        
        // Start the animation sequence
        StartCoroutine(AnimatePopup());
    }
    
    private IEnumerator AnimatePopup()
    {
        // Scale up animation
        float elapsedTime = 0;
        while (elapsedTime < scaleUpDuration)
        {
            float scale = Mathf.SmoothStep(0, 1, elapsedTime / scaleUpDuration);
            popupPanel.transform.localScale = new Vector3(scale, scale, scale);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        popupPanel.transform.localScale = Vector3.one;
        
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
        yield return StartCoroutine(TypewriterEffect(scrollMessage));
        
        // Show the close button
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
    
    private void ClosePopup()
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
        popupPanel.SetActive(false);
    }
    
    // Method to be called from Options Menu to view the scroll again
    public void ShowScrollFromOptionsMenu()
    {
        ShowPopup();
    }
    
    // Method to force the popup to show on the next session
    public void ResetForNextSession()
    {
        popupShownThisSession = false;
    }
    
    // Method to be called when starting a new game
    public void ResetForNewGame()
    {
        // Reset the session flag
        popupShownThisSession = false;
        Debug.Log("Scroll popup reset as part of new game initialization.");
    }
}