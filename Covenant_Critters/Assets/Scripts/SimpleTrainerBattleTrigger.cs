using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleTrainerBattleTrigger : MonoBehaviour 
{
    [Header("Trainer Settings")]
    [SerializeField] private string trainerName = "Trainer";
    [SerializeField] private Sprite trainerSprite;
    [SerializeField] private bool battleOnce = true;

    [Header("Battle Conditions")]
    [SerializeField] private LayerMask playerLayer;
    
    // Add a trigger state to prevent multiple triggers
    private bool isTriggering = false;

    [Header("Trainer's Pokemon")]
    [SerializeField] private List<TrainerPokemonData> trainerPokemon = new List<TrainerPokemonData>();

    private bool hasBeenDefeated = false;

    [System.Serializable]
    public class TrainerPokemonData 
    {
        public Pokemon basePokemon;
        public int level;
        public string nickname;
    }

    private void Start() 
    {
        // Query manager to see if this trainer was already defeated
        if (BattleSystemManager.Instance != null && battleOnce) 
        {
            hasBeenDefeated = BattleSystemManager.Instance.IsTrainerDefeated(trainerName);
            if (hasBeenDefeated)
            {
                Debug.Log($"Trainer {trainerName} has already been defeated.");
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision) 
    {
        // Check if this trainer has already been defeated or is currently triggering
        if (hasBeenDefeated && battleOnce) 
        {
            Debug.Log($"Trainer {trainerName} has already been defeated, skipping battle.");
            return;
        }

        if (isTriggering)
        {
            Debug.Log($"Already triggering battle with {trainerName}, ignoring duplicate trigger.");
            return;
        }

        if (((1 << collision.gameObject.layer) & playerLayer) != 0) 
        {
            Debug.Log($"Trainer battle triggered with {trainerName}");
            isTriggering = true;

            // Build the enemy team
            List<PokemonInstance> enemyTeam = new List<PokemonInstance>();
            foreach (var data in trainerPokemon) 
            {
                if (data.basePokemon != null) 
                {
                    var instance = PokemonManager.Instance.CreatePokemonInstance(data.basePokemon, data.level);
                    if (!string.IsNullOrEmpty(data.nickname)) 
                    {
                        instance.SetNickname(data.nickname);
                    }
                    enemyTeam.Add(instance);
                }
            }

            // If team is empty, fallback to one random
            if (enemyTeam.Count == 0) 
            {
                var fallback = PokemonManager.Instance.CreateRandomPokemonInstance(10);
                enemyTeam.Add(fallback);
            }

            // Trigger the battle through the manager
            BattleSystemManager.Instance.StartTrainerBattle(enemyTeam, trainerName, trainerSprite);

            // Mark as defeated (but only when battle concludes - handled in BattleManager)
            if (battleOnce)
            {
                hasBeenDefeated = true;
                // The actual adding to BattleSystemManager.defeatedTrainers will happen when battle ends
            }
        }
    }

    // Method to allow BattleManager to mark trainer as defeated
    public void MarkDefeated()
    {
        hasBeenDefeated = true;
        isTriggering = false;
        
        if (BattleSystemManager.Instance != null)
        {
            BattleSystemManager.Instance.AddDefeatedTrainer(trainerName);
        }
    }

    // Optional reset method if needed externally
    public void ResetBattle() 
    {
        hasBeenDefeated = false;
        isTriggering = false;
    }
}