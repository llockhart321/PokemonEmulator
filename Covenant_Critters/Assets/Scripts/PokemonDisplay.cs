using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PokemonDisplay : MonoBehaviour
{
    [SerializeField] private Image pokemonImage;
    [SerializeField] private Text nameText;
    [SerializeField] private Text levelText;
    [SerializeField] private Slider healthSlider;
    [SerializeField] private Text hpText;
    
    private PokemonInstance displayedPokemon;
    
    public void InitializeDisplay(PokemonInstance pokemon)
    {
        displayedPokemon = pokemon;
        
        if (pokemon == null)
            return;
            
        // Set the sprite
        if (pokemonImage != null && pokemon.basePokemon.inventorySprite != null)
        {
            pokemonImage.sprite = pokemon.basePokemon.inventorySprite;
            pokemonImage.enabled = true;
        }
        
        // Set the name
        if (nameText != null)
        {
            nameText.text = pokemon.nickname;
        }
        
        // Set the level
        if (levelText != null)
        {
            levelText.text = "Lv." + pokemon.level;
        }
        
        // Set the health
        UpdateHealthDisplay();
    }
    
    public void UpdateHealthDisplay()
    {
        if (displayedPokemon == null)
            return;
            
        // Update health slider
        if (healthSlider != null)
        {
            healthSlider.maxValue = displayedPokemon.maxHP;
            healthSlider.value = displayedPokemon.currentHP;
            
            // Optional: Change color based on HP percentage
            if (healthSlider.GetComponentInChildren<Image>() != null)
            {
                Image fill = healthSlider.fillRect.GetComponent<Image>();
                if (fill != null)
                {
                    float healthPercentage = (float)displayedPokemon.currentHP / displayedPokemon.maxHP;
                    
                    if (healthPercentage <= 0.2f)
                        fill.color = Color.red;
                    else if (healthPercentage <= 0.5f)
                        fill.color = Color.yellow;
                    else
                        fill.color = Color.green;
                }
            }
        }
        
        // Update HP text
        if (hpText != null)
        {
            hpText.text = displayedPokemon.currentHP + "/" + displayedPokemon.maxHP;
        }
    }
}