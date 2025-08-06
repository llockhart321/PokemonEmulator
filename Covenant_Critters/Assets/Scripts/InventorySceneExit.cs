using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class InventorySceneExit : MonoBehaviour
{
    [SerializeField] private string mainGameSceneName = "MainScene"; // Set this in the inspector
    
    // Store where the player was before entering inventory
    private static Vector3 playerPositionBeforeInventory = Vector3.zero;
    private static string sceneBeforeInventory = "";
    
    private void Start()
    {
        // If we're entering the inventory scene, store the current player position
        if (SceneManager.GetActiveScene().name != mainGameSceneName)
        {
            // This will run when the inventory scene loads
            Debug.Log("Inventory scene loaded, previous scene was: " + sceneBeforeInventory);
        }
    }
    
    void Update()
    {
        // Check if either "I" key or Escape key was pressed this frame
        if (Input.GetKeyDown(KeyCode.I) || Input.GetKeyDown(KeyCode.Escape))
        {
            ExitInventory();
        }
    }
    
    // Call this method when entering inventory from the main game
    public static void SavePlayerPosition(Vector3 position, string sceneName)
    {
        playerPositionBeforeInventory = position;
        sceneBeforeInventory = sceneName;
        Debug.Log($"Saved player position before inventory: {position}, scene: {sceneName}");
    }
    
    public void ExitInventory()
    {
        // Get reference to GameManager
        GameManager gameManager = GameManager.Instance;
        
        if (gameManager != null)
        {
            // Use the GameManager's battle return mechanism
            gameManager.SaveBattleReturnPosition(playerPositionBeforeInventory, sceneBeforeInventory);
            gameManager.ReturnFromBattle(); // This will load the scene and set returningFromBattle = true
            
            Debug.Log($"Exiting inventory, returning to: {sceneBeforeInventory} at position: {playerPositionBeforeInventory}");
        }
        else
        {
            // Fallback if GameManager can't be found
            Debug.LogWarning("GameManager not found. Loading main scene but position may not be preserved.");
            SceneManager.LoadScene(mainGameSceneName);
        }
    }
}