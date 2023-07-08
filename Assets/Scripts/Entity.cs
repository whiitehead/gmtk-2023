using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Entity : MonoBehaviour
{
    public enum EntityType
    {
        Adventurer,
    }

    public EntityType entityType;

    public Vector3Int pos;
}
