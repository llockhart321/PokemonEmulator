using UnityEngine;
using System.Collections;

public class CameraFollow : MonoBehaviour
{
    // Singleton instance
    public static CameraFollow Instance { get; private set; }
    public Transform player;        // Reference to the player's transform
    public float smoothing = 0.125f; // Smoothing factor
    public Vector3 offset;          // Offset from the player (e.g., camera distance)
    private bool transitioning = false;

    private void Awake()
    {
        // Set up singleton
        if (Instance == null)
        {
            Instance = this;
            //DontDestroyOnLoad(gameObject);

        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }
    }

    void LateUpdate()
    {   
        if(!transitioning)
        {
            // Calculate the desired position
            Vector3 desiredPosition = player.position + offset;
            // Smoothly interpolate between the current position and the desired position
            Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothing);
            // Set the camera's position
            transform.position = smoothedPosition;
        }
    }

    public void setTransition(bool newVar){
        transitioning = newVar;
    }

    // New method to move camera to a specific position when transitioning
    public void MoveCameraToPosition(Vector3 position)
    {
        
            transform.position = position;
        
    }
}