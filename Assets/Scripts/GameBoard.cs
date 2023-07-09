using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Tilemaps;


public class GameBoard : MonoBehaviour
{
    public static float tickWaitTime = 0.1f;

    public Tile buildTile;
    public Tilemap gameTilemap;
    public Tilemap buildTilemap;
    public GameObject[] entityPrefabs;
    
    private Grid grid;
    private TileType[,] tileTypes;
    private Camera cam;
    private int tickCount = 0;
    private bool isSimulating = false;
    private float timeSinceTick = 0;
    
    private List<Entity> entities;
    

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
        Lava,
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

            if (tile == null || tile.name == "BuildSpace")
            {
                tileTypes[tileTypesIndices.x, tileTypesIndices.y] = TileType.BuildSpace;
            }
            else if (tile.name == "Ground")
            {
                tileTypes[tileTypesIndices.x, tileTypesIndices.y] = TileType.Ground;
            }
            else if (tile.name == "NoBuildSpace")
            {
                tileTypes[tileTypesIndices.x, tileTypesIndices.y] = TileType.NoBuildSpace;
            }
            else if (tile.name == "AdventurerStart")
            {
                tileTypes[tileTypesIndices.x, tileTypesIndices.y] = TileType.AdventurerStart;
            }
            else if (tile.name == "StartButton")
            {
                tileTypes[tileTypesIndices.x, tileTypesIndices.y] = TileType.StartButton;
            }
            else if (tile.name == "Lava")
            {
                tileTypes[tileTypesIndices.x, tileTypesIndices.y] = TileType.Lava;
            }
        }
        
        // remove this when start button works!!
        
        InitEntities();
        
        StartSimulation();
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

    bool IsLava(Vector3Int pos)
    {
        var tileType = GetTileType(pos);

        if (tileType == TileType.Lava)
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

                var entity = go.GetComponent<Entity>();
                entity.pos = pos;
                entity.dir = Vector3Int.right;
                // TODO: change this later. Not everything has health of 3.
                entity.health = 3;
                entities.Add(entity);
            }
            // TODO: maybe check which entity type here and just hard-code attributes like health/maxTicks
            // e.g if entityType == ""Adventurer"
        }

        // return null;
    }

    void InitEntities()
    {
        entities = new List<Entity>();
        
        for (int x = 0; x < tileTypes.GetLength(0); x++)
        {
            for (int y = 0; y < tileTypes.GetLength(1); y++)
            {
                if (tileTypes[x, y] == TileType.AdventurerStart)
                {
                    InstantiateEntity(Entity.EntityType.Adventurer, new Vector3Int(x, y) + gameTilemap.cellBounds.position);
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
        foreach (var e in entities)
        {
            if (IsSolid(e.pos + Vector3Int.down)) // grounded
            {
                // e.waitTicks = e.max
                if (IsSolid(e.pos + e.dir + Vector3Int.up)) // solid at face
                {
                    // switch directions! 
                    if (e.dir == Vector3Int.right)
                    {
                        e.pos += e.dir;
                        e.waitTicksCount = 0;
                        e.dir = Vector3Int.left;
                    }
                    else
                    {
                        e.dir = Vector3Int.right;
                    }
                }
                else // move forward is possible
                {
                    if (IsSolid(e.pos + e.dir)) // jump
                    {
                        if (e.waitTicks == e.waitTicksCount)
                        {
                            e.pos += e.dir + Vector3Int.up;
                            e.waitTicksCount = 0;
                        }
                        else
                        {
                            e.waitTicksCount++;
                        }
                    }
                    else // forward
                    {
                        if (e.waitTicks == e.waitTicksCount)
                        {
                            e.pos += e.dir;
                            e.waitTicksCount = 0;
                        }
                        else
                        {
                            e.waitTicksCount++;
                        }
                    }

                }   
            }
            else if (IsLava(e.pos + Vector3Int.down)) // lava
            {
                // instant death
                e.health = 0;
            }
            else // fall
            {
                // increase speed
                e.pos += Vector3Int.down;
                // 
            }
            if (e.health == 0) {
                Debug.Log("Dead!");
            }

        }
        
    }

}
