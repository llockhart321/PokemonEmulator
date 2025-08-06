using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class GridTransition : MonoBehaviour
{
    public Transform player;               // Drag your Player GameObject here
    public float transitionX = 10f;        // The X coordinate where transition happens (right edge of Grid1)
    public Vector3 newPlayerPosition;      // Where to place the player in Grid2
    public GameObject grid1;               // Reference to Grid1
    public GameObject grid2;               // Reference to Grid2
    public GameObject sailingAnimation; // Reference to your sailing animation GameObject
    public Camera mainCamera;

    private Vector3 originalCameraPosition;
    private Vector3 animationCameraPosition = new Vector3(120f, -50f, -10f); // Animation viewing position
    


    private void Start()
    {
        // Find and initialize references
        //sailingAnimation = GameObject.Find("SailingAnimation"); // Replace with your actual GameObject name
        //mainCamera = Camera.main;
        
        // Set animation object inactive at start
        
    }

    private void OnTriggerEnter2D(Collider2D collision){
        StartCoroutine(PlaySailingTransition());
    }

    private IEnumerator PlaySailingTransition()
    {
        // Store original camera position
        if (mainCamera == null) mainCamera = Camera.main;

        originalCameraPosition = mainCamera.transform.position;
        
        // 1. Show sailing animation
        sailingAnimation.SetActive(true);
        
        // 2. Move camera to view the animation
        mainCamera.transform.position = animationCameraPosition;
        //player.position = new Vector3(120f, -50f, -10f);
        CameraFollow.Instance.setTransition(true);
        
        // 3. Wait for animation to play (3 seconds)
        yield return new WaitForSeconds(3f);
        
        // 4. Move player to new location
        player.position = newPlayerPosition;
        
        // 5. Switch grids
        grid1.SetActive(false);  // Hide Grid1
        grid2.SetActive(true);   // Show Grid2
        
        // 6. Move camera back to follow player
        mainCamera.transform.position = new Vector3(player.position.x, player.position.y, originalCameraPosition.z);
        
        // 7. Hide animation
        sailingAnimation.SetActive(false);
        CameraFollow.Instance.setTransition(false);
    }

    public void AssignCamera(Camera newCam)
    {
        //cam = newCam;
    }



}