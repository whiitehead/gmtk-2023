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
    public int waitTicks = 3;
    public int maxHealth;
    public int health;
    public GameObject emptyHeart;
    public GameObject fullHeart;

    [HideInInspector] public Vector3Int pos;
    [HideInInspector] public Vector3Int dir;
    [HideInInspector] public int maxWaitTicks;
    [HideInInspector] public int waitTicksCount = 0;
    [HideInInspector] public int fallCount = 0;
    [HideInInspector] public bool isFighting = false;
    [HideInInspector] public Entity fighting = null;
    
    private SpriteRenderer spriteRenderer;
    private List<GameObject> heartList;
    private int listHealth;

    private void Start()
    {
        maxWaitTicks = waitTicks;
        spriteRenderer = GetComponent<SpriteRenderer>();

        heartList = new List<GameObject>();
    }

    private void Update()
    {
        if (listHealth != health || maxHealth != heartList.Count)
        {
            listHealth = health;
            
            foreach (var go in heartList)
            {
                Destroy(go);
            }
            
            heartList.Clear();

            for (int i = 0; i < maxHealth; i++)
            {
                var heartPos = new Vector3(((float)(i) / maxHealth) * maxHealth / 2 - (float)maxHealth / 4, 2f, 0) + transform.position;
                if (health <= i)
                {
                    heartList.Add(Instantiate(emptyHeart, heartPos, Quaternion.identity,transform));
                }
                else
                {
                    heartList.Add(Instantiate(fullHeart, heartPos, Quaternion.identity,transform));
                }
            }    
        }
        
        spriteRenderer.flipX = dir != Vector3Int.right;

        var target = new Vector3(pos.x + 0.5f, pos.y, 0);
        transform.Translate(((target - transform.position).normalized * Time.deltaTime / waitTicks) / GameBoard.tickWaitTime);
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawSphere(new Vector3(0.5f + pos.x, 0.5f + pos.y, 0), 0.5f);
        
    }
}
