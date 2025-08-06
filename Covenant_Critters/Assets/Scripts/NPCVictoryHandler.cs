using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class NPCVictoryHandler : MonoBehaviour
{
    [SerializeField] private string npcName;
    [SerializeField] private GameObject victoryPanel;
    [SerializeField] private string startingSceneName = "StartMenuScene"; // Change to your title screen
    private bool hasShownVictoryPanel = false;

    void Start()
    {
        // Make sure victory panel is hidden at start
        if (victoryPanel != null)
        {
            victoryPanel.SetActive(false);
        }
        else
        {
            // Try to find the victory panel by tag
            victoryPanel = GameObject.FindGameObjectWithTag("VictoryPanel");
            if (victoryPanel != null)
            {
                victoryPanel.SetActive(false);
            }
        }

    }

    void Update()
    {
        // Only check if we haven't shown the panel yet
        if (!hasShownVictoryPanel)
        {
            CheckIfDefeated();
        }
    }

    void CheckIfDefeated()
    {
        // Skip if BattleSystemManager isn't available
        if (BattleSystemManager.Instance == null)
            return;

        // Skip if we're in a battle
        if (BattleSystemManager.Instance.IsBattleInProgress())
            return;

        // Check if this specific NPC has been defeated
        if (BattleSystemManager.Instance.IsTrainerDefeated(npcName))
        {
            ShowVictoryPanel();
        }
    }

    void ShowVictoryPanel()
    {
        if (victoryPanel != null)
        {
            // Show the victory panel
            victoryPanel.SetActive(true);
            hasShownVictoryPanel = true;
        }
        else
        {
            Debug.LogWarning("Victory panel not assigned or found!");
        }
    }

    
}