using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class BattleAction : MonoBehaviour
{
    public GameObject moveSelectorUI; // UI with move options
    [SerializeField] private List<TextMeshProUGUI> moveTexts = new List<TextMeshProUGUI>(); // Text components to display move names
    public Image selectorBox; // the black triangle or highlight box
    [SerializeField] private TextMeshProUGUI textBox;
    [SerializeField] private GameObject actionSelector; // Reference to the Attack/Run options menu

    private int currentMoveIndex = 0;

    private PokemonInstance playerPokemon;
    private PokemonInstance enemyPokemon;
    private float playerAttack;
    private float playerDefense;
    private float enemyAttack;
    private float enemyDefense;

    private bool isChoosingMove = false;
    private bool waitingForContinue = false;
    private bool battleEnded = false;
    private int waitStage = 0;
    
    // Reference to other managers
    private BattleManager battleManager;
    private BattleSystemManager battleSystemManager;

    // Battle result
    private char battleResult = '\0'; // Default no result

    private void Awake()
    {
        // Get references to managers
        battleManager = FindObjectOfType<BattleManager>();
        battleSystemManager = BattleSystemManager.Instance;
        
        if (battleSystemManager == null)
        {
            Debug.LogError("BattleSystemManager.Instance is null! Make sure it exists in the scene.");
        }
    }

    public void StartAttack(PokemonInstance pokemon, PokemonInstance enemy)
    {
        playerPokemon = pokemon;
        enemyPokemon = enemy;

        // Set stats
        attacks = playerPokemon.attacks;
        playerAttack = playerPokemon.attack;
        playerDefense = playerPokemon.defense;
        enemyAttack = enemyPokemon.attack;
        enemyDefense = enemyPokemon.defense;

        if (playerPokemon.attacks == null || playerPokemon.attacks.Count == 0)
        {
            Debug.LogError("This Pokémon has no attacks!");
            moveSelectorUI.SetActive(false);
            isChoosingMove = false;
            return;
        }

        isChoosingMove = true;
        moveSelectorUI.SetActive(true);
        waitingForContinue = false;
        waitStage = 0;
        battleEnded = false;
        
        Debug.Log("Your pokemon " + playerPokemon.nickname + " has " + playerPokemon.currentHP + " hp.");
        LoadMoves();
        UpdateSelectorPosition();
    }

    public List<Pokemon.Attack> attacks;

    private void Update()
    {
        // Handle move selection
        if (isChoosingMove)
        {
            HandleMoveSelection();
        }
        // Handle waiting for player to press Enter to continue
        else if (waitingForContinue)
        {
            if (Input.GetKeyDown(KeyCode.Return))
            {
                ContinueBattle();
            }
        }
    }

    private void HandleMoveSelection()
    {
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            currentMoveIndex = (currentMoveIndex - 1 + playerPokemon.attacks.Count) % playerPokemon.attacks.Count;
            UpdateSelectorPosition();
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            currentMoveIndex = (currentMoveIndex + 1) % playerPokemon.attacks.Count;
            UpdateSelectorPosition();
        }
        else if (Input.GetKeyDown(KeyCode.Return)) // Enter key
        {
            isChoosingMove = false;
            moveSelectorUI.SetActive(false);

            // Player's turn to attack
            Attack(playerPokemon, enemyPokemon, currentMoveIndex);
            waitingForContinue = true;
            waitStage = 1; // Next we check game state after player attack
        }
    }

    private void ContinueBattle()
    {
        switch (waitStage)
        {
            case 1: // Check game state after player attack
                if (CheckGameState())
                {
                    // Battle continues, enemy's turn
                    textBox.text = "Enemy's turn...";
                    
                    // Select a move for the enemy using AI instead of random
                    int enemyMoveIndex = ChooseEnemyMove();
                    
                    // Enemy attacks
                    Attack(enemyPokemon, playerPokemon, enemyMoveIndex);
                    waitStage = 2; // Next we check game state after enemy attack
                }
                else
                {
                    // Battle has ended
                    EndBattle();
                }
                break;
                
            case 2: // Check game state after enemy attack
                if (CheckGameState())
                {
                    // Battle continues, return to main options
                    ShowActionSelector();
                    waitingForContinue = false;
                }
                else
                {
                    // Battle has ended
                    EndBattle();
                }
                break;
                
            default:
                Debug.LogError("Invalid waitStage: " + waitStage);
                waitingForContinue = false;
                break;
        }
    }

    // Enhanced enemy AI to select appropriate moves based on situation
    private int ChooseEnemyMove()
    {
        if (enemyPokemon.attacks == null || enemyPokemon.attacks.Count == 0){
            Debug.Log("Enemy Has No Attacks!!");
            return 1;
        }
        // Simple AI logic for enemy move selection
        
        // If enemy HP is low (less than 30% of max), prioritize defense or attack boost
        if (enemyPokemon.currentHP < enemyPokemon.maxHP * 0.3f)
        {
            // Look for defense boost moves (type 3)
            for (int i = 0; i < enemyPokemon.attacks.Count; i++)
            {
                if (enemyPokemon.attacks[i].type == 3)
                    return i;
            }
            
            // Or attack boost moves (type 2)
            for (int i = 0; i < enemyPokemon.attacks.Count; i++)
            {
                if (enemyPokemon.attacks[i].type == 2)
                    return i;
            }
        }
        
        // If player's attack is high, consider lowering it
        if (playerPokemon.attack + playerPokemon.attackMod > playerPokemon.attack * 1.5f)
        {
            // Look for lower attack moves (type 4)
            for (int i = 0; i < enemyPokemon.attacks.Count; i++)
            {
                if (enemyPokemon.attacks[i].type == 4)
                    return i;
            }
        }
        
        // If player's defense is high, consider lowering it
        if (playerPokemon.defense + playerPokemon.defenseMod > playerPokemon.defense * 1.5f)
        {
            // Look for lower defense moves (type 5)
            for (int i = 0; i < enemyPokemon.attacks.Count; i++)
            {
                if (enemyPokemon.attacks[i].type == 5)
                    return i;
            }
        }
        
        // Otherwise, prefer damage moves but with some randomness
        List<int> damageAttackIndices = new List<int>();
        
        for (int i = 0; i < enemyPokemon.attacks.Count; i++)
        {
            if (enemyPokemon.attacks[i].type == 1) // Standard damage
                damageAttackIndices.Add(i);
        }
        
        if (damageAttackIndices.Count > 0)
        {
            // 75% chance to use a damage move if available
            if (Random.value < 0.75f)
                return damageAttackIndices[Random.Range(0, damageAttackIndices.Count)];
        }
        
        // Fallback to completely random selection
        return Random.Range(0, enemyPokemon.attacks.Count);
    }

    private void ShowActionSelector()
    {

        battleManager.ShowActionSelector();
        /*
        textBox.text = "What will " + playerPokemon.nickname + " do?";
        if (actionSelector != null)
        {
            actionSelector.SetActive(true);
        }
        else if (battleManager != null)
        {
            battleManager.ShowActionSelector();
        }
        else
        {
            Debug.LogError("Action selector not assigned and BattleManager not found!");
        }
        */
    }

    // Enhanced game state checking
    private bool CheckGameState()
    {
        // Check if player has no Pokémon left in inventory
        bool playerHasLivingPokemon = false;
        
        if (PokemonInventory.Instance != null)
        {
            foreach (var pokemon in PokemonInventory.Instance.ownedPokemon)
            {
                if (pokemon.currentHP > 0)
                {
                    playerHasLivingPokemon = true;
                    break;
                }
                else{

                    //if you only have one poke dont let it die
                    if(PokemonInventory.Instance.ownedPokemon.Count == 1)
                    {  
                        Debug.Log(pokemon.nickname+" is all whos left. ending");
                        pokemon.currentHP = 1;
                        this.battleResult = 'd';
                        EndBattle();
                    }
                    else//else kill
                    {
                        // say that poke died.
                        Debug.Log(pokemon.nickname+" has died :( )");
                        

                        //idk if this is right 
                        textBox.text =   playerPokemon.nickname + " has died!";
                        textBox.text += "\n\nPress [Enter] to continue...";
                        //remove them from invintory. 
                        PokemonInventory.Instance.RemovePokemon(pokemon);
                        waitingForContinue = true;
                        waitStage = 2; //enemy turn?
                    }

                }
            }
            
            // If current Pokémon is defeated but player has others, should switch
            if (playerPokemon.currentHP <= 0 && playerHasLivingPokemon)
            {
                // Prompt player to switch to next Pokémon
                SwitchToNextLivingPokemon();
            }

            // Check if enemy's current Pokémon is defeated
            if (enemyPokemon.currentHP <= 0)
            {

                PokemonInstance newEnemyPokemon = battleManager.SwitchToNextEnemyPokemon();
                if(newEnemyPokemon != null){
                    enemyPokemon = newEnemyPokemon;
                }
                
                //waitingForContinue = true;
                //Update();

            }
        }

        if (!playerHasLivingPokemon)
        {
            Debug.Log("All your Pokemon are DEAD :()");
            this.battleResult = 'd'; // defeat
            return false;
        }
        
        // Check if player's current Pokémon is defeated and no switching happened
        if (playerPokemon.currentHP <= 0 && !playerHasLivingPokemon)
        {
            Debug.Log("Your Pokémon " + playerPokemon.nickname + " has fainted. You have no more Pokémon!");
            this.battleResult = 'd'; // defeat
            return false;
        }
        
        // Check if trainer has no Pokémon left or wild Pokémon is defeated
        if (enemyPokemon == null || enemyPokemon.currentHP <= 0)
        {
            Debug.Log("Enemy Pokémon " + enemyPokemon.nickname + " has fainted!");
            this.battleResult = 'v'; // victory
            return false;
        }
        
        // Battle continues
        return true;
    }

    // Switch to the next living Pokémon in player's inventory
    private void SwitchToNextLivingPokemon()
    {
        if (PokemonInventory.Instance == null) return;
        
        foreach (var pokemon in PokemonInventory.Instance.ownedPokemon)
        {
            if (pokemon.currentHP > 0 && pokemon != playerPokemon)
            {
                playerPokemon = pokemon;
                
                // Update UI
                textBox.text = "Go, " + playerPokemon.nickname + "!";
                waitingForContinue = true;
                waitStage = 2; // After switch, enemy gets a turn
                
                // Update battle manager UI
                if (battleManager != null)
                {
                    battleManager.UpdatePlayerPokemon(playerPokemon);
                }
                
                return;
            }
        }
    }

    private void EndBattle()
    {
        if (battleEnded) return; // Prevent multiple calls
        
        battleEnded = true;
        waitingForContinue = false;
        
        // Process the battle result
        switch (this.battleResult)
        {
            case 'v': // Victory
                HandleVictory();
                 //EndBattle();
                break;
                
            case 'd': // Defeat
                HandleDefeat();
                 //EndBattle();
                break;
                
            case 'r': // Ran away
                HandleRunAway();
                 //EndBattle();
                break;
                
            default:
                textBox.text = "Battle ended.";
                break;
        }
        
        // Add message to press Enter to exit
        textBox.text += "\n\nPress [Enter] to continue...";
        
        // Wait for one more Enter press to exit battle
         waitingForContinue = true;
        StartCoroutine(WaitForFinalEnterPress());
    }
    
    private void HandleVictory()
    {
        if (battleManager.IsTrainerBattle())
        {
            // Get trainer name
            string trainerName = battleManager.GetTrainerName();
            
            textBox.text = "You won the battle against " + trainerName + "!";
            
            // Add trainer to defeated trainers list via BattleSystemManager
            if (battleSystemManager != null)
            {
                battleSystemManager.AddDefeatedTrainer(trainerName);
                Debug.Log("Added " + trainerName + " to defeated trainers list");
            }
        }
        else
        {
            // Wild Pokémon battle victory
            textBox.text = "You defeated the wild " + enemyPokemon.nickname + "!";
            
            // Add experience points to player Pokémon
            AwardExperiencePoints();
            
            // Chance to capture the wild Pokémon (show capture dialog)
            if (battleSystemManager != null)
            {
                // Show capture dialog/option
                //textBox.text += "\nWould you like to capture the wild " + enemyPokemon.nickname + "?";
                // This would ideally show a Yes/No option
                // For now we'll auto-capture for simplicity
                battleSystemManager.CaptureWildPokemon();
            }
        }
    }
    
    // Award experience points to player Pokémon after victory
    private void AwardExperiencePoints()
    {
        if (playerPokemon != null)
        {
            // Simple formula: enemy level * multiplier
            float expGained = enemyPokemon.level * 5.0f;
            playerPokemon.experience += expGained;
            
            // Check for level up
            float oldLevel = playerPokemon.level;
            playerPokemon.CheckLevelUp();
            
            textBox.text += "\n" + playerPokemon.nickname + " gained " + expGained + " experience points!";
            
            if (playerPokemon.level > oldLevel)
            {
                textBox.text += "\n" + playerPokemon.nickname + " grew to level " + playerPokemon.level + "!";
            }
        }
    }
    
    private void HandleDefeat()
    {
        textBox.text = "You were defeated!";
        
        // Heal trainer's Pokémon
        //HealPlayerPokemon();
    }
    
    private void HandleRunAway()
    {
        if (actionSelector != null)
            actionSelector.SetActive(false); // Hide Attack/Run buttons
        textBox.text = "Got away safely!";
        
        // No need to heal Pokémon when running
    }
    
   private IEnumerator WaitForFinalEnterPress()
{
    yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.Return));
    
    waitingForContinue = false; // Changed from true to false
    Debug.Log("WHEN DOES THIS RUNNNNN");
    
    // Exit battle and return to the map
    if (battleManager != null)
    {
        // Call the new method instead
        battleManager.FinalizeBattle();
    }
}
    
    private void HealPlayerPokemon()
    {
        // Heal all player Pokémon to full health
        if (PokemonInventory.Instance != null)
        {
            foreach (var pokemon in PokemonInventory.Instance.ownedPokemon)
            {
                pokemon.currentHP = pokemon.maxHP;
                pokemon.attackMod = 0;
                pokemon.defenseMod = 0;
            }
        }
    }

    public void RunFromBattle()
    {
        // Check if it's a trainer battle
        // Successfully escaped



                this.battleResult = 'r'; // ran
                EndBattle();





        /*
        if (battleManager.IsTrainerBattle())
        {
            textBox.text = "You can't run from a trainer battle!";
            waitingForContinue = true;
            waitStage = 2; // After message, enemy gets a turn
        }
        else
        {
            // For wild battles, calculate escape chance based on levels
            float escapeChance = 0.5f + 0.1f * (playerPokemon.level - enemyPokemon.level);
            escapeChance = Mathf.Clamp(escapeChance, 0.25f, 0.95f); // Between 25% and 95%
            
            if (Random.value < escapeChance)
            {
                // Successfully escaped
                battleResult = 'r'; // ran
                EndBattle();
            }
            else
            {
                // Failed to escape
                textBox.text = "Couldn't escape!";
                waitingForContinue = true;
                waitStage = 2; // Enemy gets a free attack
            }
        }
        */
    }

    private void LoadMoves()
    {
        for (int i = 0; i < moveTexts.Count; i++)
        {
            if (i < playerPokemon.attacks.Count)
            {
                moveTexts[i].text = playerPokemon.attacks[i].attackName;
                moveTexts[i].gameObject.SetActive(true);
            }
            else
            {
                moveTexts[i].gameObject.SetActive(false);
            }
        }
    }

    private void UpdateSelectorPosition()
    {
        selectorBox.gameObject.SetActive(true);
        // Move the selector box to the currentMoveIndex
        selectorBox.transform.position = moveTexts[currentMoveIndex].transform.position + new Vector3(-4.5f, 0, 0); 
    }

    private void Attack(PokemonInstance attacker, PokemonInstance defender, int moveIndex)
    {
        // type of attack. 
        // 1. standard damage. 
        // 2. raise your damage stat 
        // 3. raise your defense.  
        // 4. lower their attack  
        // 5. lower their defense

        string attackClass = attacker.typeClass; 
        string defendClass = defender.typeClass;
        float damageMultiplier = 1.0f;
        string effective = "effective";

        switch (attacker.attacks[moveIndex].type)
        {
            case 1: // Standard damage
                switch (attackClass.ToLower()) // Convert to lowercase for consistency
                {
                    case "fire":
                        switch (defendClass.ToLower())
                        {
                            case "water":
                                damageMultiplier = 0.5f; // Not very effective
                                effective = "It is not Very Effective.";
                                break;
                            case "grass":
                                damageMultiplier = 2.0f; // Super effective
                                effective = "It is Super Effective.";
                                break;
                            default:
                                damageMultiplier = 1.0f; // Normal effectiveness
                                effective = "It is effective.";
                                break;
                        }
                        break;
                        
                    case "water":
                        switch (defendClass.ToLower())
                        {
                            case "fire":
                                damageMultiplier = 2.0f; // Super effective
                                effective = "It is Super Effective!";
                                break;
                            case "grass":
                                damageMultiplier = 0.5f; // Not very effective
                                effective = "It is not very effective.";
                                break;
                            default:
                                damageMultiplier = 1.0f; // Normal effectiveness
                                effective = "It is effective.";
                                break;
                        }
                        break;
                        
                    case "grass":
                        switch (defendClass.ToLower())
                        {
                            case "fire":
                                damageMultiplier = 0.5f; // Not very effective
                                effective = "It is not very effective.";
                                break;
                            case "water":
                                damageMultiplier = 2.0f; // Super effective
                                effective = "It is super effective!";
                                break;
                            default:
                                damageMultiplier = 1.0f; // Normal effectiveness
                                effective = "It is effective.";
                                break;
                        }
                        break;
                        
                    case "normal":
                        // Normal type doesn't have super effective matches in the basic type system
                        damageMultiplier = 1.0f;
                        effective = "It is effective.";
                        break;
                        
                    default:
                        damageMultiplier = 1.0f;
                        effective = "It is effective.";
                        break;
                }

                // (((damage + attack stat) * effectiveness) - defence stat)
                float totalDamage = (((attacker.attacks[moveIndex].amount + (attacker.attack + attacker.attackMod)) * damageMultiplier) - (defender.defense + defender.defenseMod));

                // Make sure damage isn't negative
                if (totalDamage > 0) {
                    defender.currentHP -= totalDamage;
                    Debug.Log(attacker.nickname + " did " + totalDamage + " damage");
                    Debug.Log(defender.nickname + " now has " + defender.currentHP + " HP remaining.");
                }
                else {
                    effective = "It was the least effective attack ever.";
                    totalDamage = 0;
                }
                textBox.text = attacker.nickname + " used " + attacker.attacks[moveIndex].attackName + " and " + effective + " dealing " + Mathf.Round(totalDamage) + " damage";
                
                // Update HP bars
                UpdateHPBars();
                break;

            case 2: // raise your attack stat
                attacker.attackMod += attacker.attacks[moveIndex].amount;
                Debug.Log(attacker.nickname + " gained " + attacker.attacks[moveIndex].amount + " attack");
                textBox.text = attacker.nickname + " used " + attacker.attacks[moveIndex].attackName + ". +" + attacker.attacks[moveIndex].amount + " attack!";
                break;

            case 3: // raise your defence
                attacker.defenseMod += attacker.attacks[moveIndex].amount;
                Debug.Log(attacker.nickname + " gained " + attacker.attacks[moveIndex].amount + " defense");
                textBox.text = attacker.nickname + " used " + attacker.attacks[moveIndex].attackName + ". +" + attacker.attacks[moveIndex].amount + " defense!";
                break;

            case 4: // lower their attack
                defender.attackMod -= attacker.attacks[moveIndex].amount;
                Debug.Log(defender.nickname + " lost " + attacker.attacks[moveIndex].amount + " attack");
                textBox.text = attacker.nickname + " used " + attacker.attacks[moveIndex].attackName + ". " + defender.nickname + " loses " + attacker.attacks[moveIndex].amount + " attack!";
                break;

            case 5: // lower their defense
                defender.defenseMod -= attacker.attacks[moveIndex].amount;
                Debug.Log(defender.nickname + " lost " + attacker.attacks[moveIndex].amount + " defense");
                textBox.text = attacker.nickname + " used " + attacker.attacks[moveIndex].attackName + ". " + defender.nickname + " loses " + attacker.attacks[moveIndex].amount + " defense!";
                break;

            default: // Fallback
                attacker.defenseMod += attacker.attacks[moveIndex].amount;
                Debug.LogError("Unknown move type: " + attacker.attacks[moveIndex].type);
                break;
        } //end of attack type switch   

        textBox.text += "\n\nPress [Enter] to continue...";
        textBox.gameObject.SetActive(true);
    }
    
    private void UpdateHPBars()
    {
        // Update the battle manager's HP bars
        battleManager.UpdatePokemonHP(playerPokemon, enemyPokemon);
    }
}