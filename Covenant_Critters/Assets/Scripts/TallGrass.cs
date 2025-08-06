using UnityEngine;

public class TallGrass : MonoBehaviour
{
    public Pokemon[] wildPokemon; // Assign wild Pokémon species in the Inspector

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            int randomIndex = Random.Range(0, wildPokemon.Length);
            int level = Random.Range(3, 10); // Random level between 3-10
            PokemonInstance wildPokemonInstance = new PokemonInstance(wildPokemon[randomIndex], level);

            Debug.Log("A wild " + wildPokemonInstance.basePokemon.pokeName + " appeared at level " + wildPokemonInstance.level + "!");
            
            // Here, you would transition to a battle scene and pass `wildPokemonInstance`
        }
    }
}
