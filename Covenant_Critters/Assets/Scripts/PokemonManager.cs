using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class PokemonManager : MonoBehaviour
{
    public static PokemonManager Instance { get; private set; }
    
    [SerializeField]
    private List<Pokemon> allBasePokemon = new List<Pokemon>();
    
    // Random generator for stats
    private System.Random random;
    
    void Awake()
    {
        // Singleton pattern implementation
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            random = new System.Random();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    /// <summary>
    /// Creates a Pokemon instance with specific parameters if provided, otherwise uses random values
    /// </summary>
    /// <param name="basePokemon">Base Pokemon (required)</param>
    /// <param name="level">Level (optional, random if not specified)</param>
    /// <returns>A new PokemonInstance</returns>
    public PokemonInstance CreatePokemonInstance(Pokemon basePokemon, int? level = null)
    {
        if (basePokemon == null)
        {
            Debug.LogError("Cannot create a Pokemon instance with null base Pokemon!");
            return null;
        }
        
        // Use provided level or generate random level between 1 and 20
        int actualLevel = level ?? random.Next(1, 21);
        
        // Create the instance with required parameters
        PokemonInstance newPokemon = new PokemonInstance(basePokemon, actualLevel);
        
        // Randomize some additional stats
        //RandomizeStats(newPokemon);
        
        return newPokemon;
    }
    
    /// <summary>
    /// Creates a random Pokemon instance from the available base Pokemon
    /// </summary>
    /// <param name="level">Level (optional, random if not specified)</param>
    /// <returns>A new random PokemonInstance</returns>
    public PokemonInstance CreateRandomPokemonInstance(int? level = null)
    {
        if (allBasePokemon.Count == 0)
        {
            Debug.LogError("No base Pokemon available to create a random instance!");
            return null;
        }
        
        // Select a random base Pokemon
        Pokemon randomBase = allBasePokemon[random.Next(allBasePokemon.Count)];
        
        // Create the instance using the other method
        return CreatePokemonInstance(randomBase, level);
    }
    
    /// <summary>
    /// Adds a base Pokemon to the manager's collection
    /// </summary>
    /// <param name="pokemon">The base Pokemon to add</param>
    public void AddBasePokemon(Pokemon pokemon)
    {
        if (pokemon != null && !allBasePokemon.Contains(pokemon))
        {
            allBasePokemon.Add(pokemon);
        }
    }
    
    /// <summary>
    /// Gets a base Pokemon by name
    /// </summary>
    /// <param name="pokemonName">Name of the Pokemon to find</param>
    /// <returns>The base Pokemon or null if not found</returns>
    public Pokemon GetBasePokemonByName(string pokemonName)
    {
        return allBasePokemon.Find(p => p.pokeName == pokemonName);
    }
    
    /// <summary>
    /// Randomizes additional stats for a Pokemon instance
    /// </summary>
    /// <param name="pokemon">The Pokemon instance to modify</param>
    private void RandomizeStats(PokemonInstance pokemon)
    {
        // Apply random variations to stats (±10%)
        float attackVariation = 0.9f + (float)random.NextDouble() * 0.2f;  // 0.9 to 1.1
        float defenseVariation = 0.9f + (float)random.NextDouble() * 0.2f; // 0.9 to 1.1
        float hpVariation = 0.9f + (float)random.NextDouble() * 0.2f;      // 0.9 to 1.1
        
        // Apply variations
        pokemon.attack = Mathf.RoundToInt(pokemon.attack * attackVariation);
        pokemon.defense = Mathf.RoundToInt(pokemon.defense * defenseVariation);
        pokemon.maxHP = Mathf.RoundToInt(pokemon.maxHP * hpVariation);
        pokemon.currentHP = pokemon.maxHP; // Reset current HP to match new max HP
        Debug.Log("\n\n\nTHIS BETTER NOT BE RUNNING\n\n");
    }
}