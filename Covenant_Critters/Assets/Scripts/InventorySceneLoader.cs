using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement; // Required for scene management

public class InventorySceneLoader : MonoBehaviour
{
    [SerializeField] private string inventorySceneName = "InventoryScene"; // Set this in the inspector
    
    void Update()
    {
        // Check if the "I" key was pressed this frame
        if (Input.GetKeyDown(KeyCode.I))
        {
            if(BattleSystemManager.Instance.IsBattleInProgress() == false)
                OpenInventory();
        }
    }
    
    public void OpenInventory()
    {
        // Get player position before switching to inventory
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        
        if (player != null)
        {
            // Save the current position and scene name in InventorySceneExit
            InventorySceneExit.SavePlayerPosition(
                player.transform.position,
                SceneManager.GetActiveScene().name
            );
            
            Debug.Log($"Saving player position before inventory: {player.transform.position}");
        }
        
        // Load the inventory scene
        SceneManager.LoadScene(inventorySceneName);
        Debug.Log("Loading inventory scene from invscl: " + inventorySceneName);
    }
}