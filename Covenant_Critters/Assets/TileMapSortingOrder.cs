using UnityEngine;
using UnityEngine.Tilemaps; // Needed for TilemapRenderer

public class TileMapSortingOrder : MonoBehaviour
{
    private TilemapRenderer tileMapRenderer;


    void Start()
    {
        tileMapRenderer = GetComponent<TilemapRenderer>();
    }

    void Update()
    {
        // Invert Y position to determine sorting order
        tileMapRenderer.sortingOrder = Mathf.RoundToInt(-transform.position.y * 100);
    }
}
