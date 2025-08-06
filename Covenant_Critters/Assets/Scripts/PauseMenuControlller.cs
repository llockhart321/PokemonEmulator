using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PauseMenuController : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private GameObject pauseMenuPanel;
    [SerializeField] private Button startOverButton;
    [SerializeField] private Button exitToTitleButton;
    
    [Header("Scene References")]
    [SerializeField] private string titleSceneName = "StartMenuScene";
    
    private bool isPaused = false;
    
    private void Start()
    {
        // Ensure pause menu is hidden at start
        if (pauseMenuPanel != null)
        {
            pauseMenuPanel.SetActive(false);
        }
        
        // Add button listeners
        if (startOverButton != null)
        {
            startOverButton.onClick.AddListener(StartOver);
        }
        
        if (exitToTitleButton != null)
        {
            exitToTitleButton.onClick.AddListener(ExitToTitleScreen);
        }
    }
    
    private void Update()
    {
        // Toggle pause menu when Q is pressed
        if (Input.GetKeyDown(KeyCode.Q))
        {
            TogglePauseMenu();
        }
    }
    
    private void TogglePauseMenu()
    {
        isPaused = !isPaused;
        
        if (pauseMenuPanel != null)
        {
            pauseMenuPanel.SetActive(isPaused);
        }
        
        // Pause/unpause the game
        Time.timeScale = isPaused ? 0f : 1f;
    }
    
    public void StartOver()
    {
        // Reset player progress and go to title screen
        TitleScreenController.ResetProgress();
        Time.timeScale = 1f; // Ensure game speed is reset
        SceneManager.LoadScene(titleSceneName);
    }
    
    public void ExitToTitleScreen()
    {
        // Save current player position before leaving
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            TitleScreenController.SavePlayerProgress(
                player.transform.position, 
                SceneManager.GetActiveScene().name
            );
        }
        
        Time.timeScale = 1f; // Ensure game speed is reset
        SceneManager.LoadScene(titleSceneName);
    }
    
    private void OnDestroy()
    {
        // Ensure we reset the time scale when this script is destroyed
        Time.timeScale = 1f;
    }
}