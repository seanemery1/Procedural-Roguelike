using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using UnityEngine;
using UnityEngine.Tilemaps;

public enum DoorType
{
    key,
    enemy,
    button
}
public class Door : MonoBehaviour
{
    [Header("Door Variables")]
    public bool playerTouching = false;
    public DoorType thisDoorType;
    public bool open = false;
    public Collider2D trigger;
    public Tilemap grid;
    public PlayerStats stats;
    //public Inventory playerInventory;

    private void Awake()
    {
        
        trigger = GetComponent<Collider2D>();
        
    }
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.tag == "Player")
        {
            stats = other.GetComponent<PlayerStats>();
            grid = GetComponentInParent<Tilemap>();
            playerTouching = true;
        }
        
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.tag == "Player")
        {
            stats = null;
            grid = null;
            playerTouching = false;
        }
    }
    private void Update()
    {
        if (Input.GetButtonDown("Jump"))
        {
            if (playerTouching)
            {
                Vector3Int lPos = grid.WorldToCell(trigger.transform.position);
                if (grid.HasTile(lPos))
                {
                    if (stats.useKeyOnDoor())
                    {
                        grid.SetTile(lPos, null);
                    }
                    
                    // Does the player have a key
                    // if so, then call the open method
                }
            }
        }
    }
    // Start is called before the first frame update
    public void Open()
    {
        // Delete tile
        // set open to true
    }
}
