using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    // Singleton pattern
    public static GameManager Instance { get; private set; }
    
    [Header("References")]
    [SerializeField] private GameObject playerPrefab;
    public GameObject ship;
    public Camera mainCamera;
    // Add these variables for battle position tracking
    private Vector3 battleReturnPosition = Vector3.zero;
    private string battleReturnScene = "";
    private bool returningFromBattle = false;
    private bool shipVisible = false;
    
    
    private void Awake()
    {
        // Set up singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            //DontDestroyOnLoad(ship);
            //DontDestroyOnLoad(mainCamera);
            

        }
        else
        {
            Destroy(gameObject);
            return;
        }
        
        // Register to scene loaded event
        SceneManager.sceneLoaded += OnSceneLoaded;
    }
    
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {


        
        mainCamera = Camera.main;
        
        var transition = FindObjectOfType<GridTransition>();
        if (transition != null)
        {
            transition.AssignCamera(mainCamera);
        }
        GameObject ship = GameObject.Find("shippp_0");
        if(shipVisible){

            
            ship.SetActive(true);
        }
        else{
            ship.SetActive(false);
        }

        // Reset the scroll popup
        if (ScrollPopup.Instance != null)
        {
            ScrollPopup.Instance.ResetForNewGame();
        }
        
        // Don't handle player positioning in title screen
        if (scene.name == "StartMenuScene")
            return;
            
        // Find or spawn player and position properly
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        
        if (player == null && playerPrefab != null)
        {
            // Spawn player if not found
            player = Instantiate(playerPrefab, Vector3.zero, Quaternion.identity);
        }
        
        if (player != null)
        {
            // NEW: Check if we're returning from battle
            if (returningFromBattle && scene.name == battleReturnScene)
            {
                // Restore battle position
                CameraFollow.Instance.setTransition(true);
                StartCoroutine(SetPlayerPositionDelayed(player, battleReturnPosition));
                CameraFollow.Instance.setTransition(false);
                
                
                Debug.Log($"Restoring player position after battle: {battleReturnPosition}");
                
                // Reset the flag
                returningFromBattle = false;
            }
            else
            {
                // Regular scene transition logic (unchanged)
                Vector3 startPosition = TitleScreenController.GetLastPlayerPosition();
                if (startPosition != Vector3.zero)
                {
                    CameraFollow.Instance.setTransition(true);
                    CameraFollow.Instance.MoveCameraToPosition(startPosition);
                    player.transform.position = startPosition;
                    CameraFollow.Instance.setTransition(false);
                }
                else
                {
                    // Find a spawn point
                    GameObject spawnPoint = GameObject.FindGameObjectWithTag("SpawnPoint");
                    if (spawnPoint != null)
                    {
                        player.transform.position = spawnPoint.transform.position;
                    }
                }
            }
        }
    }
    
    // NEW: Added coroutine to delay position setting
    private System.Collections.IEnumerator SetPlayerPositionDelayed(GameObject player, Vector3 position)
    {
        // Wait for a short time to make sure everything is initialized
        yield return new WaitForSeconds(0.05f);
        
        if (player != null)
        {
            player.transform.position = position;
             CameraFollow.Instance.setTransition(true);
            CameraFollow.Instance.MoveCameraToPosition(position);
            player.transform.position = position;
            CameraFollow.Instance.setTransition(false);
            Debug.Log($"Player position set to: {position} after delay");
        }
    }
    
    // NEW: Method to save position before battle
    public void SaveBattleReturnPosition(Vector3 position, string sceneName)
    {
        battleReturnPosition = position;
        battleReturnScene = sceneName;
        Debug.Log($"Saved battle return position: {position}, scene: {sceneName}");
    }
    
    // NEW: Method to indicate we're returning from battle
    public void ReturnFromBattle()
    {
        returningFromBattle = true;
        SceneManager.LoadScene(battleReturnScene);
        Debug.Log($"Returning from battle to scene: {battleReturnScene}");
    }
    
    private void OnDestroy()
    {
        // Unregister from scene loaded event
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    public void showShip(){
        shipVisible = true;
    }
}