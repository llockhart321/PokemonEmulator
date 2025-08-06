using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class MapController : MonoBehaviour
{
    // Reference to your map canvas
    public Canvas mapCanvas;
    
    // Reference to map background image
    public Image mapBackground;
    
    // Reference to player marker on the map
    public GameObject playerMarker;
    
    // Reference to your actual player GameObject
    public GameObject player;

    // List to store all building markers
    private List<GameObject> buildingMarkers = new List<GameObject>();

    // Reference to the map canvas transform
    public Transform mapCanvasTransform;
    
    // Scale factor (how much to scale the world coordinates to fit the map)
    public float mapScale = 0.1f;
    
    void Start()
    {
        // Make sure map is off at start
        mapCanvas.gameObject.SetActive(false);
        // Create markers for all buildings
        CreateBuildingMarkers();
    }
    
    void Update()
    {
        // Toggle map on/off with M key
        if (Input.GetKeyDown(KeyCode.M))
        {
            mapCanvas.gameObject.SetActive(!mapCanvas.gameObject.activeSelf);
        }
        
        // If map is visible, update all map markers
        if (mapCanvas.gameObject.activeSelf)
        {
            UpdatePlayerMarker();
            UpdateBuildingMarkers();
        }
    }

    void FixedUpdate(){

        UpdatePlayerMarker();
    }

    void CreateBuildingMarkers()
    {
        // Find all BuildingMarker components in the scene
        BuildingMarker[] buildings = FindObjectsOfType<BuildingMarker>();
        
        foreach (BuildingMarker building in buildings)
        {
            if (building.showOnMap)
            {
                // Create a UI Image for this building
                GameObject marker = new GameObject(building.buildingName + " Marker");
                marker.transform.SetParent(mapCanvasTransform, false);
                
                // Add Image component
                Image markerImage = marker.AddComponent<Image>();
                markerImage.sprite = building.mapSprite;
                
                // Size the marker appropriately
                RectTransform rt = marker.GetComponent<RectTransform>();
                rt.sizeDelta = new Vector2(30, 30); // Adjust size as needed
                
                // Store reference to update later
                buildingMarkers.Add(marker);
                
                // Store reference to the building
                marker.AddComponent<MapMarkerLink>().worldObject = building.transform;
            }
        }
    }

    // Add this method to update all building markers
    void UpdateBuildingMarkers()
    {
        foreach (GameObject marker in buildingMarkers)
        {
            MapMarkerLink link = marker.GetComponent<MapMarkerLink>();
            if (link != null)
            {
                // Calculate position of building marker on map
                Vector2 buildingMapPos = new Vector2(
                    link.worldObject.position.x * mapScale,
                    link.worldObject.position.y * mapScale
                );
                
                marker.GetComponent<RectTransform>().anchoredPosition = buildingMapPos;
            }
        }
    }
        
        void UpdatePlayerMarker()
        {
            // Calculate position of player marker on map
            // This depends on your UI setup, but generally:
            Vector2 playerMapPos = new Vector2(
                player.transform.position.x * mapScale,
                player.transform.position.y * mapScale
            );
            
            playerMarker.GetComponent<RectTransform>().anchoredPosition = playerMapPos;
        }
}