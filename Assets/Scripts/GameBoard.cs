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

    private bool adventurerHasKey = true;
    
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

        EnemyStart,

        OutOfBounds,
        Lava,
        Door,
        Key,  
    }
    
    void Start()
    {
        grid = GetComponent<Grid>();
        cam = Camera.main;
        tileTypes = new TileType[gameTilemap.cellBounds.size.x, gameTilemap.cellBounds.size.y];

        var time = Time.time;

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
            else if (tile.name == "EnemyStart")
            {
                tileTypes[tileTypesIndices.x, tileTypesIndices.y] = TileType.EnemyStart;
            }
            else if (tile.name == "StartButton")
            {
                tileTypes[tileTypesIndices.x, tileTypesIndices.y] = TileType.StartButton;
            }
            else if (tile.name == "Lava")
            {
                tileTypes[tileTypesIndices.x, tileTypesIndices.y] = TileType.Lava;
            }
            else if (tile.name == "Door")
            {
                tileTypes[tileTypesIndices.x, tileTypesIndices.y] = TileType.Door;
            }
            else if (tile.name == "Key")
            {
                tileTypes[tileTypesIndices.x, tileTypesIndices.y] = TileType.Key;
                adventurerHasKey = false; // there's a key, the adventurer will need to get it
            }
        }

        Debug.Log(Time.time - time);
        time = Time.time;
        
        // remove this when start button works!!
        
        InitEntities();
        Debug.Log(Time.time - time);
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

    // NOTE: If we don't want so many "IsX" funcs we could generalize to IsType(pos, Type)
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
        return false;
    }

    bool IsDoor(Vector3Int pos)
    {
        var tileType = GetTileType(pos);

        if (tileType == TileType.Door)
        {
            return true;
        }
        return false;
    }
    bool IsKey(Vector3Int pos)
    {
        var tileType = GetTileType(pos);

        if (tileType == TileType.Key)
        {
            return true;
        }
        return false;
    }

    bool IsNonFightingEntity(Vector3Int pos)
    {
        foreach (var e in entities)
        {
            if (pos == e.pos && e.health > 0)
            {
                Debug.Log("Ran into another entity... but didn't fight.");
                return true;
            }
        }
        return false;
    }



    void Update()
    {
        var mousePos = cam.ScreenToWorldPoint(Input.mousePosition);
        var pos = grid.WorldToCell(mousePos);
        var tileType = GetTileType(pos);

        if (Input.GetMouseButton(LEFT_CLICK))
        {
            if (tileType == TileType.BuildSpace)
            {
                if (!buildTilemap.HasTile(pos))
                {
                    buildTilemap.SetTile(pos, buildTile);
                }
            }
        }
        else if (Input.GetMouseButton(RIGHT_CLICK))
        {
            if (tileType == TileType.BuildSpace)
            {
                if (buildTilemap.HasTile(pos))
                {
                    buildTilemap.SetTile(pos, null);
                }
            }
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            Debug.Log("STARTED");
            StartSimulation();
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
                entities.Add(entity);
            }
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
                if (tileTypes[x, y] == TileType.EnemyStart)
                {
                    InstantiateEntity(Entity.EntityType.Goblin, new Vector3Int(x, y) + gameTilemap.cellBounds.position);
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

    int GetFallDamage(int fallCount)
    {
        if (fallCount < 4) 
        { 
            return 0;
        }
        int damage = (fallCount - 2) / 2;
        Debug.Log("Fall damage: " + damage);
        return damage;
    }

    void Simulate()
    {
        foreach (var e in entities)
        {
            if (e.health <= 0)
            {
                Debug.Log(e.entityType + " is still dead!");
            }
            else if (e.isFighting)
            {
                Debug.Log("Combat");
                e.fighting.health = e.fighting.health - 1;
                if (e.fighting.health <= 0)
                {
                    e.isFighting = false;
                    Debug.Log(e.fighting.entityType + "killed in combat dead!");
                }
            }
            // take keys
            if (e.entityType == Entity.EntityType.Adventurer && IsKey(e.pos)) // TODO: ONLY ADVENTURER
            {
                // TODO: will need to disappear the key tile one they hit it
                adventurerHasKey = true;
            }
            // Initiate combat
            if (e.entityType == Entity.EntityType.Adventurer)
            {
                foreach (var scenePartner in entities)
                {
                    if (scenePartner.entityType == Entity.EntityType.Goblin)
                    {
                        if ((scenePartner.pos == (e.pos + e.dir)) && (scenePartner.health > 0))
                        {
                            Debug.Log("Fight initiated");
                            e.isFighting = true;
                            e.fighting = scenePartner;
                            
                            scenePartner.isFighting = true;
                            scenePartner.fighting = e;

                            if (scenePartner.dir == scenePartner.dir)
                            { // make enemies face their death
                                if (scenePartner.dir == Vector3Int.right)
                                {
                                    scenePartner.dir = Vector3Int.left;
                                }
                                else
                                {
                                    scenePartner.dir = Vector3Int.right;
                                }
                            }
                        }
                    }
                }
            }
            // sorry for this condition caleb
            if (IsSolid(e.pos + Vector3Int.down)) // grounded
            {
                if (e.entityType == Entity.EntityType.Adventurer) 
                {
                    e.health = e.health - GetFallDamage(e.fallCount);
                    e.fallCount = 0;
                    if (e.health <= 0) // dead from being grounded
                    {
                        Debug.Log("Dead from fall!");
                    }
                }
                
                if (e.health > 0) // not dead from being grounded
                {
                    e.waitTicks = e.maxWaitTicks; // always move at maxWaitTicks when grounded
                    if (IsSolid(e.pos + e.dir + Vector3Int.up)) // face blocked
                    {
                        // switch directions! 
                        SwitchDirection(e);
                    }
                    // NOTE: this is only to stop multiple enemies from attacking/fighting at once. Enemies can't walk through 
                    // entities (adventurer and other enemies) that they aren't fighting.
                    else if (e.entityType == Entity.EntityType.Goblin && IsNonFightingEntity(e.pos + e.dir))
                    {
                        SwitchDirection(e);
                    }
                    else // move forward is possible
                    {
                        if (IsSolid(e.pos + e.dir)) // jump // TODO: ONLY ADVENTURER
                        {
                            if (e.entityType == Entity.EntityType.Adventurer)
                            {
                                JumpUp(e);
                            }
                            else // enemy hits a jump spot, turn around
                            {
                                SwitchDirection(e);
                            }
                        }
                        else // forward
                        {
                            MoveForward(e);
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
                if (e.entityType == Entity.EntityType.Adventurer)
                {
                    FallDown(e);
                }
                else // enemies don't fall
                {
                    SwitchDirection(e); 
                }
            }


            if (e.entityType == Entity.EntityType.Adventurer)
            {
                if (e.health <= 0) // make sure you don't let a dead adventurer get through door
                {
                    Debug.Log("Dead!");
                }
                else if (IsDoor(e.pos) || IsDoor(e.pos + e.dir)) // TODO: ONLY ADVENTURER
                {
                    if (adventurerHasKey)
                    {
                        Debug.Log("You won, dude.");
                    }
                    else
                    {
                        Debug.Log("Don't have key yet!");
                    }
                }
            } 
        }
        
    }
    // Specific movement logic. In their own functions for (hopefully) readability  
    void SwitchDirection(Entity e)
    {
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
    void JumpUp(Entity e)
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
    void FallDown(Entity e)
    {
        e.waitTicks = 1; // speed up when falling
        e.fallCount++;
        e.pos += Vector3Int.down;
    }
    void MoveForward(Entity e)
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
