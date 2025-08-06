using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

// This will be a singleton to manage battle data and transitions
public class BattleSystemManager : MonoBehaviour
{
    // Singleton instance
    public static BattleSystemManager Instance { get; private set; }
    
    // Battle data storage
    private BattleData currentBattleData;
    
    // Battle type enum
    public enum BattleType { None, Trainer, Wild }
    private BattleType currentBattleType = BattleType.None;
    
    // Scene transition info
    [SerializeField] private string battleSceneName = "BattleScene";
    
    // List to track defeated trainers
    private List<string> defeatedTrainers = new List<string>();
    
    // Flag to track if a battle is in progress
    private bool isBattleInProgress = false;

    //public GameObject ship;
    
    private void Awake()
    {
        // Set up singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            //ship.SetActive(false);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }
    
    // Start a trainer battle
    public void StartTrainerBattle(List<PokemonInstance> enemyTeam, string trainerName, Sprite trainerSprite)
    {
        // If already in battle, ignore this call
        if (isBattleInProgress)
        {
            Debug.LogWarning("Attempted to start a battle while another is in progress. Ignored.");
            return;
        }
        
        // Check if this trainer is already defeated
        if (IsTrainerDefeated(trainerName))
        {
            Debug.Log($"Trainer {trainerName} is already defeated. Not starting battle.");
            return;
        }
        
        // Set battle in progress
        isBattleInProgress = true;
        
        // Store battle data
        currentBattleData = new BattleData(enemyTeam, true, trainerName, trainerSprite);
        currentBattleType = BattleType.Trainer;
        
        // Save player position via GameManager
        if (GameManager.Instance != null)
        {
            Vector3 playerPos = GameObject.FindGameObjectWithTag("Player")?.transform.position ?? Vector3.zero;
            // Add a small offset so player doesn't spawn directly on the trigger
            Vector3 offsetPosition = playerPos + new Vector3(0, -1.0f, 0);
            GameManager.Instance.SaveBattleReturnPosition(offsetPosition, SceneManager.GetActiveScene().name);
            Debug.Log($"Saved trainer battle position: {offsetPosition}");
        }
        
        // Load battle scene
        SceneManager.LoadScene(battleSceneName);
    }
    
    // Start a wild Pokemon battle
    public void StartWildBattle(PokemonInstance wildPokemon)
    {
        // If already in battle, ignore this call
        if (isBattleInProgress)
        {
            Debug.LogWarning("Attempted to start a battle while another is in progress. Ignored.");
            return;
        }
        
        // Set battle in progress
        isBattleInProgress = true;
        
        // Create list with single wild Pokemon
        List<PokemonInstance> enemyTeam = new List<PokemonInstance> { wildPokemon };
        
        // Store battle data
        currentBattleData = new BattleData(enemyTeam, false);
        currentBattleType = BattleType.Wild;
        
        // Save player position via GameManager
        if (GameManager.Instance != null)
        {
            Vector3 playerPos = GameObject.FindGameObjectWithTag("Player")?.transform.position ?? Vector3.zero;
            GameManager.Instance.SaveBattleReturnPosition(playerPos, SceneManager.GetActiveScene().name);
            Debug.Log($"Saved wild battle position: {playerPos}");
        }
        
        // Load battle scene
        SceneManager.LoadScene(battleSceneName);
    }
    
    // Get the current battle data
    public BattleData GetCurrentBattleData()
    {
        if (currentBattleData == null)
        {
            Debug.LogWarning("Attempted to access null battle data!");
            // Return a default battle data to prevent crashes
            return new BattleData(new List<PokemonInstance>());
        }
        return currentBattleData;
    }
    
    // Get the current battle type
    public BattleType GetCurrentBattleType()
    {
        return currentBattleType;
    }
    
    // Clear battle data after battle ends
    public void ClearBattleData()
    {
        if (currentBattleData != null)
        {
            currentBattleData.Reset();
            currentBattleData = null;
        }
        
        currentBattleType = BattleType.None;
        isBattleInProgress = false; // Reset battle in progress flag
        
        // Reset the wild encounter cooldown
        SimpleWildPokemonTrigger.ResetCooldown();
        
        Debug.Log("Battle data cleared");
    }
    
        // Add a trainer to the defeated trainers list
    public void AddDefeatedTrainer(string trainerName)
    {
        if (!defeatedTrainers.Contains(trainerName))
        {
            defeatedTrainers.Add(trainerName);
            Debug.Log($"Added {trainerName} to defeated trainers list");
            
            // Save to PlayerPrefs if desired
            SaveDefeatedTrainers();
            
            // Check trainer progress if tracker exists
            if (TrainerProgressTracker.Instance != null)
            {
                TrainerProgressTracker.Instance.CheckTrainerProgress();
            }

            if (trainerName == "Dr. Manjarres"){
                //ship = GameObject.FindGameObjectWithTag("shippp_0");
                //ship.SetActive(true);
                GameManager.Instance.showShip();
                Debug.Log("Defeated DJ");
            }
        }
    }

    
    // Check if a trainer has been defeated
    public bool IsTrainerDefeated(string trainerName)
    {
        return defeatedTrainers.Contains(trainerName);
    }
    
    // Save the defeated trainers list to PlayerPrefs
    private void SaveDefeatedTrainers()
    {
        // Convert list to a single string with comma delimiter
        string trainersList = string.Join(",", defeatedTrainers);
        PlayerPrefs.SetString("DefeatedTrainers", trainersList);
        PlayerPrefs.Save();
    }
    
    // Load defeated trainers from PlayerPrefs
    public void LoadDefeatedTrainers()
    {
        if (PlayerPrefs.HasKey("DefeatedTrainers"))
        {
            string trainersList = PlayerPrefs.GetString("DefeatedTrainers");
            if (!string.IsNullOrEmpty(trainersList))
            {
                defeatedTrainers = new List<string>(trainersList.Split(','));
                Debug.Log($"Loaded {defeatedTrainers.Count} defeated trainers from save");
            }
        }
    }
    
    // Capture a wild Pokemon (for wild battles)
    public void CaptureWildPokemon()
    {
        if (currentBattleType == BattleType.Wild && currentBattleData != null && 
            currentBattleData.enemyPokemon != null && currentBattleData.enemyPokemon.Count > 0)
        {
            PokemonInstance capturedPokemon = currentBattleData.enemyPokemon[0];
            
            // Heal the Pokemon
            capturedPokemon.currentHP = capturedPokemon.maxHP;
            capturedPokemon.attackMod = 0;
            capturedPokemon.defenseMod = 0;
            
            // Add to inventory
            if (PokemonInventory.Instance != null)
            {
                PokemonInventory.Instance.AddPokemon(capturedPokemon);
                Debug.Log($"Captured wild {capturedPokemon.nickname} and added to inventory");
            }
            else
            {
                Debug.LogError("Could not add captured Pokemon to inventory - PokemonInventory.Instance is null");
            }
        }
    }
    
    // Method to check if a battle is in progress
    public bool IsBattleInProgress()
    {
        return isBattleInProgress;
    }
    
    private void OnApplicationQuit()
    {
        // Save defeated trainers when game quits
        SaveDefeatedTrainers();
    }
}