using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MoodPopupTrigger : MonoBehaviour
{
    [Header("Popup Settings")]
    [SerializeField] private GameObject messagePanel;
    [SerializeField] private Image scrollImage;
    [SerializeField] private TMP_Text messageText;
    [SerializeField] private Button closeButton;
    [SerializeField] private LayerMask playerLayer;
    
    [Header("Animation Settings")]
    [SerializeField] private float fadeDuration = 1.0f;
    [SerializeField] private float scaleUpDuration = 0.5f;
    [SerializeField] private float typingSpeed = 0.05f;
    [SerializeField] private bool showOnce = true;

    [Header("Starter Poke")]
    [SerializeField] public Pokemon starterPokemon;
    
    [TextArea(5, 10)]
    [SerializeField] private string message = "Hello traveler! This is your special message.";
    
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
        // Check if it's the player and we haven't shown the message yet (if showOnce is true)
        if (((1 << collision.gameObject.layer) & playerLayer) != 0 && (!hasInteracted || !showOnce))
        {
            Debug.Log("Showing message popup!");
            ShowPopup();

            // Check if PokemonInventory exists
            if (PokemonInventory.Instance != null)
            {
                // Check if player already has the duck Pokemon
                bool hasDuckPokemon = false;
                
                // Look through all owned Pokemon to check for the duck
                for (int i = 0; i < PokemonInventory.Instance.ownedPokemon.Count; i++)
                {
                    // Check if this Pokemon is the duck (comparing by Pokemon base type)
                    if (PokemonInventory.Instance.ownedPokemon[i].basePokemon == starterPokemon)
                    {
                        hasDuckPokemon = true;
                        Debug.Log("Player already has the duck Pokemon!");
                        break;
                    }
                }
                
                // Only give the duck if player doesn't have it
                if (!hasDuckPokemon)
                {
                    // Create a new duck Pokemon instance at level 5
                    PokemonInstance newPoke = new PokemonInstance(starterPokemon, 5);
                    
                    // Add it to the player's inventory
                    if (PokemonInventory.Instance.AddPokemon(newPoke))
                    {
                        Debug.Log($"Dr. Mood gave you a {newPoke.nickname} to inventory!");
                    }
                    else
                    {
                        Debug.Log("Couldn't add duck Pokemon - inventory might be full.");
                    }
                }
            }
            else
            {
                Debug.LogError("Could not add mood's Pokemon to inventory - PokemonInventory.Instance is null");
            }
            
            // Mark as interacted for one-time messages
            if (showOnce)
                hasInteracted = true;
        }
    }
    
    public void ShowPopup()
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
        yield return StartCoroutine(TypewriterEffect(message));
        
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