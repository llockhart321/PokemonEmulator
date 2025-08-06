using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class TitleScreenController : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private Button startButton;
    
    [Header("Scene References")]
    [SerializeField] private string gameSceneName = "GameScene";
    
    // Player progress tracking
    private static bool hasGameStarted = false;
    private static Vector3 lastPlayerPosition;
    private static string lastPlayerScene;
    
    private void Start()
    {
        // Add listener to the start button
        if (startButton != null)
        {
            startButton.onClick.AddListener(StartGame);
        }
        else
        {
            Debug.LogError("Start button reference missing in TitleScreenController!");
        }
    }
    
    public void StartGame()
    {
        if (!hasGameStarted)
        {
            // First time starting the game
            hasGameStarted = true;
            SceneManager.LoadScene(gameSceneName);
        }
        else
        {
            // Resume from where the player left off
            if (!string.IsNullOrEmpty(lastPlayerScene))
            {
                SceneManager.LoadScene(lastPlayerScene);
                // Player position will be handled by GameManager in the game scene
            }
            else
            {
                // Fallback if no last scene is stored
                SceneManager.LoadScene(gameSceneName);
            }
        }
    }
    
    // Static methods to save player progress (called from game scene)
    public static void SavePlayerProgress(Vector3 position, string currentScene)
    {
        lastPlayerPosition = position;
        lastPlayerScene = currentScene;
    }
    
    public static Vector3 GetLastPlayerPosition()
    {
        return lastPlayerPosition;
    }
    
    public static void ResetProgress()
    {
        hasGameStarted = false;
        lastPlayerPosition = Vector3.zero;
        lastPlayerScene = "";
    }
}