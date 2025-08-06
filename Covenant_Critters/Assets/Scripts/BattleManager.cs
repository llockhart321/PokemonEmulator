using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BattleManager : MonoBehaviour
{
    [Header("Player Pokemon UI")]
    [SerializeField] private Image ourPokemonImage;
    [SerializeField] private TextMeshProUGUI ourPokemonNameText;
    [SerializeField] private TextMeshProUGUI ourPokemonLevelText;
    [SerializeField] private Image ourPokemonHPBar;
    [SerializeField] private TextMeshProUGUI ourPokemonHPText;

    [Header("Enemy Pokemon UI")]
    [SerializeField] private Image enemyPokemonImage;
    [SerializeField] private TextMeshProUGUI enemyPokemonNameText;
    [SerializeField] private TextMeshProUGUI enemyPokemonLevelText;
    [SerializeField] private Image enemyPokemonHPBar;
    [SerializeField] private TextMeshProUGUI enemyPokemonHPText;
    [SerializeField] private Image trainerImage; // For trainer battles

    [Header("UI Elements")]
    [SerializeField] private TextMeshProUGUI dialogueText;
    [SerializeField] private GameObject actionSelector;
    [SerializeField] private TextMeshProUGUI runText;
    [SerializeField] private TextMeshProUGUI attackText;
    [SerializeField] private GameObject moveSelector;
    [SerializeField] private GameObject battlePanel;
    [SerializeField] private GameObject endBattlePanel; // Optional panel for end battle messages

    [Header("Default Sprites")]
    [SerializeField] private Sprite defaultPlayerSprite;
    [SerializeField] private Sprite defaultEnemySprite;
    [SerializeField] private Sprite defaultTrainerSprite;

    // Original width of the HP bars - set in Start()
    private float playerHPBarOriginalWidth;
    private float enemyHPBarOriginalWidth;

    // Battle data
    private PokemonInstance playerPokemon;
    private PokemonInstance enemyPokemon;
    private List<PokemonInstance> enemyTeam = new List<PokemonInstance>();
    private int currentEnemyIndex = 0;
    private bool isTrainerBattle;
    private string trainerName;
    private Sprite trainerSprite;
    private bool battleEnded = false;
    private BattleAction battleAction;
    private BattleSystemManager battleSystemManager;
    
    // Battle state tracking
    private enum BattleState { Start, ActionSelection, MoveSelection, PerformMove, EnemyMove, Busy, BattleOver }
    private BattleState currentState;
    
    // Battle result tracking
    private char battleResult = '\0'; // 'v' for victory, 'd' for defeat, 'r' for run

    void Start()
    {
        Debug.Log("BattleManager started.");
        battleEnded = false;
        currentState = BattleState.Start;
        
        // Get references
        battleAction = FindObjectOfType<BattleAction>();
        battleSystemManager = BattleSystemManager.Instance;
        
        if (battleSystemManager == null)
        {
            Debug.LogError("BattleSystemManager.Instance is null! Make sure it exists in the scene.");
        }
        // Store original HP bar widths
        if (ourPokemonHPBar != null)
            playerHPBarOriginalWidth = ourPokemonHPBar.rectTransform.sizeDelta.x;
            
        if (enemyPokemonHPBar != null)
            enemyHPBarOriginalWidth = enemyPokemonHPBar.rectTransform.sizeDelta.x;

        LoadBattleData();
        SetupBattleUI();
        //SetupActionButtons();
        
        // Start battle sequence
        StartCoroutine(BattleStartSequence());
    }

    private IEnumerator BattleStartSequence()
    {
        if (actionSelector != null)
            actionSelector.SetActive(false); // Hide Attack/Run buttons

        // Start battle animation/intro
        if (dialogueText != null)
        {
            string battleStartText = isTrainerBattle 
                ? $"Trainer {trainerName} wants to battle!"
                : $"A wild {enemyPokemon?.nickname ?? "Pokemon"} appeared!";
                
            dialogueText.text = battleStartText;
        }
        
        yield return new WaitForSeconds(1.5f);
        
        if (isTrainerBattle && dialogueText != null)
        {
            dialogueText.text = $"{trainerName} sent out {enemyPokemon.nickname}!";
            yield return new WaitForSeconds(1.5f);
        }
        
        // Show player pokemon
        if (dialogueText != null)
            dialogueText.text = $"Go, {playerPokemon.nickname}!";
            
        yield return new WaitForSeconds(1.5f);
        
        // Transition to action selection
        currentState = BattleState.ActionSelection;
        SetupActionButtons();
        ShowActionSelector();
    }

    private void LoadBattleData()
    {
        // Use the BattleSystemManager to get battle data
        if (battleSystemManager == null) return;
        
        BattleData battleData = battleSystemManager.GetCurrentBattleData();
        
        if (battleData == null)
        {
            Debug.LogError("No battle data found! Make sure a battle was properly triggered.");
            return;
        }

        // Load enemy pokemon team
        if (battleData.enemyPokemon != null && battleData.enemyPokemon.Count > 0)
        {
            enemyTeam = battleData.enemyPokemon;
            enemyPokemon = enemyTeam[0]; // Set first enemy pokemon
            enemyPokemon.currentHP = enemyPokemon.maxHP;
            currentEnemyIndex = 0;
            Debug.Log($"Enemy pokemon: {enemyPokemon.nickname} (Lvl {enemyPokemon.level}) with hp: {enemyPokemon.currentHP}");
        }
        else
        {
            Debug.LogError("No enemy pokemon in battle data!");
        }

        // Set trainer battle info
        isTrainerBattle = battleData.isTrainerBattle;
        trainerName = battleData.trainerName;
        trainerSprite = battleData.trainerSprite;
        
        // Get player's first Pokemon with HP > 0
        if (PokemonInventory.Instance != null && 
            PokemonInventory.Instance.ownedPokemon != null && 
            PokemonInventory.Instance.ownedPokemon.Count > 0)
        {
            // Find first pokemon with HP > 0
            foreach (var pokemon in PokemonInventory.Instance.ownedPokemon)
            {
                if (pokemon.currentHP > 0)
                {
                    playerPokemon = pokemon;
                    break;
                }
            }
            
            if (playerPokemon != null)
            {
                Debug.Log($"Player pokemon: {playerPokemon.nickname} (Lvl {playerPokemon.level})");
            }
            else
            {
                Debug.LogError("No player pokemon with HP > 0 found!");
                // Handle no available pokemon case
                EndBattle(false);
            }
            UpdatePokemonHP(playerPokemon, enemyPokemon);
        }
        else
        {
            Debug.LogError("No player pokemon found!");
        }
    }

    private void SetupBattleUI()
    {
        // Setup player pokemon UI
        if (playerPokemon != null)
        {
            if (ourPokemonNameText != null)
                ourPokemonNameText.text = playerPokemon.nickname;

            if (ourPokemonLevelText != null)
                ourPokemonLevelText.text = "Lv." + playerPokemon.level;

            if (ourPokemonHPBar != null)
            {
                // Update HP bar
                float hpRatio = (float)playerPokemon.currentHP / playerPokemon.maxHP;
                Vector2 sizeDelta = ourPokemonHPBar.rectTransform.sizeDelta;
                sizeDelta.x = playerHPBarOriginalWidth * hpRatio;
                ourPokemonHPBar.rectTransform.sizeDelta = sizeDelta;
            }

            if (ourPokemonHPText != null)
                ourPokemonHPText.text = $"{playerPokemon.currentHP}/{playerPokemon.maxHP}";

            if (ourPokemonImage != null)
                ourPokemonImage.sprite = playerPokemon.basePokemon.backSprite ?? defaultPlayerSprite;
        }

        // Setup enemy pokemon UI
        if (enemyPokemon != null)
        {
            if (enemyPokemonNameText != null)
                enemyPokemonNameText.text = enemyPokemon.nickname;

            if (enemyPokemonLevelText != null)
                enemyPokemonLevelText.text = "Lv." + enemyPokemon.level;

            if (enemyPokemonHPBar != null)
            {
                // Update HP bar
                float hpRatio = (float)enemyPokemon.currentHP / enemyPokemon.maxHP;
                Vector2 sizeDelta = enemyPokemonHPBar.rectTransform.sizeDelta;
                sizeDelta.x = enemyHPBarOriginalWidth * hpRatio;
                enemyPokemonHPBar.rectTransform.sizeDelta = sizeDelta;
            }

            if (enemyPokemonHPText != null)
                enemyPokemonHPText.text = $"{enemyPokemon.currentHP}/{enemyPokemon.maxHP}";

            if (enemyPokemonImage != null)
                enemyPokemonImage.sprite = enemyPokemon.basePokemon.frontSprite ?? defaultEnemySprite;
        }
        
        // Setup trainer sprite if it's a trainer battle
        if (isTrainerBattle && trainerImage != null)
        {
            trainerImage.gameObject.SetActive(true);
            trainerImage.sprite = trainerSprite ?? defaultTrainerSprite;
        }
        else if (trainerImage != null)
        {
            trainerImage.gameObject.SetActive(false);
        }
        UpdatePokemonHP(playerPokemon, enemyPokemon);
    }

    private void SetupActionButtons()
    {
        if (runText != null)
        {
            var runButton = runText.GetComponent<Button>();
            if (runButton == null)
            {
                runButton = runText.gameObject.AddComponent<Button>();
            }
            runButton.onClick.RemoveAllListeners();
            runButton.onClick.AddListener(OnRunClicked);
        }

        if (attackText != null)
        {
            var attackButton = attackText.GetComponent<Button>();
            if (attackButton == null)
            {
                attackButton = attackText.gameObject.AddComponent<Button>();
            }
            attackButton.onClick.RemoveAllListeners();
            attackButton.onClick.AddListener(OnAttackClicked);
        }
    }

    private void OnRunClicked()
    {
        Debug.Log("Run clicked!");
        
        if (currentState != BattleState.ActionSelection) return;
        
        currentState = BattleState.Busy;
        
        // Use the BattleAction's RunFromBattle method
        if (battleAction != null)
        {
            battleAction.RunFromBattle();
        }
        else
        {
            Debug.Log("CRITICALL ERROR");
        }
    }

    private void OnAttackClicked()
    {
        Debug.Log("Attack clicked!");
        
        if (currentState != BattleState.ActionSelection) return;
        
        currentState = BattleState.MoveSelection;

        if (actionSelector != null)
            actionSelector.SetActive(false); // Hide Attack/Run buttons

        // Start the attack action script with our battle action
        if (battleAction != null)
        {
            battleAction.StartAttack(playerPokemon, enemyPokemon);
        }
        else
        {
            Debug.LogError("BattleAction not found!");
            currentState = BattleState.ActionSelection;
        }
    }
    
    // Methods to expose functionality to BattleAction
    public void ShowActionSelector()
    {
        if (actionSelector != null)
            actionSelector.SetActive(true);
            
        if (dialogueText != null)
            dialogueText.text = "What will " + playerPokemon.nickname + " do?";
            
        currentState = BattleState.ActionSelection;
    }
    
    public bool IsTrainerBattle()
    {
        return isTrainerBattle;
    }
    
    public string GetTrainerName()
    {
        return trainerName;
    }
    
    // Method to update player's pokemon reference when switched
    public void UpdatePlayerPokemon(PokemonInstance newPokemon)
    {
        playerPokemon = newPokemon;
        
        // Update UI
        if (ourPokemonNameText != null)
            ourPokemonNameText.text = playerPokemon.nickname;

        if (ourPokemonLevelText != null)
            ourPokemonLevelText.text = "Lv." + playerPokemon.level;

        if (ourPokemonHPBar != null)
        {
            // Update HP bar 
            float hpRatio = (float)playerPokemon.currentHP / playerPokemon.maxHP;
            Vector2 sizeDelta = ourPokemonHPBar.rectTransform.sizeDelta;
            sizeDelta.x = playerHPBarOriginalWidth * hpRatio;
            ourPokemonHPBar.rectTransform.sizeDelta = sizeDelta;
        }

        if (ourPokemonHPText != null)
            ourPokemonHPText.text = $"{playerPokemon.currentHP}/{playerPokemon.maxHP}";

        if (ourPokemonImage != null)
            ourPokemonImage.sprite = playerPokemon.basePokemon.backSprite ?? defaultPlayerSprite;
    }
    
    // Method to update HP bars during battle
    public void UpdatePokemonHP(PokemonInstance playerPokemon, PokemonInstance enemyPokemon)
    {
        if (playerPokemon != null)
        {
            if (ourPokemonHPBar != null)
            {
                 //Update HP bar with animation
                StartCoroutine(AnimateHPBar(ourPokemonHPBar, 
                                          playerHPBarOriginalWidth,
                                          (float)playerPokemon.currentHP / playerPokemon.maxHP));
            }
            
            if (ourPokemonHPText != null)
                ourPokemonHPText.text = $"{Mathf.Max(0, (int)playerPokemon.currentHP)}/{playerPokemon.maxHP}";
        }
        
        if (enemyPokemon != null)
        {
            if (enemyPokemonHPBar != null)
            {
                // Update HP bar with animation
                StartCoroutine(AnimateHPBar(enemyPokemonHPBar, 
                                          enemyHPBarOriginalWidth,
                                          (float)enemyPokemon.currentHP / enemyPokemon.maxHP));
            }
            
            if (enemyPokemonHPText != null)
                enemyPokemonHPText.text = $"{Mathf.Max(0, (int)enemyPokemon.currentHP)}/{enemyPokemon.maxHP}";
        }
    }
    // Coroutine to animate HP bar moving from right to left
private IEnumerator AnimateHPBar(Image hpBar, float originalWidth, float targetPercentage)
{
    // Get the current percentage
    float currentPercentage = hpBar.rectTransform.sizeDelta.x / originalWidth;
    
    // If HP is decreasing
    if (targetPercentage < currentPercentage)
    {
        float animationDuration = 1.0f; // Adjust duration as needed
        float elapsedTime = 0f;
        
        RectTransform rectTransform = hpBar.rectTransform;
        Vector2 originalSize = rectTransform.sizeDelta;
        Vector2 originalPosition = rectTransform.anchoredPosition;
        
        // Calculate target size
        Vector2 targetSize = new Vector2(originalWidth * targetPercentage, originalSize.y);
        
        // Calculate how much the position needs to shift to maintain the right edge
        float positionOffset = (originalSize.x - targetSize.x) / 2f;
        Vector2 targetPosition = new Vector2(originalPosition.x + positionOffset, originalPosition.y);
        
        // Animate both size and position
        while (elapsedTime < animationDuration)
        {
            float t = elapsedTime / animationDuration;
            rectTransform.sizeDelta = Vector2.Lerp(originalSize, targetSize, t);
            
            // Only adjust position if the bar is anchored at the center
            // If your bar uses a different anchor, you'll need to adjust this logic
            rectTransform.anchoredPosition = Vector2.Lerp(originalPosition, targetPosition, t);
            
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        
        // Ensure the final values are set exactly
        rectTransform.sizeDelta = targetSize;
        rectTransform.anchoredPosition = targetPosition;
    }
    // If HP is increasing, you can implement a different animation if desired
    else if (targetPercentage > currentPercentage)
    {
        // Similar logic but reversed for HP increase
        // ... (implement if needed)
    }
}
    // Method to check if enemy team has any pokemon left
    public bool EnemyHasLivingPokemon()
    {
        if (enemyTeam == null || enemyTeam.Count == 0)
            return false;
            
        foreach (var pokemon in enemyTeam)
        {
            if (pokemon.currentHP > 0)
                return true;
        }
        
        return false;
    }
    
    
    // Method to switch to next enemy pokemon
    public PokemonInstance SwitchToNextEnemyPokemon()
    {
        if (!isTrainerBattle)
            return null; // Wild battles don't have multiple pokemon
            
        for (int i = currentEnemyIndex + 1; i < enemyTeam.Count; i++)
        {
            if (enemyTeam[i].currentHP > 0)
            {
                // Switch to this pokemon
                currentEnemyIndex = i;
                enemyPokemon = enemyTeam[i];
                Debug.Log("switching to enemy poke"+ enemyPokemon.nickname);
                
                // Update UI
                if (dialogueText != null)
                    dialogueText.text = $"{trainerName} sent out {enemyPokemon.nickname}!";

                    
                if (enemyPokemonNameText != null)
                    enemyPokemonNameText.text = enemyPokemon.nickname;

                if (enemyPokemonLevelText != null)
                    enemyPokemonLevelText.text = "Lv." + enemyPokemon.level;

                if (enemyPokemonHPBar != null)
                {
                    float hpRatio = (float)enemyPokemon.currentHP / enemyPokemon.maxHP;
                    Vector2 sizeDelta = enemyPokemonHPBar.rectTransform.sizeDelta;
                    sizeDelta.x = enemyHPBarOriginalWidth * hpRatio;
                    enemyPokemonHPBar.rectTransform.sizeDelta = sizeDelta;
                }

                if (enemyPokemonHPText != null)
                    enemyPokemonHPText.text = $"{enemyPokemon.currentHP}/{enemyPokemon.maxHP}";

                if (enemyPokemonImage != null)
                    enemyPokemonImage.sprite = enemyPokemon.basePokemon.frontSprite ?? defaultEnemySprite;
                    
                return enemyPokemon;
            }
        }
        
        return null; // No more living pokemon
    }
    

/*
    public IEnumerator SwitchToNextEnemyPokemonCoroutine(System.Action<PokemonInstance> callback)
{
    if (!isTrainerBattle)
    {
        callback(null);
        yield break;
    }

    for (int i = currentEnemyIndex + 1; i < enemyTeam.Count; i++)
    {
        if (enemyTeam[i].currentHP > 0)
        {
            currentEnemyIndex = i;
            enemyPokemon = enemyTeam[i];
            Debug.Log("switching to enemy poke" + enemyPokemon.nickname);

            // Update UI
            if (dialogueText != null)
                dialogueText.text = $"{trainerName} sent out {enemyPokemon.nickname}!";

            if (enemyPokemonNameText != null)
                enemyPokemonNameText.text = enemyPokemon.nickname;

            if (enemyPokemonLevelText != null)
                enemyPokemonLevelText.text = "Lv." + enemyPokemon.level;

            if (enemyPokemonHPBar != null)
            {
                enemyPokemonHPBar.maxValue = enemyPokemon.maxHP;
                enemyPokemonHPBar.value = enemyPokemon.currentHP;
            }

            if (enemyPokemonHPText != null)
                enemyPokemonHPText.text = $"{enemyPokemon.currentHP}/{enemyPokemon.maxHP}";

            if (enemyPokemonImage != null)
                enemyPokemonImage.sprite = enemyPokemon.basePokemon.frontSprite ?? defaultEnemySprite;

            // Wait for Enter key press
            yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.Return));

            // Callback to return the selected Pokémon after the wait
            callback(enemyPokemon);
            yield break;
        }
    }

    // No living Pokémon left
    callback(null);
}


*/







    
    // Helper methods for state transitions
    private IEnumerator ReturnToActionSelection(float delay)
    {
        yield return new WaitForSeconds(delay);
        currentState = BattleState.ActionSelection;
        ShowActionSelector();
    }
    
    private IEnumerator TransitionToEnemyMove(float delay)
    {
        yield return new WaitForSeconds(delay);
        currentState = BattleState.EnemyMove;
        
        // Enemy's turn - handle move selection and execution
        PerformEnemyTurn();
    }
    
    private void PerformEnemyTurn()
    {
        if (battleAction != null)
        {
            // Let BattleAction handle the enemy turn
            // It will eventually call back to this class when done
            currentState = BattleState.Busy;
            
            // Enemy logic continues in BattleAction class
            // After enemy move, BattleAction will check game state and either:
            // 1. Return to action selection if battle continues
            // 2. End battle if it's over
        }
        else
        {
            // Fallback simple enemy logic if BattleAction not found
            // Select a random move
            int moveIndex = Random.Range(0, enemyPokemon.attacks.Count);
            
            // Apply damage or effects based on move type
            ApplyEnemyMove(moveIndex);
            
            // Check game state after enemy move
            bool battleContinues = CheckGameState();
            
            if (battleContinues)
            {
                StartCoroutine(ReturnToActionSelection(1.5f));
            }
            else
            {
                EndBattle(battleResult == 'v'); // Player wins if result is 'v'
            }
        }
    }
    
    // Fallback enemy move implementation if BattleAction isn't available
    private void ApplyEnemyMove(int moveIndex)
    {
        if (enemyPokemon.attacks == null || enemyPokemon.attacks.Count == 0 || moveIndex >= enemyPokemon.attacks.Count)
        {
            if (dialogueText != null)
                dialogueText.text = $"{enemyPokemon.nickname} has no moves!";
            return;
        }
        
        Pokemon.Attack attack = enemyPokemon.attacks[moveIndex];
        
        if (dialogueText != null)
            dialogueText.text = $"Enemy {enemyPokemon.nickname} used {attack.attackName}!";
            
        // Simple implementation for damage calculation
        switch (attack.type)
        {
            case 1: // Standard damage
                float damageAmount = attack.amount + enemyPokemon.attack + enemyPokemon.attackMod;
                damageAmount -= (playerPokemon.defense + playerPokemon.defenseMod);
                
                if (damageAmount <= 0)
                    damageAmount = 1; // Minimum 1 damage
                    
                playerPokemon.currentHP -= damageAmount;
                
                if (dialogueText != null)
                    dialogueText.text += $"\nIt dealt {Mathf.Round(damageAmount)} damage!";
                break;
                
            case 2: // Raise attack
                enemyPokemon.attackMod += attack.amount;
                if (dialogueText != null)
                    dialogueText.text += $"\nIts Attack rose!";
                break;
                
            case 3: // Raise defense
                enemyPokemon.defenseMod += attack.amount;
                if (dialogueText != null)
                    dialogueText.text += $"\nIts Defense rose!";
                break;
                
            case 4: // Lower player attack
                playerPokemon.attackMod -= attack.amount;
                if (dialogueText != null)
                    dialogueText.text += $"\n{playerPokemon.nickname}'s Attack fell!";
                break;
                
            case 5: // Lower player defense
                playerPokemon.defenseMod -= attack.amount;
                if (dialogueText != null)
                    dialogueText.text += $"\n{playerPokemon.nickname}'s Defense fell!";
                break;
        }
        
        // Update HP bars
        UpdatePokemonHP(playerPokemon, enemyPokemon);
    }
    
    // Check if battle should continue or end
    private bool CheckGameState()
    {
        // Check if player's current Pokémon is defeated
        if (playerPokemon.currentHP <= 0)
        {
            playerPokemon.currentHP = 0; // Ensure it doesn't go negative
            UpdatePokemonHP(playerPokemon, enemyPokemon);
            
            // Check if player has any pokemon left
            bool hasLivingPokemon = false;
            
            if (PokemonInventory.Instance != null)
            {
                foreach (var pokemon in PokemonInventory.Instance.ownedPokemon)
                {
                    if (pokemon.currentHP > 0 && pokemon != playerPokemon)
                    {
                        hasLivingPokemon = true;
                        break;
                    }
                }
            }
            
            if (!hasLivingPokemon)
            {
                // Player has lost
                battleResult = 'd';
                return false;
            }
            else
            {
                // Player needs to switch Pokemon
                // This would be handled by BattleAction but here's a placeholder
                if (dialogueText != null)
                    dialogueText.text = $"{playerPokemon.nickname} fainted!";
                    
                // Force switch to next available Pokemon
                SwitchToNextLivingPokemon();
                return true;
            }
        }
        
        // Check if enemy's current Pokémon is defeated
        if (enemyPokemon.currentHP <= 0)
        {
            enemyPokemon.currentHP = 0; // Ensure it doesn't go negative
            UpdatePokemonHP(playerPokemon, enemyPokemon);
            
            if (dialogueText != null)
                dialogueText.text = $"Enemy {enemyPokemon.nickname} fainted!";
                
                /*
            // Check if trainer has more pokemon
            if (isTrainerBattle && SwitchToNextEnemyPokemon())
            {
                // Trainer switched to next pokemon
                return true;
            }
            else
            {
                // Enemy has no more pokemon - player wins
                battleResult = 'v';
                return false;
            }
            */
        }
        
        // Battle continues
        return true;
    }
    
    // Switch to next living player pokemon
    private void SwitchToNextLivingPokemon()
    {
        if (PokemonInventory.Instance == null) return;
        
        foreach (var pokemon in PokemonInventory.Instance.ownedPokemon)
        {
            if (pokemon.currentHP > 0 && pokemon != playerPokemon)
            {
                // Switch to this pokemon
                UpdatePlayerPokemon(pokemon);
                
                if (dialogueText != null)
                    dialogueText.text = $"Go, {playerPokemon.nickname}!";
                    
                break;
            }
        }
    }

    



    // End battle with a win/lose status
public void EndBattle(bool playerWon = true)
{
    if (battleEnded) return; // Prevent multiple calls
    battleEnded = true;
    currentState = BattleState.BattleOver;
    
    Debug.Log($"Battle ended with player {(playerWon ? "winning" : "losing")}");
    
    // Don't automatically start the exit coroutine - let BattleAction control the flow
    // Remove or comment out this line:
    // StartCoroutine(EndBattleAfterDelay(3.0f, playerWon));
}

// Create a new method that can be called by BattleAction when ready
public void FinalizeBattle()
{
    Debug.Log("Finalizing battle after player pressed Enter");
    StartCoroutine(EndBattleAfterDelay(0.5f, true));
}



    
    private void HandleVictory()
    {
        Debug.Log("victory called in battle manager");





        /*
        if (isTrainerBattle)
        {
            // Get trainer name
            if (dialogueText != null)
                dialogueText.text = "You won the battle against " + trainerName + "!";
            
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
            if (dialogueText != null)
                dialogueText.text = "You defeated the wild " + enemyPokemon.nickname + "!";
            
            // Award experience points to player Pokémon
            AwardExperiencePoints();
            
            // Chance to capture the wild Pokémon (show capture dialog)
            if (battleSystemManager != null)
            {
                // In a real implementation, you would show a capture dialog here
                // For now we're auto-capturing
                if (dialogueText != null)
                    dialogueText.text += "\nYou captured the wild " + enemyPokemon.nickname + "!";
                    
                battleSystemManager.CaptureWildPokemon();
            }
        }
        */
    }
    
    private void HandleDefeat()
    {
        /*
        This should be getting done in battle action
        if (dialogueText != null)
            dialogueText.text = "You were defeated!";
        
        // Heal player's Pokémon
        HealPlayerPokemon();
        */
    }
    
    private void HandleRunAway()
    {
        /*
        This should be getting done in battle action

        if (dialogueText != null)
            dialogueText.text = "Got away safely!";
        */
    }
    
    /*
    not being used anymore here
    // Award experience points to player Pokémon after victory
    private void AwardExperiencePoints()
    {
        if (playerPokemon != null)
        {
            // Simple formula: enemy level * multiplier
            int expGained = enemyPokemon.level * 5;
            playerPokemon.experience += expGained;
            
            // Check for level up
            int oldLevel = playerPokemon.level;
            playerPokemon.CheckLevelUp();
            
            if (dialogueText != null)
            {
                dialogueText.text += "\n" + playerPokemon.nickname + " gained " + expGained + " experience points!";
                
                if (playerPokemon.level > oldLevel)
                {
                    dialogueText.text += "\n" + playerPokemon.nickname + " grew to level " + playerPokemon.level + "!";
                }
            }
        }
    }
    
    
    private void HealPlayerPokemon()
    {
        // Heal all player Pokémon to full health after defeat
        if (PokemonInventory.Instance != null)
        {
            foreach (var pokemon in PokemonInventory.Instance.ownedPokemon)
            {
                pokemon.currentHP = pokemon.maxHP;
                pokemon.attackMod = 0;
                pokemon.defenseMod = 0;
            }
            
            Debug.Log("All player Pokémon healed after defeat");
        }
    }
    */
    
    private IEnumerator EndBattleAfterDelay(float delay, bool playerWon)
    {
        yield return new WaitForSeconds(0.5f);
        

        // Clean up battle data
        if (battleSystemManager != null)
        {
            battleSystemManager.ClearBattleData();
        }
        
        // Use GameManager to return to previous scene
        if (GameManager.Instance != null)
        {
            GameManager.Instance.ReturnFromBattle();
        }
    }
}