using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public abstract class BattleTriggerBase : MonoBehaviour
{
    [Header("Battle Settings")]
    [SerializeField] protected string battleSceneName = "BattleScene";
    [SerializeField] protected LayerMask playerLayer;
    [SerializeField] protected float encounterCooldown = 3f;

    // Static property to pass battle data between scenes
    public static BattleData PendingBattleData { get; protected set; }
    
    protected bool canTriggerBattle = true;
    protected float lastBattleTime;

    protected virtual void Start()
    {
        // Initialize the time to ensure cooldown works correctly
        lastBattleTime = -encounterCooldown;
    }

    protected virtual void OnTriggerEnter2D(Collider2D collision)
    {
        // Check if it's the player and if we can start a battle
        if (((1 << collision.gameObject.layer) & playerLayer) != 0 && canTriggerBattle && Time.time >= lastBattleTime + encounterCooldown)
        {
            HandleBattleTrigger();
        }
    }
    
    // Abstract method to be implemented by derived classes
    protected abstract void HandleBattleTrigger();
    
    // Method to start the battle
    protected void StartBattle(BattleData battleData)
    {
        // Store the battle data so it can be accessed in the battle scene
        PendingBattleData = battleData;
        
        // Record the time to prevent immediate re-triggering
        lastBattleTime = Time.time;
        
        // Load the battle scene
        SceneManager.LoadScene(battleSceneName);
    }
}
