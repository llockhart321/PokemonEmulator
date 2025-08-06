using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PokemonSwapper : MonoBehaviour
{
    public GameObject[] pokemonEntries;
    
    private GameObject[] selectionMarkers;
    private GameObject[] hollowMarkers;
    
    private int currentSelection = 0;
    private int firstSelection = -1;
    
    // Binary blink settings
    public float blinkRate = 0.4f;
    private bool isVisible = true;
    private float nextBlinkTime = 0f;
    
    // Track which Pokemon is at which display slot
    private int[] pokemonIndices;
    
    // Store original HP bar widths
    private float[] healthBarOriginalWidths;
    
    // Store original HP bar positions
    private Vector2[] healthBarOriginalPositions;
    
    void Start()
    {
        // Initialize health bar widths and positions arrays
        healthBarOriginalWidths = new float[pokemonEntries.Length];
        healthBarOriginalPositions = new Vector2[pokemonEntries.Length];
        
        // Collect original HP bar widths and positions first
        for (int i = 0; i < pokemonEntries.Length; i++)
        {
            Transform healthBarFill = FindHealthBarFill(pokemonEntries[i].transform);
            if (healthBarFill != null)
            {
                RectTransform rectTransform = healthBarFill.GetComponent<RectTransform>();
                healthBarOriginalWidths[i] = rectTransform.sizeDelta.x;
                healthBarOriginalPositions[i] = rectTransform.anchoredPosition;
            }
        }
        
        // Load Pokemon from inventory AFTER getting original bar widths
        LoadPokemonFromInventory();
        
        // THEN create markers since we now know how many active entries we have
        selectionMarkers = new GameObject[pokemonEntries.Length];
        hollowMarkers = new GameObject[pokemonEntries.Length];
        
        for (int i = 0; i < pokemonEntries.Length; i++)
        {
            // Skip inactive entries
            if (!pokemonEntries[i].activeSelf)
                continue;
                
            // Create solid marker
            GameObject marker = new GameObject("SelectionMarker");
            marker.transform.SetParent(pokemonEntries[i].transform, false);
            
            TextMeshProUGUI triangleText = marker.AddComponent<TextMeshProUGUI>();
            triangleText.text = "►";
            triangleText.color = Color.black;
            triangleText.fontSize = 48;
            triangleText.alignment = TextAlignmentOptions.Center;
            
            RectTransform rt = marker.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0, 0.5f);
            rt.anchorMax = new Vector2(0, 0.5f);
            rt.pivot = new Vector2(0, 0.5f);
            rt.sizeDelta = new Vector2(50, 50);
            rt.anchoredPosition = new Vector2(-150, 0);
            
            selectionMarkers[i] = marker;
            marker.SetActive(false);
            
            // Create hollow marker
            GameObject hollow = new GameObject("HollowMarker");
            hollow.transform.SetParent(pokemonEntries[i].transform, false);
            
            TextMeshProUGUI hollowText = hollow.AddComponent<TextMeshProUGUI>();
            hollowText.text = "▻";
            hollowText.color = Color.gray;
            hollowText.fontSize = 48;
            hollowText.alignment = TextAlignmentOptions.Center;
            
            RectTransform hollowRT = hollow.GetComponent<RectTransform>();
            hollowRT.anchorMin = new Vector2(0, 0.5f);
            hollowRT.anchorMax = new Vector2(0, 0.5f);
            hollowRT.pivot = new Vector2(0, 0.5f);
            hollowRT.sizeDelta = new Vector2(50, 50);
            hollowRT.anchoredPosition = new Vector2(-150, 0);
            
            hollowMarkers[i] = hollow;
            hollow.SetActive(false);
        }
        
        // Make sure current selection points to an active entry
        currentSelection = 0;
        // Make sure the first marker is active
        if (currentSelection < selectionMarkers.Length && selectionMarkers[currentSelection] != null)
            selectionMarkers[currentSelection].SetActive(true);
    }
    
    private Transform FindHealthBarFill(Transform parent)
    {
        foreach (Transform child in parent)
        {
            if (child.name == "HealthBarBackground")
            {
                Transform healthBarFill = child.Find("HealthBarFill");
                if (healthBarFill != null)
                    return healthBarFill;
            }
        }
        return null;
    }
    
    // Method to update health bar with proper positioning
    private void UpdateHealthBar(Transform healthBarFill, float originalWidth, Vector2 originalPosition, float hpRatio)
    {
        if (healthBarFill != null)
        {
            RectTransform rectTransform = healthBarFill.GetComponent<RectTransform>();
            
            // Calculate new width based on HP ratio
            float newWidth = originalWidth * hpRatio;
            
            // Update size
            Vector2 sizeDelta = rectTransform.sizeDelta;
            sizeDelta.x = newWidth;
            rectTransform.sizeDelta = sizeDelta;
            
            // Keep using original position
            rectTransform.anchoredPosition = originalPosition;
            
            // Make sure the HP bar fill is enabled
            Image healthBarImage = healthBarFill.GetComponent<Image>();
            if (healthBarImage != null)
                healthBarImage.enabled = true;
        }
    }
    
    void LoadPokemonFromInventory()
    {
        if (PokemonInventory.Instance != null)
        {
            List<PokemonInstance> ownedPokemon = PokemonInventory.Instance.ownedPokemon;
            
            // Keep track of how many valid entries we have
            int validEntryCount = 0;
            
            // Iterate through all slot entries
            for (int i = 0; i < pokemonEntries.Length; i++)
            {
                GameObject entry = pokemonEntries[i];
                
                // Find all UI components
                Transform iconTransform = null;
                TextMeshProUGUI nameText = null;
                TextMeshProUGUI levelText = null;
                TextMeshProUGUI healthText = null;
                Transform healthBarFill = null;
                Transform healthBarBackground = null;
                TextMeshProUGUI hpText = null;
                
                foreach (Transform child in entry.transform)
                {
                    if (child.name == "PokemonIcon")
                        iconTransform = child;
                    else if (child.name == "PokemonName")
                        nameText = child.GetComponent<TextMeshProUGUI>();
                    else if (child.name == "LevelText")
                        levelText = child.GetComponent<TextMeshProUGUI>();
                    else if (child.name == "PokemonHealth")
                        healthText = child.GetComponent<TextMeshProUGUI>();
                    else if (child.name == "HealthBarBackground")
                    {
                        healthBarBackground = child;
                        // Look for HealthBarFill as a child of HealthBarBackground
                        healthBarFill = child.Find("HealthBarFill");
                    }
                    else if (child.name == "HP_Text")
                        hpText = child.GetComponent<TextMeshProUGUI>();
                }
                
                // If we have a Pokemon for this slot, display it
                Debug.Log("Total ownedPokemon: " + ownedPokemon.Count);
                if (i < ownedPokemon.Count)
                {
                    PokemonInstance pokemon = ownedPokemon[i];
                    Debug.Log("Pokemon " + i + ": " + ownedPokemon[i].nickname + " HP: " + ownedPokemon[i].getCurrentHP());
                    
                    // Set icon
                    if (iconTransform != null)
                    {
                        Image iconImage = iconTransform.GetComponent<Image>();
                        if (iconImage != null && pokemon.basePokemon.inventorySprite != null)
                        {
                            iconImage.sprite = pokemon.basePokemon.inventorySprite;
                            iconImage.enabled = true;
                        }
                    }
                    
                    // Set name
                    if (nameText != null)
                    {
                        nameText.text = pokemon.basePokemon.pokeName;
                        nameText.enabled = true;
                        Debug.Log("naming "+pokemon.nickname+" hp at "+pokemon.getCurrentHP()+" were looking at poke "+i+" going to "+nameText.GetHashCode());
                    }
                    
                    // Set level - using the Pokemon's actual level data
                    if (levelText != null)
                    {
                        levelText.text = "Lv. " + pokemon.level;
                        levelText.enabled = true;
                    }
                    
                    // Set health text
                    float currentHP = pokemon.getCurrentHP();
                    float maxHP = pokemon.maxHP;
                    
                    if (healthText != null)
                    {
                        Debug.Log("showing "+pokemon.nickname+" hp at "+pokemon.getCurrentHP()+" were looking at poke "+i+" going to "+healthText.GetHashCode());
                        healthText.text = "HP:\n" + currentHP + "/" + maxHP;
                        healthText.enabled = true;
                    }
                    
                    // Update HP bar with new method to maintain left edge position
                    if (healthBarFill != null && healthBarOriginalWidths[i] > 0)
                    {
                        float hpRatio = (float)currentHP / maxHP;
                        UpdateHealthBar(healthBarFill, healthBarOriginalWidths[i], healthBarOriginalPositions[i], hpRatio);
                    }
                    
                    // Ensure HP text is visible
                    if (hpText != null)
                        hpText.enabled = true;
                    
                    // Ensure health bar background is visible
                    if (healthBarBackground != null)
                        healthBarBackground.gameObject.SetActive(true);
                    
                    // Make entry visible and count it as valid
                    entry.SetActive(true);
                    validEntryCount++;
                }
                else
                {
                    // Empty slot - completely hide the entry
                    entry.SetActive(false);
                }
            }
            
            // Resize the pokemonIndices array to match only valid entries
            pokemonIndices = new int[validEntryCount];
            for (int i = 0; i < validEntryCount; i++)
            {
                pokemonIndices[i] = i;
            }
        }
    }
    
    void Update()
    {
        // Binary blink effect
        if (Time.time > nextBlinkTime)
        {
            isVisible = !isVisible;
            nextBlinkTime = Time.time + blinkRate;
            
            if (currentSelection < selectionMarkers.Length && selectionMarkers[currentSelection] != null)
            {
                TextMeshProUGUI text = selectionMarkers[currentSelection].GetComponent<TextMeshProUGUI>();
                if (text != null)
                {
                    text.color = new Color(0, 0, 0, isVisible ? 1 : 0);
                }
            }
        }
        
        // Count how many active entries we have
        int activeEntryCount = 0;
        for (int i = 0; i < pokemonEntries.Length; i++)
        {
            if (pokemonEntries[i].activeSelf)
                activeEntryCount++;
        }
        
        // Only allow navigation if we have more than one Pokémon
        if (activeEntryCount > 1)
        {
            // Handle navigation - only between active entries
            if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                // Hide current marker
                if (currentSelection < selectionMarkers.Length && selectionMarkers[currentSelection] != null)
                    selectionMarkers[currentSelection].SetActive(false);
                
                // Update selection
                int nextSelection = currentSelection;
                do {
                    nextSelection--;
                    if (nextSelection < 0)
                        nextSelection = pokemonEntries.Length - 1;
                } while (!pokemonEntries[nextSelection].activeSelf && nextSelection != currentSelection);
                
                currentSelection = nextSelection;
                
                // Show new marker
                if (currentSelection < selectionMarkers.Length && selectionMarkers[currentSelection] != null)
                {
                    selectionMarkers[currentSelection].SetActive(true);
                    isVisible = true;
                }
            }
            else if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                // Hide current marker
                if (currentSelection < selectionMarkers.Length && selectionMarkers[currentSelection] != null)
                    selectionMarkers[currentSelection].SetActive(false);
                
                // Update selection
                int nextSelection = currentSelection;
                do {
                    nextSelection++;
                    if (nextSelection >= pokemonEntries.Length)
                        nextSelection = 0;
                } while (!pokemonEntries[nextSelection].activeSelf && nextSelection != currentSelection);
                
                currentSelection = nextSelection;
                
                // Show new marker
                if (currentSelection < selectionMarkers.Length && selectionMarkers[currentSelection] != null)
                {
                    selectionMarkers[currentSelection].SetActive(true);
                    isVisible = true;
                }
            }
        }
        
        // Handle Enter key for selection - only allow swapping if we have multiple Pokemon
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            if (activeEntryCount > 1)
            {
                if (firstSelection == -1)
                {
                    // First selection
                    firstSelection = currentSelection;
                    hollowMarkers[firstSelection].SetActive(true);
                }
                else
                {
                    // Second selection - swap and reset
                    if (firstSelection != currentSelection)
                    {
                        SwapPokemon(firstSelection, currentSelection);
                        
                        // Also swap in the actual PokemonInventory
                        SwapPokemonInInventory(pokemonIndices[firstSelection], pokemonIndices[currentSelection]);
                    }
                    
                    // Hide the hollow marker
                    hollowMarkers[firstSelection].SetActive(false);
                    firstSelection = -1;
                }
            }
        }
        
        // Cancel selection with Escape
        if (Input.GetKeyDown(KeyCode.Escape) && firstSelection != -1)
        {
            hollowMarkers[firstSelection].SetActive(false);
            firstSelection = -1;
        }
    }
    
    void SwapPokemonInInventory(int index1, int index2)
    {
        if (PokemonInventory.Instance != null && 
            index1 < PokemonInventory.Instance.ownedPokemon.Count && 
            index2 < PokemonInventory.Instance.ownedPokemon.Count)
        {
            // Swap the actual Pokemon instances in the inventory
            PokemonInstance temp = PokemonInventory.Instance.ownedPokemon[index1];
            PokemonInventory.Instance.ownedPokemon[index1] = PokemonInventory.Instance.ownedPokemon[index2];
            PokemonInventory.Instance.ownedPokemon[index2] = temp;
        }
    }
    
    void SwapPokemon(int displayPos1, int displayPos2)
    {
        // Find the PokemonIcon, PokemonName, LevelText, PokemonHealth in each entry
        Transform entry1 = pokemonEntries[displayPos1].transform;
        Transform entry2 = pokemonEntries[displayPos2].transform;
        
        // Get all the components we need to swap for Entry 1
        Transform icon1 = null;
        TextMeshProUGUI name1 = null;
        TextMeshProUGUI level1 = null;
        TextMeshProUGUI health1 = null;
        GameObject healthBarBackground1 = null;
        
        // Get all the components we need to swap for Entry 2
        Transform icon2 = null;
        TextMeshProUGUI name2 = null;
        TextMeshProUGUI level2 = null;
        TextMeshProUGUI health2 = null;
        GameObject healthBarBackground2 = null;
        
        // Find all components by name in Entry 1
        foreach (Transform child in entry1)
        {
            if (child.name == "PokemonIcon")
                icon1 = child;
            else if (child.name == "PokemonName")
                name1 = child.GetComponent<TextMeshProUGUI>();
            else if (child.name == "LevelText")
                level1 = child.GetComponent<TextMeshProUGUI>();
            else if (child.name == "PokemonHealth")
                health1 = child.GetComponent<TextMeshProUGUI>();
            else if (child.name == "HealthBarBackground")
            {
                healthBarBackground1 = child.gameObject;
            }
        }
        
        // Find all components by name in Entry 2
        foreach (Transform child in entry2)
        {
            if (child.name == "PokemonIcon")
                icon2 = child;
            else if (child.name == "PokemonName")
                name2 = child.GetComponent<TextMeshProUGUI>();
            else if (child.name == "LevelText")
                level2 = child.GetComponent<TextMeshProUGUI>();
            else if (child.name == "PokemonHealth")
                health2 = child.GetComponent<TextMeshProUGUI>();
            else if (child.name == "HealthBarBackground")
            {
                healthBarBackground2 = child.gameObject;
            }
        }
        
        // Swap icon images
        if (icon1 != null && icon2 != null)
        {
            Image iconImg1 = icon1.GetComponent<Image>();
            Image iconImg2 = icon2.GetComponent<Image>();
            
            if (iconImg1 != null && iconImg2 != null)
            {
                Sprite tempSprite = iconImg1.sprite;
                iconImg1.sprite = iconImg2.sprite;
                iconImg2.sprite = tempSprite;
            }
        }
        
        // Swap text contents
        if (name1 != null && name2 != null)
        {
            string tempName = name1.text;
            name1.text = name2.text;
            name2.text = tempName;
        }
        
        if (level1 != null && level2 != null)
        {
            string tempLevel = level1.text;
            level1.text = level2.text;
            level2.text = tempLevel;
        }
        
        if (health1 != null && health2 != null)
        {
            string tempHealth = health1.text;
            health1.text = health2.text;
            health2.text = tempHealth;
        }
        
        // COMPLETELY DIFFERENT APPROACH: Physically swap the entire health bar GameObjects
        if (healthBarBackground1 != null && healthBarBackground2 != null)
        {
            // Store original positions and parent transforms
            Vector3 pos1 = healthBarBackground1.transform.localPosition;
            Vector3 pos2 = healthBarBackground2.transform.localPosition;
            Quaternion rot1 = healthBarBackground1.transform.localRotation;
            Quaternion rot2 = healthBarBackground2.transform.localRotation;
            Vector3 scale1 = healthBarBackground1.transform.localScale;
            Vector3 scale2 = healthBarBackground2.transform.localScale;
            
            Transform parent1 = healthBarBackground1.transform.parent;
            Transform parent2 = healthBarBackground2.transform.parent;
            
            int siblingIndex1 = healthBarBackground1.transform.GetSiblingIndex();
            int siblingIndex2 = healthBarBackground2.transform.GetSiblingIndex();
            
            // Swap parents (this physically moves the GameObjects)
            healthBarBackground1.transform.SetParent(parent2, false);
            healthBarBackground2.transform.SetParent(parent1, false);
            
            // Restore original positions in new parents
            healthBarBackground1.transform.localPosition = pos2;
            healthBarBackground2.transform.localPosition = pos1;
            healthBarBackground1.transform.localRotation = rot2;
            healthBarBackground2.transform.localRotation = rot1;
            healthBarBackground1.transform.localScale = scale2;
            healthBarBackground2.transform.localScale = scale1;
            
            // Restore sibling indices to maintain hierarchy order
            healthBarBackground1.transform.SetSiblingIndex(siblingIndex2);
            healthBarBackground2.transform.SetSiblingIndex(siblingIndex1);
            
            // Force layout refresh
            LayoutRebuilder.ForceRebuildLayoutImmediate(entry1.GetComponent<RectTransform>());
            LayoutRebuilder.ForceRebuildLayoutImmediate(entry2.GetComponent<RectTransform>());
            
            // Log the swap for debugging
            Debug.Log("Physically swapped health bar GameObjects between entries " + displayPos1 + " and " + displayPos2);
        }
        
        // Swap indices in our tracking array
        int temp = pokemonIndices[displayPos1];
        pokemonIndices[displayPos1] = pokemonIndices[displayPos2];
        pokemonIndices[displayPos2] = temp;
    }
    
    public void RefreshInventoryDisplay()
    {
        LoadPokemonFromInventory();
    }
    
    void OnEnable()
    {
        // This will be called whenever the GameObject becomes active
        LoadPokemonFromInventory();
    }
    
    // This version of TestUpdateFirstPokemon is kept for testing but cleaned up
    public void TestUpdateFirstPokemon()
    {
        if (PokemonInventory.Instance == null || PokemonInventory.Instance.ownedPokemon.Count == 0)
            return;

        if (pokemonEntries.Length == 0 || !pokemonEntries[0].activeSelf)
            return;

        PokemonInstance pokemon = PokemonInventory.Instance.ownedPokemon[0];
        Transform entry = pokemonEntries[0].transform;
        TextMeshProUGUI nameText = null;
        TextMeshProUGUI levelText = null;
        TextMeshProUGUI healthText = null;
        Transform healthBarFill = null;

        foreach (Transform child in entry)
        {
            if (child.name == "PokemonName")
                nameText = child.GetComponent<TextMeshProUGUI>();
            else if (child.name == "LevelText")
                levelText = child.GetComponent<TextMeshProUGUI>();
            else if (child.name == "PokemonHealth")
                healthText = child.GetComponent<TextMeshProUGUI>();
            else if (child.name == "HealthBarBackground")
            {
                healthBarFill = child.Find("HealthBarFill");
            }
        }

        if (nameText != null)
            nameText.text = pokemon.basePokemon.pokeName;

        if (levelText != null)
            levelText.text = "Lv. " + pokemon.level;

        if (healthText != null)
            healthText.text = "HP:\n" + pokemon.currentHP + "/" + pokemon.maxHP;

        // Update HP bar with proper positioning
        if (healthBarFill != null && healthBarOriginalWidths[0] > 0)
        {
            float hpRatio = (float)pokemon.currentHP / pokemon.maxHP;
            UpdateHealthBar(healthBarFill, healthBarOriginalWidths[0], healthBarOriginalPositions[0], hpRatio);
        }
    }
}
