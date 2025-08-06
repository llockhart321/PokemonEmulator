using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TrainerProgressTracker : MonoBehaviour
{
    // Singleton pattern
    public static TrainerProgressTracker Instance { get; private set; }

    [Header("Required Trainer Settings")]
    [SerializeField] private string[] requiredTrainerNames;
    [SerializeField] private string finalBossName = "Final Boss";
    [SerializeField] private bool hasAlreadyNotified = false;

    [Header("UI References")]
    [SerializeField] private GameObject notificationPanel;
    [SerializeField] private TextMeshProUGUI notificationText;
    [SerializeField] private Button closeButton;
    [SerializeField] private float autoCloseTime = 5f;
    [SerializeField] private string completionMessage = "You have defeated all trainers. The final boss awaits!";

    private void Awake()
    {
        // Set up singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // Try to load previous notification state
        hasAlreadyNotified = PlayerPrefs.GetInt("FinalBossNotified", 0) == 1;
    }

    private void Start()
    {
        // Set up the notification panel if not already set
        if (notificationPanel == null)
        {
            // Try to find the notification panel in the scene
            notificationPanel = GameObject.FindGameObjectWithTag("NotificationPanel");
        }

        if (notificationPanel != null)
        {
            // Make sure it's hidden at start
            notificationPanel.SetActive(false);

            // Set up close button
            if (closeButton == null)
            {
                closeButton = notificationPanel.GetComponentInChildren<Button>();
            }

            if (closeButton != null)
            {
                closeButton.onClick.AddListener(CloseNotification);
            }
        }

        // Start checking for trainer completion periodically
        StartCoroutine(CheckTrainerProgressRoutine());
    }

    // Check trainer progress every few seconds
    private IEnumerator CheckTrainerProgressRoutine()
    {
        // Initial delay to ensure everything is loaded
        yield return new WaitForSeconds(2f);

        while (true)
        {
            CheckTrainerProgress();
            // Check again after a few seconds
            yield return new WaitForSeconds(5f);
        }
    }

    // Check if all required trainers have been defeated
    public void CheckTrainerProgress()
    {
        // Skip if we've already notified the player
        if (hasAlreadyNotified)
            return;

        // Skip if BattleSystemManager isn't available
        if (BattleSystemManager.Instance == null)
            return;

        // Skip if we're in a battle
        if (BattleSystemManager.Instance.IsBattleInProgress())
            return;

        // Check if all trainers are defeated
        if (AreAllTrainersDefeated())
        {
            ShowFinalBossNotification();
        }
    }

    // Check if all required trainers are defeated
    public bool AreAllTrainersDefeated()
    {
        if (BattleSystemManager.Instance == null)
        {
            Debug.LogError("BattleSystemManager.Instance is null!");
            return false;
        }

        if (requiredTrainerNames == null || requiredTrainerNames.Length == 0)
        {
            Debug.LogWarning("No required trainers specified in TrainerProgressTracker!");
            return false;
        }

        foreach (string trainerName in requiredTrainerNames)
        {
            if (!BattleSystemManager.Instance.IsTrainerDefeated(trainerName))
            {
                return false;
            }
        }

        return true;
    }

    // Show notification that the final boss is now available
    public void ShowFinalBossNotification()
    {
        Debug.Log("All trainers defeated! Final boss now available!");

        // Mark that we've notified the player
        hasAlreadyNotified = true;
        PlayerPrefs.SetInt("FinalBossNotified", 1);
        PlayerPrefs.Save();

        // Show UI notification if available
        if (notificationPanel != null && notificationText != null)
        {
            notificationText.text = completionMessage;
            notificationPanel.SetActive(true);

            // Auto-close after delay
            StartCoroutine(AutoCloseNotification());
        }
        else
        {
            Debug.LogWarning("Notification UI not set up properly!");
        }
    }

    // Auto-close the notification after a delay
    private IEnumerator AutoCloseNotification()
    {
        yield return new WaitForSeconds(autoCloseTime);
        CloseNotification();
    }

    // Close the notification panel
    public void CloseNotification()
    {
        if (notificationPanel != null)
        {
            notificationPanel.SetActive(false);
        }
    }

    // Get current trainer progress (useful for UI)
    public string GetTrainerProgressText()
    {
        if (BattleSystemManager.Instance == null || requiredTrainerNames == null)
            return "0/0";

        int defeated = 0;
        foreach (string trainer in requiredTrainerNames)
        {
            if (BattleSystemManager.Instance.IsTrainerDefeated(trainer))
                defeated++;
        }

        return $"{defeated}/{requiredTrainerNames.Length}";
    }

    // Reset progress tracking (for debugging or new game)
    public void ResetProgress()
    {
        hasAlreadyNotified = false;
        PlayerPrefs.SetInt("FinalBossNotified", 0);
        PlayerPrefs.Save();
    }
    public string GetFinalBossName()
    {
        return finalBossName;
    }
}