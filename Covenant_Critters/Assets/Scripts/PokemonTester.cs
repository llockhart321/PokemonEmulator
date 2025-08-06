using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PokemonTester : MonoBehaviour
{
    void Update()
    {
        // Check if PokemonInventory exists
        if (PokemonInventory.Instance == null || PokemonInventory.Instance.ownedPokemon.Count == 0)
            return;
            
        // Press L to level up the first Pokémon
        if (Input.GetKeyDown(KeyCode.L))
        {
            // Get the first Pokémon
            PokemonInstance pokemon = PokemonInventory.Instance.ownedPokemon[0];
            
            // Level up
            pokemon.level += 1;
            
            // Update HP (similar to what happens in PokemonInstance.LevelUp())
            float oldMaxHP = pokemon.maxHP;
            pokemon.maxHP = pokemon.currentHP;
            pokemon.currentHP += (pokemon.maxHP - oldMaxHP); // Add the HP difference
            
            Debug.Log($"Leveled up {pokemon.basePokemon.pokeName} to level {pokemon.level}! HP: {pokemon.currentHP}/{pokemon.maxHP}");
        }
        
        // Press K to damage the first Pokémon
        if (Input.GetKeyDown(KeyCode.K))
        {
            // Get the first Pokémon
            PokemonInstance pokemon = PokemonInventory.Instance.ownedPokemon[0];
            
            // Reduce HP by 10%
            int damage = Mathf.RoundToInt(pokemon.maxHP * 0.1f);
            pokemon.currentHP = Mathf.Max(1, pokemon.currentHP - damage); // Don't go below 1 HP
            
            Debug.Log($"Damaged {pokemon.basePokemon.pokeName}! HP: {pokemon.currentHP}/{pokemon.maxHP}");
        }
        
        // Press H to heal the first Pokémon
        if (Input.GetKeyDown(KeyCode.H))
        {
            // Get the first Pokémon
            PokemonInstance pokemon = PokemonInventory.Instance.ownedPokemon[0];
            
            // Restore HP to max
            pokemon.currentHP = pokemon.maxHP;
            
            Debug.Log($"Healed {pokemon.basePokemon.pokeName}! HP: {pokemon.currentHP}/{pokemon.maxHP}");
        }
    }
}
