using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using System.Linq;


public class GameBoard : MonoBehaviour
{
    public const float TickWaitTime = 0.15f;
    public Tile buildTile;
    public GameObject[] entityPrefabs;
    public bool HealOnLevelUp;
    
    [HideInInspector] public bool isSimulating = false;
    
    private Tilemap gameTilemap;
    private Tilemap buildTilemap;
    private Grid grid;
    private TileType[,] tileTypes;
    private Camera cam;
    private int tickCount = 0;
    private float timeSinceTick = 0;
    private int requiredKeyCount = 0;
    private int collectedKeyCount = 0;
    private AudioPlayer audioPlayer;
    private List<Entity> entities;
    private const int LeftClick = 0;
    private const int RightClick = 1;


    private enum TileType
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
        cam = Camera.main;
        audioPlayer = GetComponent<AudioPlayer>();
        grid = FindObjectOfType<Grid>();
        
        
        foreach (var tilemap in grid.GetComponentsInChildren<Tilemap>())
        {
            if (tilemap.name == "GameTilemap")
            {
                gameTilemap = tilemap;
            }
            else if (tilemap.name == "BuildTilemap")
            {
                buildTilemap = tilemap;
            }
        }
        
        InitTileTypes();
        InitEntities();
    }

    private void InitTileTypes()
    {
        tileTypes = new TileType[gameTilemap.cellBounds.size.x, gameTilemap.cellBounds.size.y];

        foreach (var pos in gameTilemap.cellBounds.allPositionsWithin)
        {
            var worldPos = gameTilemap.CellToWorld(pos);
            var tile = gameTilemap.GetTile(new Vector3Int((int)worldPos.x, (int)worldPos.y));
            var tileTypesIndices = pos - gameTilemap.cellBounds.position;

            if (tile == null)
            {
                tileTypes[tileTypesIndices.x, tileTypesIndices.y] = TileType.BuildSpace;
                continue;
            }

            switch (tile.name)
            {
                case "BuildSpace":
                    tileTypes[tileTypesIndices.x, tileTypesIndices.y] = TileType.BuildSpace;
                    break;
                case "Ground":
                    tileTypes[tileTypesIndices.x, tileTypesIndices.y] = TileType.Ground;
                    break;
                case "NoBuildSpace":
                    tileTypes[tileTypesIndices.x, tileTypesIndices.y] = TileType.NoBuildSpace;
                    break;
                case "AdventurerStart":
                    tileTypes[tileTypesIndices.x, tileTypesIndices.y] = TileType.AdventurerStart;
                    break;
                case "EnemyStart":
                    tileTypes[tileTypesIndices.x, tileTypesIndices.y] = TileType.EnemyStart;
                    break;
                case "StartButton":
                    tileTypes[tileTypesIndices.x, tileTypesIndices.y] = TileType.StartButton;
                    break;
                case "Lava":
                    tileTypes[tileTypesIndices.x, tileTypesIndices.y] = TileType.Lava;
                    break;
                case "Door":
                    tileTypes[tileTypesIndices.x, tileTypesIndices.y] = TileType.Door;
                    break;
                case "Key":
                    tileTypes[tileTypesIndices.x, tileTypesIndices.y] = TileType.Key;
                    requiredKeyCount++;
                    break;
            }
        }
    }
    
        
    private void InstantiateEntity(EntityType entityType, Vector3Int pos)
    {
        foreach (var prefab in entityPrefabs)
        {
            if (prefab.GetComponent<Entity>().Type == entityType)
            {
                var go = Instantiate(prefab, new Vector3(pos.x, pos.y), Quaternion.identity, grid.transform);
                var entity = go.GetComponent<Entity>();
                entity.Pos = pos;
                entity.Dir = Vector3Int.right;
                entities.Add(entity);
                return;
            }
        }
    }

    void InitEntities()
    {
        if (entities == null)
        {
            entities = new List<Entity>();
        }
        else
        {
            foreach (var entity in entities)
            {
                Destroy(entity.gameObject);
            }
            
            entities.Clear();
        }

        for (int x = 0; x < tileTypes.GetLength(0); x++)
        {
            for (int y = 0; y < tileTypes.GetLength(1); y++)
            {
                if (tileTypes[x, y] == TileType.AdventurerStart)
                {
                    InstantiateEntity(EntityType.Adventurer, new Vector3Int(x, y) + gameTilemap.cellBounds.position);
                }
                if (tileTypes[x, y] == TileType.EnemyStart)
                {
                    InstantiateEntity(EntityType.Goblin, new Vector3Int(x, y) + gameTilemap.cellBounds.position);
                }
            }
        }
    }
    
    void Update()
    {
        var mousePos = cam.ScreenToWorldPoint(Input.mousePosition);
        var pos = grid.WorldToCell(mousePos);
        var tileType = GetTileType(pos);

        if (!isSimulating)
        {
            if (Input.GetMouseButton(LeftClick))
            {
                if (tileType == TileType.BuildSpace)
                {
                    if (!buildTilemap.HasTile(pos))
                    {
                        audioPlayer.PlaySound("Make Bridge");
                        Debug.Log("BUILD!!!");
                        buildTilemap.SetTile(pos, buildTile);
                    }
                }
            }
            else if (Input.GetMouseButton(RightClick))
            {
                if (tileType == TileType.BuildSpace)
                {
                    if (buildTilemap.HasTile(pos))
                    {
                        buildTilemap.SetTile(pos, null);
                    }
                }
            }
        }

        timeSinceTick += Time.deltaTime;
        
        if (isSimulating && TickWaitTime < timeSinceTick)
        {
            timeSinceTick -= TickWaitTime;
            tickCount++;
            Simulate();
        }
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

    Entity GetEntitiy(params Vector3Int[] positions)
    {
        foreach (var e in entities)
        {
            if (!e.IsDead)
            {
                foreach (var pos in positions)
                {
                    if (e.Pos == pos)
                    {
                        return e;
                    }
                }
            }
        }

        return null;
    }
    
    bool IsEntitiy(params Vector3Int[] positions)
    {
        foreach (var e in entities)
        {
            if (!e.IsDead)
            {
                foreach (var pos in positions)
                {
                    if (e.Pos == pos)
                    {
                        return true;
                    }
                }
            }
        }

        return false;
    }

    public void StartSimulation()
    {
        isSimulating = true;
        timeSinceTick = 0;
    }

    public void EndSimulation()
    {
        isSimulating = false;
        collectedKeyCount = 0;
        InitEntities();
    }

    void Simulate()
    {
        foreach (var e in entities)
        {
            if (e.IsDead)
            {
                continue;
            }
            
            if (GetTileType(e.Pos) == TileType.Lava)
            {
                e.Kill();
                continue;
            }
            
            if (e.IsMainCharacter && GetTileType(e.Pos) == TileType.Key)
            {
                audioPlayer.PlaySound("Pickup Item");
                collectedKeyCount++;
            }
            
            if (!IsSolid(e.Pos + Vector3Int.down)) // not grounded
            {
                e.MaxWaitTicks = 1;
                e.FallDown();
                continue;
            }

            // Adjust to being grounded
            e.LandFromFall(); // 0 unless falling
            e.MaxWaitTicks = e.DefaultMaxWaitTicks; // Set movement back to default
            
            if (e.IsDead)
            {
                continue;
            }
            
            if (e.IsMainCharacter && GetTileType(e.Pos) == TileType.Door && collectedKeyCount == requiredKeyCount)
            {
                Debug.Log("WIN");
                return;
            }

            // Attack
            var isFacingEntity = IsEntitiy(e.Pos + e.Dir, e.Pos + e.Dir + Vector3Int.up, e.Pos + e.Dir + Vector3Int.down);
            
            if (!isFacingEntity)
            {
                isFacingEntity = IsEntitiy(e.Pos - e.Dir, e.Pos - e.Dir + Vector3Int.up, e.Pos - e.Dir + Vector3Int.down);
            
                if (isFacingEntity)
                {
                    e.SwitchDirection();
                }
            }
            
            if (isFacingEntity)
            {
                if (e.IsReadyToAttack)
                {
                    e.IsReadyToAttack = false;
                    e.Attack();
                    var combatant = GetEntitiy(e.Pos + e.Dir, e.Pos + e.Dir + Vector3Int.up, e.Pos + e.Dir + Vector3Int.down);
                    combatant.Hurt();
            
                    if (combatant.IsDead)
                    {
                        e.LevelUp(HealOnLevelUp);
                    }
                }
                else
                {
                    e.IsReadyToAttack = true;
                }
                continue;
            }
            
            // Adventurer attacks after monster
            e.IsReadyToAttack = !e.IsSlowAttacker;
            
            // Movement
            // The architecture here is to test tiles from high to low and then break the flow as soon as there is a valid action.
            // This way tiles above are implicitly empty and we dont to check if a tile is empty.
            
            if (e.WaitTicks < e.MaxWaitTicks)
            {
                e.WaitTicks++;
                continue;
            }

            e.WaitTicks = 0;
            
            if (!e.IsSmall && IsSolid(e.Pos + e.Dir + Vector3Int.up)) // face blocked
            {
                e.SwitchDirection();
                continue;
            }

            if (IsSolid(e.Pos + e.Dir))
            {
                if (e.CanClimb)
                {
                    if (e.IsSmall)
                    {
                        e.ClimbUp();
                        continue;
                    }
                    
                    if (!IsSolid(e.Pos + e.Dir + Vector3Int.up * 2))
                    {
                        e.ClimbUp();
                        continue;
                    }
                }
                
                e.SwitchDirection();
                continue;
                
            }

            if (IsSolid(e.Pos + e.Dir + Vector3Int.down))
            {
                e.MoveForward();
                continue;
            }

            if (IsSolid(e.Pos + e.Dir + Vector3Int.down * 2))
            {
                if (e.CanClimb)
                {
                    e.ClimbDown();
                    continue;
                }
            }

            if (e.CanDropOffLedge)
            {
                e.ClimbDown();
                continue;
            }
            
            e.SwitchDirection();
        }
    }
}
