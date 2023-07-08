using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Tilemaps;


public class GameBoard : MonoBehaviour
{

    public Tile buildTile;
    public Tilemap gameTilemap;
    public Tilemap buildTilemap;
    public GameObject[] entityPrefabs;
    
    private Grid grid;
    private TileType[,] tileTypes;
    private Camera cam;
    private int tickCount = 0;
    private bool isSimulating = false;
    private float tickWaitTime = 0.5f;
    private float timeSinceTick = 0;
    
    private Dictionary<Vector3Int, Entity> entities;
    

    private const int LEFT_CLICK = 0;
    private const int RIGHT_CLICK = 1;

    public enum TileType
    {
        Ground,
        BuildSpace,
        NoBuildSpace,
        StartButton,
        AdventurerStart,
        OutOfBounds,
    }
    
    void Start()
    {
        grid = GetComponent<Grid>();
        cam = Camera.main;
        tileTypes = new TileType[gameTilemap.cellBounds.size.x, gameTilemap.cellBounds.size.y];

        foreach (var pos in gameTilemap.cellBounds.allPositionsWithin)
        {
            var worldPos = gameTilemap.CellToWorld(pos);
            var tile = gameTilemap.GetTile(new Vector3Int((int)worldPos.x, (int)worldPos.y));
            // var tile = gameTilemap.GetTile(pos);
            var tileTypesIndices = pos - gameTilemap.cellBounds.position;

            if (tile == null || tile.name == "NoBuildSpace")
            {
                tileTypes[tileTypesIndices.x, tileTypesIndices.y] = TileType.NoBuildSpace;
            }
            else if (tile.name == "Ground")
            {
                tileTypes[tileTypesIndices.x, tileTypesIndices.y] = TileType.Ground;
            }
            else if (tile.name == "BuildSpace")
            {
                tileTypes[tileTypesIndices.x, tileTypesIndices.y] = TileType.BuildSpace;
            }
            else if (tile.name == "AdventurerStart")
            {
                tileTypes[tileTypesIndices.x, tileTypesIndices.y] = TileType.AdventurerStart;
            }
            else if (tile.name == "StartButton")
            {
                tileTypes[tileTypesIndices.x, tileTypesIndices.y] = TileType.StartButton;
            }
        }
        
        // remove this when start button works!!
        entities = new Dictionary<Vector3Int, Entity>();
        InitEntities();
        
    }

    TileType GetTileType(Vector3Int pos)
    {
        var indices = pos - gameTilemap.cellBounds.position;
        if (0 <= indices.x && indices.x < tileTypes.GetLength(0) && 
            0 <= indices.y && indices.y < tileTypes.GetLength(1))
        {
            return tileTypes[indices.x, indices.y];
        }
        return TileType.OutOfBounds;
        
    }

    bool IsSolid(Vector3Int pos)
    {
        var tileType = GetTileType(pos);

        if (tileType == TileType.Ground)
        {
            return true;
        }

        return tileType == TileType.BuildSpace && buildTilemap.HasTile(pos);
    }


    void Update()
    {
        var mousePos = cam.ScreenToWorldPoint(Input.mousePosition);
        var pos = grid.WorldToCell(mousePos);
        var tileType = GetTileType(pos);

        if (tileType == TileType.OutOfBounds)
        {
            return;
        }

        if (Input.GetMouseButtonDown(LEFT_CLICK))
        {
            if (tileType == TileType.BuildSpace)
            {
                if (!buildTilemap.HasTile(pos))
                {
                    buildTilemap.SetTile(pos, buildTile);
                }
            }
        }
        else if (Input.GetMouseButtonDown(RIGHT_CLICK))
        {
            if (tileType == TileType.BuildSpace)
            {
                if (buildTilemap.HasTile(pos))
                {
                    buildTilemap.SetTile(pos, null);
                }
            }
        }

        timeSinceTick += Time.deltaTime;
        
        if (isSimulating && tickWaitTime < timeSinceTick)
        {
            timeSinceTick -= tickWaitTime;
            tickCount++;
            Simulate();
        }
    }

    // this wont instantiate anything if there is not prefab added with the same entity type.
    void InstantiateEntity(Entity.EntityType entityType, Vector3Int pos)
    {
        foreach (var prefab in entityPrefabs)
        {
            if (prefab.GetComponent<Entity>().entityType == entityType)
            {
                var go = Instantiate(prefab, new Vector3(pos.x, pos.y, 0), Quaternion.identity, grid.transform);

                entities[pos] = go.GetComponent<Entity>();
                // return entities[pos];
            }
        }

        // return null;
    }

    void InitEntities()
    {
        for (int x = 0; x < tileTypes.GetLength(0); x++)
        {
            for (int y = 0; y < tileTypes.GetLength(1); y++)
            {
                if (tileTypes[x, y] == TileType.AdventurerStart)
                {
                    Debug.Log("instantiate");
                    InstantiateEntity(Entity.EntityType.Adventurer, new Vector3Int(x, y));
                }
            }
        }
    }

    void StartSimulation()
    {
        isSimulating = true;
        timeSinceTick = 0;
    }

    void EndSimulation()
    {
        isSimulating = false;
    }

    void Simulate()
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
            case TileType.BuildSpace:
            {
                Debug.Log($"air x:{pos.x} y:{pos.y}");
                tilemap.SetTile(pos, buildTile);
                break;
            }
        }
    }
    
}
