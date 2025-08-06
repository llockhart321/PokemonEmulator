using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleWildPokemonTrigger : MonoBehaviour
{
    [Header("Battle Trigger Settings")]
    [SerializeField] private LayerMask playerLayer;
    [SerializeField] private float encounterChance = 0.3f;
    [SerializeField] private float cooldownDuration = 3f; // Cooldown in seconds

    [Header("Wild Pokemon Settings")]
    [SerializeField] private List<Pokemon> possiblePokemon = new List<Pokemon>();
    [SerializeField] private int minLevel = 2;
    [SerializeField] private int maxLevel = 15;

    // Static cooldown timer to prevent multiple encounters
    private static float globalCooldownTimer = 0f;
    private static bool isOnCooldown = false;

    private void Update()
    {
        // Update cooldown timer
        if (isOnCooldown)
        {
            globalCooldownTimer -= Time.deltaTime;
            if (globalCooldownTimer <= 0)
            {
                isOnCooldown = false;
                Debug.Log("Wild encounter cooldown ended");
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Check if we're on cooldown
        if (isOnCooldown)
        {
            Debug.Log("Wild encounter on cooldown, skipping check");
            return;
        }

        if (((1 << collision.gameObject.layer) & playerLayer) != 0 && Random.value <= encounterChance)
        {
            Debug.Log("Wild Pokemon encounter triggered!");

            // Start cooldown
            globalCooldownTimer = cooldownDuration;
            isOnCooldown = true;

            PokemonInstance enemyPokemon = null;

            if (possiblePokemon.Count > 0)
            {
                var basePokemon = possiblePokemon[Random.Range(0, possiblePokemon.Count)];
                int level = Random.Range(minLevel, maxLevel + 1);
                enemyPokemon = PokemonManager.Instance.CreatePokemonInstance(basePokemon, level);
                Debug.Log($"Creating wild {basePokemon.pokeName} at level {level}");
            }
            else
            {
                int level = Random.Range(minLevel, maxLevel + 1);
                enemyPokemon = PokemonManager.Instance.CreateRandomPokemonInstance(level);
                Debug.Log($"Creating random wild Pokemon at level {level}");
            }

            if (enemyPokemon == null)
            {
                Debug.LogError("Failed to create wild Pokemon.");
                isOnCooldown = false; // Reset cooldown if creation fails
                return;
            }

            // Start wild battle through manager
            BattleSystemManager.Instance.StartWildBattle(enemyPokemon);
        }
    }

    // Static method to reset cooldown (could be useful when changing areas)
    public static void ResetCooldown()
    {
        isOnCooldown = false;
        globalCooldownTimer = 0f;
    }
}