using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class GameBoard : MonoBehaviour
{
    private Grid grid;
    public Tile buildTile;
    private Dictionary<Vector3Int, TileType> tileTypes;

    public enum TileType
    {
        None,
        Ground,
        Air,
    }
    
    void Start()
    {
        grid = GetComponent<Grid>();
        tileTypes = new Dictionary<Vector3Int, TileType>();
    }

    // Update is called once per frame
    void Update()
    {

        
    }

    private void FixedUpdate()
    {
    }
    


    public void OnClick(TileType tileType, Tilemap tilemap, Vector3Int pos)
    {
        
        switch (tileType)
        {
            case TileType.Ground:
            {
                Debug.Log($"ground x:{pos.x} y:{pos.y}");
                break;
            } 
            case TileType.Air:
            {
                Debug.Log($"air x:{pos.x} y:{pos.y}");
                tilemap.SetTile(pos, buildTile);
                break;
            }
        }
    }
    
}
