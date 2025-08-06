using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class FinalBossTrainerTrigger : MonoBehaviour
{
    [Header("Boss Settings")]
    [SerializeField] private string bossName = "Final Boss";
    [SerializeField] private Sprite bossSprite;
    [SerializeField] private string[] requiredTrainers; // Names of trainers that must be defeated
    [SerializeField] private string notReadyMessage = "Come back when you've proven yourself by defeating all trainers!";
    [SerializeField] private float messageDisplayTime = 3.0f;
    [SerializeField] private bool battleOnce = true;

    [Header("Battle Conditions")]
    [SerializeField] private LayerMask playerLayer;

    [Header("Boss's Pokemon")]
    [SerializeField] private List<BossPokemonData> bossPokemon = new List<BossPokemonData>();

    [Header("UI References")]
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private TextMeshProUGUI dialogueText;

    public GameObject ship;
    

    private bool hasBeenDefeated = false;
    private bool isTriggering = false;

    [System.Serializable]
    public class BossPokemonData
    {
        public Pokemon basePokemon;
        public int level;
        public string nickname;
        //[Tooltip("Optional: special moves for boss Pokemon")]
        //public List<Move> specialMoves;
    }

    private void Start()
    {
        //ship.SetActive(false);
        // Check if boss was already defeated
        if (BattleSystemManager.Instance != null && battleOnce)
        {
            hasBeenDefeated = BattleSystemManager.Instance.IsTrainerDefeated(bossName);
            if (hasBeenDefeated)
            {
                Debug.Log($"Boss {bossName} has already been defeated.");
            }
        }

        // If we don't have UI references, try to find them
        if (dialoguePanel == null)
        {
            dialoguePanel = GameObject.FindGameObjectWithTag("DialoguePanel");
        }

        if (dialogueText == null && dialoguePanel != null)
        {
            dialogueText = dialoguePanel.GetComponentInChildren<TextMeshProUGUI>();
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Check if this boss has already been defeated or is currently triggering
        if (hasBeenDefeated && battleOnce)
        {
            ShowMessage($"You've already proven yourself against {bossName}.");
            return;
        }

        if (isTriggering)
        {
            return;
        }

        if (((1 << collision.gameObject.layer) & playerLayer) != 0)
        {
            Debug.Log($"Final boss {bossName} triggered, checking requirements...");
            isTriggering = true;

            // Check if all required trainers are defeated
            if (!AreAllTrainersDefeated())
            {
                ShowMessage(notReadyMessage);
                StartCoroutine(ResetTriggerAfterDelay(messageDisplayTime));
                return;
            }
            else{

                Debug.Log($"All required trainers defeated! Starting battle with {bossName}");

                // Build the boss team
                List<PokemonInstance> bossTeam = new List<PokemonInstance>();
                foreach (var data in bossPokemon)
                {
                    if (data.basePokemon != null)
                    {
                        var instance = PokemonManager.Instance.CreatePokemonInstance(data.basePokemon, data.level);
                        if (!string.IsNullOrEmpty(data.nickname))
                        {
                            instance.SetNickname(data.nickname);
                        }
                        
                        // If special moves are specified, assign them
                        /*
                        if (data.specialMoves != null && data.specialMoves.Count > 0)
                        {
                            // This assumes your PokemonInstance has a method to set moves
                            // You might need to adjust based on your implementation
                            for (int i = 0; i < Mathf.Min(4, data.specialMoves.Count); i++)
                            {
                                instance.learnedMoves[i] = data.specialMoves[i];
                            }
                        }
                        */
                        
                        bossTeam.Add(instance);
                    }
                }

                // If team is empty, fallback to one strong random
                if (bossTeam.Count == 0)
                {
                    var fallback = PokemonManager.Instance.CreateRandomPokemonInstance(50); // Higher level for boss
                    bossTeam.Add(fallback);
                }

                // Trigger the battle through the manager
                BattleSystemManager.Instance.StartTrainerBattle(bossTeam, bossName, bossSprite);

                if (battleOnce)
                {
                    hasBeenDefeated = true;
                }
            }
        }
    }

    private bool AreAllTrainersDefeated()
    {
        if (BattleSystemManager.Instance == null)
        {
            Debug.LogError("BattleSystemManager.Instance is null!");
            return false;
        }

        if (requiredTrainers == null || requiredTrainers.Length == 0)
        {
            Debug.LogWarning("No required trainers specified for final boss!");
            return true; // If no trainers specified, always allow battle
        }

        foreach (string trainerName in requiredTrainers)
        {
            if (!BattleSystemManager.Instance.IsTrainerDefeated(trainerName))
            {
                Debug.Log($"Trainer {trainerName} not yet defeated!");
                return false;
            }
        }

        return true;
    }

    private void ShowMessage(string message)
    {
        if (dialoguePanel != null && dialogueText != null)
        {
            dialogueText.text = message;
            dialoguePanel.SetActive(true);
            StartCoroutine(HideMessageAfterDelay(messageDisplayTime));
        }
        else
        {
            Debug.Log(message); // Fallback to console if UI not available
        }
    }

    private IEnumerator HideMessageAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(false);
        }
    }

    private IEnumerator ResetTriggerAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        isTriggering = false;
    }

    // Method to allow BattleManager to mark boss as defeated
    public void MarkDefeated()
    {
        hasBeenDefeated = true;
        isTriggering = false;
        
        if (BattleSystemManager.Instance != null)
        {
            BattleSystemManager.Instance.AddDefeatedTrainer(bossName);
        }

        // open teleport to next island
        //ship.SetActive(true);

    }

    // Optional reset method if needed externally
    public void ResetBattle()
    {
        hasBeenDefeated = false;
        isTriggering = false;
    }
}