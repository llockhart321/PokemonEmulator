using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PokemonInventory : MonoBehaviour
{
    public static PokemonInventory Instance { get; private set; }
    
    public List<PokemonInstance> ownedPokemon = new List<PokemonInstance>();
    //public Pokemon starterPokemon; // Assign in the Inspector
    public int maxPokemonCapacity = 6;
    public Pokemon starterPokemon;

    
    void Awake()
    {
        // Singleton pattern implementation
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        
        if (ownedPokemon.Count == 0 && starterPokemon != null)
        {
            // Use PokemonManager to create the starter Pokemon
            PokemonInstance starter = PokemonManager.Instance.CreatePokemonInstance(starterPokemon, 5);
            ownedPokemon.Add(starter);
            Debug.Log("Starter Pokémon: " + starterPokemon.pokeName + " added to inventory!");
            Debug.Log("your pokemon "+starter.nickname+" has "+starter.currentHP+" hp. ");
            Debug.Log(starterPokemon.pokeName+" has "+starterPokemon.baseHP+" base hp. ");
            Debug.Log(ownedPokemon[0].nickname+" has "+ownedPokemon[0].maxHP+" " +ownedPokemon[0].attack+ " ?? ");



            
        }
        else if (ownedPokemon.Count == 0 && starterPokemon == null)
        {
            Debug.LogWarning("No starter Pokemon assigned in the inspector!");
        }
        
        
    }

    public bool AddPokemon(PokemonInstance newPokemon)
    {
        if (newPokemon == null)
        {
            Debug.LogError("Attempted to add null Pokemon to inventory!");
            return false;
        }
        
        if (ownedPokemon.Count >= maxPokemonCapacity)
        {
            Debug.Log("Pokemon inventory is full! Cannot add " + newPokemon.basePokemon.pokeName);
            return false;
        }
        
        ownedPokemon.Add(newPokemon);
        Debug.Log(newPokemon.basePokemon.pokeName + " added to inventory!");
        return true;
    }
    
    public void RemovePokemon(PokemonInstance pokemon)
    {
        if (pokemon != null && ownedPokemon.Contains(pokemon))
        {
            ownedPokemon.Remove(pokemon);
            Debug.Log(pokemon.basePokemon.pokeName + " removed from inventory!");
        }
    }
    
    public void RemovePokemonAt(int index)
    {
        if (index >= 0 && index < ownedPokemon.Count)
        {
            string pokeName = ownedPokemon[index].basePokemon.pokeName;
            ownedPokemon.RemoveAt(index);
            Debug.Log(pokeName + " removed from inventory!");
        }
    }
    
    public PokemonInstance GetPokemonAt(int index)
    {
        if (index >= 0 && index < ownedPokemon.Count)
        {
            return ownedPokemon[index];
        }
        return null;
    }
    
    public int GetPokemonCount()
    {
        return ownedPokemon.Count;
    }
    
    public bool HasAvailableSpace()
    {
        return ownedPokemon.Count < maxPokemonCapacity;
    }

    
}