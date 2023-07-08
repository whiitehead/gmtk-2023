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
    public int waitTicks = 3;
    public int waitTicksCount = 0;
    public int speed = 1;
    

    public int health;
    public int maxHealth;

    private void OnDrawGizmos()
    {
        Gizmos.DrawSphere(new Vector3(0.5f + pos.x, 0.5f + pos.y, 0), 0.5f);
        
    }
}
