
using UnityEngine;

public class BuildingMarker : MonoBehaviour
{
    // The sprite to show on the map for this building
    public Sprite mapSprite;
    
    // The name of this building (optional)
    public string buildingName;
    
    // Is this building visible on the map?
    public bool showOnMap = true;
}