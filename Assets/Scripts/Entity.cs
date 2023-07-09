using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Entity : MonoBehaviour
{
    public enum EntityType
    {
        Adventurer,
        Goblin,
    }

    public EntityType entityType;

    public Vector3Int pos;
    public Vector3Int dir;
    [HideInInspector]
    public int maxWaitTicks;
    public int waitTicks = 3;
    public int waitTicksCount = 0;
    public int fallCount = 0;

    public bool isFighting = false;
    public Entity fighting = null;

    public int health;
    public int maxHealth;



    private SpriteRenderer spriteRenderer;

    private void Start()
    {
        maxWaitTicks = waitTicks;
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        spriteRenderer.flipX = dir != Vector3Int.right;

        transform.Translate(((pos - transform.position).normalized * Time.deltaTime / waitTicks) / GameBoard.tickWaitTime);
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawSphere(new Vector3(0.5f + pos.x, 0.5f + pos.y, 0), 0.5f);
        
    }
}
