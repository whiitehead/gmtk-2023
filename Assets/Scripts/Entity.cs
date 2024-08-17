using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public enum EntityType
{
    Adventurer,
    Goblin,
}

public class Entity : MonoBehaviour
{


    public EntityType Type;
    [FormerlySerializedAs("WaitTicks")] public int MaxWaitTicks = 3;
    public int MaxHealth;
    public int Health;
    public GameObject EmptyHeart;
    public GameObject FullHeart;
    public bool CanDropOffLedge;
    public bool CanClimb;
    public bool CanMove;
    public bool IsSlowAttacker;
    public bool IsSmall; // true -> 1 tile tall, false -> 2 tiles tall
    public bool IsMainCharacter;

    [HideInInspector] public bool IsReadyToAttack = true;
    [HideInInspector] public Vector3Int Dir;
    [HideInInspector] public Vector3Int Pos;
    [HideInInspector] public int DefaultMaxWaitTicks;
    [HideInInspector] public int WaitTicks = 0;
    [HideInInspector] public int FallCount = 0;

    public bool IsDead => Health <= 0;

    private SpriteRenderer spriteRenderer;
    private List<GameObject> heartList;
    private int listHealth;
    private Animator animator;

    private void Start()
    {
        DefaultMaxWaitTicks = MaxWaitTicks;
        spriteRenderer = GetComponent<SpriteRenderer>();
        heartList = new List<GameObject>();
        animator = GetComponent<Animator>();
    }

    public void Hurt(int damage = 1)
    {
        Health -= damage;
    }

    public void Kill()
    {
        Health = 0;
    }

    public void Attack()
    {
        animator.SetTrigger("Attack");
    }

    public void LevelUp(bool healOnLevelUp = true)
    {
        MaxHealth++;
        Health = healOnLevelUp ? MaxHealth : Health + 1;
    }

    public void SwitchDirection()
    {
        Dir = -Dir;
    }

    public void ClimbUp()
    {
        Pos += Dir + Vector3Int.up;
    }
    
    public void ClimbDown()
    {
        Pos += Dir + Vector3Int.down;
    }
    
    public void FallDown()
    {
        FallCount++;
        Pos += Vector3Int.down;
    }

    public void Move(Vector3Int dir)
    {
        animator.SetTrigger("Walk");
        Pos += dir;
    }

    public void MoveForward()
    {
        Move(Dir);
    }
    
    public void LandFromFall()
    {
        if (FallCount < 4) 
        { 
            return;
        }
        
        int damage = (FallCount - 2) / 2;
        
        Hurt(damage);

        FallCount = 0;

        if (damage > 0)
        {
            // audioPlayer.PlaySound("Lose Health");
        }
    }

    // Only put visual stuff in entity update. Game state should only change on GameBoard.Simulate()
    private void Update()
    {
        if (listHealth != Health || MaxHealth != heartList.Count)
        {
            listHealth = Health;
            
            foreach (var go in heartList)
            {
                Destroy(go);
            }
            
            heartList.Clear();

            for (int i = 0; i < MaxHealth; i++)
            {
                var heartPos = new Vector3(((float)i / MaxHealth) * MaxHealth / 2 - (float)MaxHealth / 4, 2f, 0) + transform.position;

                heartList.Add(Instantiate(Health <= i ? EmptyHeart : FullHeart, heartPos, Quaternion.identity,transform));
            }    
        }
        
        spriteRenderer.flipX = Dir != Vector3Int.right;

        var target = new Vector3(Pos.x + 0.5f, Pos.y, 0);
        
        transform.Translate((target - transform.position) * Time.deltaTime / (MaxWaitTicks * GameBoard.TickWaitTime));
        
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawSphere(new Vector3(0.5f + Pos.x, 0.5f + Pos.y, 0), 0.5f);
        
    }
}
