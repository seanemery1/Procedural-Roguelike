using UnityEngine;
using UnityEngine.Tilemaps;

public class TileTest : MonoBehaviour
{
    void Start()
    {
        Tilemap tilemap = GetComponent<Tilemap>();

        BoundsInt bounds = tilemap.cellBounds;
        TileBase[] allTiles = tilemap.GetTilesBlock(bounds);

        for (int x = 0; x < bounds.size.x; x++)
        {
            for (int y = 0; y < bounds.size.y; y++)
            {
                TileBase tile = allTiles[x + y * bounds.size.x];
                switch (tile.name)
                {

                    case "dungeonTileMap_19":
                        Debug.Log("woo");
                        break;

                    case "two":

                        break;

                    default:

                        break;
                }
            }
        }
    }
}
